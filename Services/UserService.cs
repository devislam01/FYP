using DemoFYP.Enums;
using DemoFYP.Exceptions;
using DemoFYP.Models;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;
using Microsoft.IdentityModel.Tokens;

namespace DemoFYP.Services
{
    public class UserService : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly ICommonServices _commonServices;
        private readonly IEmailServices _emailServices;
        private readonly IConfiguration _config;
        private readonly IProductRepository _productRepository;

        public UserService(IUserRepository userRepository, ICommonServices commonServices, IEmailServices emailServices, IConfiguration config, IProductRepository productRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
            _emailServices = emailServices ?? throw new ArgumentNullException(nameof(emailServices));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        #region Read Services

        public async Task<UserJwtClaims> CheckLoginCredentials(UserLoginRequest payload)
        {
            if (payload == null) throw new BadRequestException("Payload is required.");

            try
            {
                var JwtClaim = await _userRepository.CheckUserLoginCredentials(payload);

                return JwtClaim ?? throw new NotFoundException("Invalid JwtClaims");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Guid> CheckIsLogin(string refreshToken)
        {
            return await _userRepository.CheckIfUserLogin(refreshToken);
        }

        public async Task<UserDetailResponse> GetUserProfile(Guid curUserID)
        {
            if (curUserID == Guid.Empty) throw new UnauthorizedAccessException();

            try
            {
                return await _userRepository.GetUserProfileByLoginID(curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task<UserPermissionResponse> GetPermissionsList()
        {
            try
            {
                return await _userRepository.GetPermissions() ?? throw new NotFoundException("Permission List is Empty");
            }
            catch
            {
                throw;
            }
        }

        public async Task<UserPermissionResponse> GetAdminPermissionsList()
        {
            try
            {
                return await _userRepository.GetAdminPermissions() ?? throw new NotFoundException("Permission List is Empty");
            }
            catch
            {
                throw;
            }
        }

        public async Task<UserPermissionResponse> GetUserPermissions(Guid curUserID)
        {
            if (curUserID == Guid.Empty) throw new UnauthorizedAccessException();

            try
            {
                return await _userRepository.GetUserPermissionsByLoginID(curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task<PagedResult<UserListResponse>> GetUserList(UserListFilterRequest filter)
        {
            try
            {
                return await _userRepository.GetUserList(filter);
            }
            catch
            {
                throw;
            }
        }

        public async Task<EditUserDetailsResponse> GetUserDetails(Guid userID)
        {
            try
            {
                return await _userRepository.GetUserDetails(userID);
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Create Services

        public async Task RegisterUser(UserRegisterRequest payload, Guid updatedBy)
        {
            if (payload == null) throw new BadRequestException("Payload is required.");
            if (payload.Email == null) throw new BadRequestException("Email is required.");
            if (payload.Password == null) throw new BadRequestException("Password is required.");

            try
            {
                var hasUser = await _userRepository.CheckIfEmailExist(payload.Email);

                if (hasUser) throw new ConflictException("User already exist.");

                if (payload.QRCode != null)
                {
                    payload.PaymentQRCode = await _commonServices.UploadImage(payload.QRCode, "", FolderName.PaymentQR.ToString());
                }

                await _userRepository.RegisterUser(payload, updatedBy);
            }
            catch (Exception) {
                throw;
            }
        }

        #endregion

        #region Update Services

        public async Task UpdateUserProfile(UserUpdateDetailRequest payload, Guid curUserID)
        {
            if (payload == null) throw new BadRequestException("Payload is required");

            try
            {
                if (payload.QRCode != null)
                {
                    payload.PaymentQRCode = await _commonServices.UploadImage(payload.QRCode, "", FolderName.PaymentQR.ToString());
                }

                await _userRepository.UpdateUserProfile(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task SendTemporilyPassword(string email, Guid curUserID)
        {
            if (email == null) throw new BadRequestException("Email is required");

            try
            {
                var temporilyPassword = _commonServices.GenerateTemporaryPassword();
                string hashPassword = _commonServices.HashPassword(temporilyPassword);
                await _userRepository.UpdateTempPassword(email, curUserID, hashPassword);

                string subject = "[Test] Reset Password";
                string body = $"Your temporary password is: {temporilyPassword}\n\nPlease login and change it immediately with the link {_config["FrontendUrl"]}";

                await _emailServices.SendEmailAsync(email, subject, body);
            }
            catch
            {
                throw;
            }
        }

        public async Task ResetPassword(ResetPasswordRequest payload, Guid curUserID)
        {
            if (payload.Password == null) throw new BadRequestException("New Password is required");

            try
            {
                var hashPassword = _commonServices.HashPassword(payload.Password);
                var email =await _userRepository.UpdatePassword(curUserID, hashPassword);

                string subject = "Reset Password";
                string body = $"Your password is reset. Please contact us if you didn't made this action";

                await _emailServices.SendEmailAsync(email, subject, body);
            }
            catch
            {
                throw;
            }
        }

        public async Task ResetPassword(AdminResetPasswordRequest payload, Guid curUserID)
        {
            if (payload.UserID == Guid.Empty) throw new BadRequestException("User ID is required");
            if (payload.Password == null) throw new BadRequestException("New Password is required");

            try
            {
                var hashPassword = _commonServices.HashPassword(payload.Password);
                var email = await _userRepository.UpdatePassword(payload.UserID, curUserID, hashPassword);

                string subject = "Reset Password by Admin";
                string body = $"Your password is reset by admin, please check with admin and get your latest password.";

                await _emailServices.SendEmailAsync(email, subject, body);
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
