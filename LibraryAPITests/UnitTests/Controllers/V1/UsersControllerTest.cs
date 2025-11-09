using LibraryAPI.Controllers.V1;
using LibraryAPI.Models.Entities;
using LibraryAPI.Models.Requests;
using LibraryAPI.Services;
using LibraryAPITests.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace LibraryAPITests.UnitTests.Controllers.V1
{
    [TestClass]
    public class UsersControllerTest: TestBase
    {
        private UserManager<UserEntity> userManager = null!;
        private SignInManager<UserEntity> signInManager = null!;
        private UsersController controller = null!;
        private string databaseName = Guid.NewGuid().ToString();

        [TestInitialize]
        public void SetUp()
        {
            var context = BuildContext(databaseName);
            userManager = Substitute.For<UserManager<UserEntity>>(
                Substitute.For<IUserStore<UserEntity>>(), null, null, null, null, null, null,
                null, null);

            var myConfiguration = new Dictionary<string, string>
            {
                {
                    "jwtKey", "TJA0IJDJJFadaDJIJFJ0FSDIAJFOJWAFOAWFOMJFPOWAJFPOAJWF"
                }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration!)
                .Build();

            var httpContextAccesor = Substitute.For<IHttpContextAccessor>();
            var userClaimsFactory = Substitute.For<IUserClaimsPrincipalFactory<UserEntity>>();

            signInManager = Substitute.For<SignInManager<UserEntity>>(userManager,
                httpContextAccesor, userClaimsFactory ,null, null, null, null);

            var userService = Substitute.For<IUserService>();
            var mapper = ConfigureAutoMapper();

            controller = new UsersController(userManager, configuration, 
                signInManager, userService, context, mapper);
        }

        [TestMethod]
        public async Task Register_ReturnValidationProblem_WhenIsNotSuccessfully()
        {
            // Preparation

            var errorMessage = "test";
            var userCredentials = new UserCredentialsRequest
            {
                Email = "test@email.com",
                Password = "aA123456!"
            };

            userManager.CreateAsync(Arg.Any<UserEntity>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed(new IdentityError
                {
                    Code = "test",
                    Description = errorMessage
                }));

            // Test

            var response = await controller.Register(userCredentials);

            // Validation

            var result = response.Result as ObjectResult;
            var problemDetails = result!.Value as ValidationProblemDetails;

            Assert.IsNotNull(result, "Expected an ObjectResult");
            Assert.IsNotNull(problemDetails, "Expected an ValidationProblemDetails");
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: errorMessage, actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Register_ReturnToken_WhenIsSuccessfully()
        {
            // Preparation

            var userCredentials = new UserCredentialsRequest
            {
                Email = "test@email.com",
                Password = "aA123456!"
            };

            userManager.CreateAsync(Arg.Any<UserEntity>(), Arg.Any<string>())
                .Returns(IdentityResult.Success);

            // Test

            var response = await controller.Register(userCredentials);

            // Validation

            Assert.IsNotNull(response.Value);
            Assert.IsNotNull(response.Value.Token);
        }

        [TestMethod]
        public async Task Login_ReturnsValidationProblem_WhenUserDoesNotExists()
        {
            // Preparation

            var userCredentials = new UserCredentialsRequest
            {
                Email = "test@email.com",
                Password = "aA123456!"
            };

            userManager.FindByEmailAsync(userCredentials.Email)!
                .Returns(Task.FromResult<UserEntity>(null!));

            // Test

            var response = await controller.Login(userCredentials);

            // Validation

            var result = response.Result as ObjectResult;
            var problemDetails = result!.Value as ValidationProblemDetails;

            Assert.IsNotNull(result, "Expected an ObjectResult");
            Assert.IsNotNull(problemDetails, "Expected an ValidationProblemDetails");
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: "Invalid Login", actual: problemDetails.Errors.Values.First().First());

        }

        [TestMethod]
        public async Task Login_ReturnsValidationProblem_WhenCredentialsAreIncorrect()
        {
            // Preparation

            var userCredentials = new UserCredentialsRequest
            {
                Email = "test@email.com",
                Password = "aA123456!"
            };

            var user = new UserEntity
            {
                Email = "test@email.com"
            };

            userManager.FindByEmailAsync(userCredentials.Email)!
                .Returns(Task.FromResult<UserEntity>(user));

            signInManager.CheckPasswordSignInAsync(user, userCredentials.Password, false)
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            // Test

            var response = await controller.Login(userCredentials);

            // Validation

            var result = response.Result as ObjectResult;
            var problemDetails = result!.Value as ValidationProblemDetails;

            Assert.IsNotNull(result, "Expected an ObjectResult");
            Assert.IsNotNull(problemDetails, "Expected an ValidationProblemDetails");
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: "Invalid Login", actual: problemDetails.Errors.Values.First().First());

        }

        [TestMethod]
        public async Task Login_ReturnsToken_WhenLoginIsSuccessfully()
        {
            // Preparation

            var userCredentials = new UserCredentialsRequest
            {
                Email = "test@email.com",
                Password = "aA123456!"
            };

            var user = new UserEntity
            {
                Email = "test@email.com"
            };

            userManager.FindByEmailAsync(userCredentials.Email)!
                .Returns(Task.FromResult<UserEntity>(user));

            signInManager.CheckPasswordSignInAsync(user, userCredentials.Password, false)
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Test

            var response = await controller.Login(userCredentials);

            // Validation

            Assert.IsNotNull(response.Value);
            Assert.IsNotNull(response.Value.Token);

        }
    }
}
