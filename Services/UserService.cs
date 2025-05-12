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

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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

        public async Task UpdateUserProfile(UserUpdateDetailRequest payload)
        {
            if (payload == null) throw new BadRequestException("Payload is required");
            if (payload.UserID == Guid.Empty) throw new BadRequestException("User ID is required");

            try
            {
                await _userRepository.UpdateUserProfile(payload);
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }
}
