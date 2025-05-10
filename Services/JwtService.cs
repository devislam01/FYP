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

        public async Task<JwtAuthResult> GenerateToken(UserLoginRequest payload, byte[] curUserID)
        {
            if (payload == null) throw new BadRequestException("User request is required");

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, curUserID.ToString()),
                new Claim(ClaimTypes.Email, payload.Email),
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            var refreshToken = GenerateSecureRefreshToken();

            Usertoken userToken = new Usertoken
            {
                UserId = curUserID,
                AccessToken = jwtToken,
                AccessTokenExpiresAt = token.ValidTo,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiresAt = refreshToken.Expiry,
            };

            try
            {
                await _jwtRepository.AddUserToken(userToken);

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

        private RefreshToken GenerateSecureRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            return new RefreshToken {
                Token = Convert.ToBase64String(randomBytes),
                Expiry = DateTime.UtcNow.AddDays(7)
            };
        }
    }
}
