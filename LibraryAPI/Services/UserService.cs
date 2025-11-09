using LibraryAPI.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Services
{
    public class UserService: IUserService
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;
        public UserService(UserManager<UserEntity> userManager, 
            IHttpContextAccessor httpContext)
        {
            _userManager = userManager;
            _contextAccessor = httpContext;
        }

        public async Task<UserEntity?> GetUser()
        {
            var emailClaim = _contextAccessor.HttpContext!.User.Claims
                .Where(claim => claim.Type == "email")
                .FirstOrDefault();

            if(emailClaim is null)
            {
                return null;
            }

            var email = emailClaim.Value;
            return await _userManager.FindByEmailAsync(email);
        }
    }
}
