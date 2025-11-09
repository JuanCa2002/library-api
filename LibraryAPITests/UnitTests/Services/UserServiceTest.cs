using LibraryAPI.Models.Entities;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using System.Security.Claims;

namespace LibraryAPITests.UnitTests.Services
{
    [TestClass]
    public class UserServiceTest
    {
        private UserManager<UserEntity> userManager = null!;
        private IHttpContextAccessor httpContextAccessor = null!;
        private IUserService userService = null!;

        [TestInitialize]
        public void SetUp()
        {
            userManager = Substitute.For<UserManager<UserEntity>>(
                Substitute.For<IUserStore<UserEntity>>(), null, null, null, null, null, null,
                null, null);

            httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            userService = new UserService(userManager, httpContextAccessor);
        }

        [TestMethod]
        public async Task GetUser_ReturnNull_WhenThereIsNotEmailClaim()
        {
            // Preparation

            var httpContext = new DefaultHttpContext();
            httpContextAccessor.HttpContext.Returns(httpContext);

            // Test

            var user = await userService.GetUser();

            // Validation

            Assert.IsNull(user, "User has to be null");
        }


        [TestMethod]
        public async Task GetUser_ReturnUser_WhenThereIsEmailClaim()
        {
            // Preparation

            var email = "test@gmail.com";
            var expectedUser = new UserEntity { Email = email };

            userManager
                .FindByEmailAsync(email)!
                .Returns(Task.FromResult(expectedUser));

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpContext = new DefaultHttpContext() { User = claims};
            httpContextAccessor.HttpContext.Returns(httpContext);

            // Test

            var user = await userService.GetUser();

            // Validation

            Assert.IsNotNull(user, "User must not be null");
            Assert.AreEqual(expected: email, actual: user.Email);
        }

        [TestMethod]
        public async Task GetUser_ReturnNull_WhenUserDoesNotExists()
        {
            // Preparation

            var email = "test@gmail.com";
            var expectedUser = new UserEntity { Email = email };

            userManager
                .FindByEmailAsync(email)!
                .Returns(Task.FromResult<UserEntity>(null!));

            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("email", email)
            }));

            var httpContext = new DefaultHttpContext() { User = claims };
            httpContextAccessor.HttpContext.Returns(httpContext);

            // Test

            var user = await userService.GetUser();

            // Validation

            Assert.IsNull(user, "User has to be null");
        }
    }
}
