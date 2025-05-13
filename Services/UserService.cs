using DemoFYP.Exceptions;
using DemoFYP.Models.Dto.Request;
using DemoFYP.Models.Dto.Response;
using DemoFYP.Repositories.IRepositories;
using DemoFYP.Services.IServices;

namespace DemoFYP.Services
{
    public class UserService : IUserServices
    {
        private readonly IUserRepository _userRepository;
        private readonly ICommonServices _commonServices;
        private readonly IEmailServices _emailServices;
        private readonly IConfiguration _config;

        public UserService(IUserRepository userRepository, ICommonServices commonServices, IEmailServices emailServices, IConfiguration config)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
            _emailServices = emailServices ?? throw new ArgumentNullException(nameof(emailServices));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        #region Read Services

        public async Task<Guid> CheckLoginCredentials(UserLoginRequest payload)
        {
            if (payload == null) throw new BadRequestException("Payload is required.");

            try
            {
                var hasUserID = await _userRepository.CheckUserLoginCredentials(payload);

                if (hasUserID == Guid.Empty) throw new BadRequestException("Invalid Email or Password");

                return hasUserID;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<UserDetailResponse> GetUserProfile(Guid CurUserID)
        {
            if (CurUserID == Guid.Empty) throw new UnauthorizedAccessException();

            try
            {
                return await _userRepository.GetUserProfileByLoginID(CurUserID);
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
                await _userRepository.UpdateUserProfile(payload, curUserID);
            }
            catch
            {
                throw;
            }
        }

        public async Task SendTemporilyPassword(string email, Guid CurUserID)
        {
            if (email == null) throw new BadRequestException("Email is required");

            try
            {
                var temporilyPassword = _commonServices.GenerateTemporaryPassword();
                string hashPassword = _commonServices.HashPassword(temporilyPassword);
                await _userRepository.UpdatePassword(email, CurUserID, hashPassword);

                string subject = "[Test] Reset Password";
                string body = $"Your temporary password is: {temporilyPassword}\n\nPlease login and change it immediately with the link {_config["FrontendUrl"]}";

                await _emailServices.SendEmailAsync(email, temporilyPassword, subject, body);
            }
            catch
            {
                throw;
            }
        }

        public async Task ResetPassword(ResetPasswordRequest payload, Guid CurUserID)
        {
            if (payload.Email == null) throw new BadRequestException("Email is required");
            if (payload.Password == null) throw new BadRequestException("New Password is required");

            try
            {
                var hashPassword = _commonServices.HashPassword(payload.Password);
                await _userRepository.UpdatePassword(payload.Email, CurUserID, hashPassword);
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
