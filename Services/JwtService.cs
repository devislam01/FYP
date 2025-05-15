using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DemoFYP.Services
{
    public class JwtService : IJwtServices
    {
        private readonly string _key;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly IJwtRepository _jwtRepository;

        public JwtService(IConfiguration configuration, IJwtRepository jwtRepository) {
            _key = configuration["Jwt:Key"] ?? throw new ArgumentNullException("Missing JWT Key in Config");
            _issuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Missing JWT Issuer in Config");
            _audience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Missing JWT Audience in Config");
            _jwtRepository = jwtRepository ?? throw new ArgumentNullException(nameof(jwtRepository));
        }

        public async Task<JwtAuthResult> GenerateToken(UserJwtClaims userClaims)
        {
            if (userClaims == null) throw new BadRequestException("User request is required");

            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, userClaims.UserID.ToString()),
                new Claim(ClaimTypes.Email, userClaims.Email),
                new Claim(ClaimTypes.Role, userClaims.Role)
            };

            foreach (var permission in userClaims.Permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: creds
            );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerateSecureRefreshToken();

            Usertoken userToken = new Usertoken
            {
                UserId = userClaims.UserID,
                AccessToken = jwtToken,
                AccessTokenExpiresAt = DateTime.Now.AddMinutes(15),
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiresAt = refreshToken.Expiry,
            };

            try
            {
                await _jwtRepository.AddOrUpdateUserToken(userToken);

                return new JwtAuthResult
                {
                    accessToken = jwtToken,
                    RefreshToken = refreshToken.Token,
                };
            }
            catch (Exception ex) {
                throw;
            }
        }

        public async Task<JwtAuthResult> VerifyAndGenerateRefreshToken(RefreshTokenRequest payload, string curUserEmail, string curUserRole)
        {
            if (string.IsNullOrEmpty(payload.RefreshToken)) throw new BadRequestException("Refresh token is required");

            var userToken = await _jwtRepository.GetUserTokenByRefreshToken(payload.RefreshToken) ?? throw new UnauthorizedAccessException("Invalid or expired refresh token");
            var userId = userToken.UserId;

            await _jwtRepository.RevokeUserTokenByRefreshToken(payload.RefreshToken);

            var jwtResult = await GenerateToken(new UserJwtClaims
            {
                UserID = userId,
                Email = curUserEmail,
                Role = curUserRole,
            });

            return jwtResult;
        }

        public async Task RevokeUser(Guid userID, Guid curUserID)
        {
            if (userID == Guid.Empty) throw new BadRequestException("User ID is required");

            try
            {
                await _jwtRepository.RevokeUserByUserID(userID, curUserID);
            }
            catch
            {
                throw;
            }
        }

        private static RefreshToken GenerateSecureRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return new RefreshToken {
                Token = Convert.ToBase64String(randomBytes),
                Expiry = DateTime.Now.AddDays(7)
            };
        }
    }
}
