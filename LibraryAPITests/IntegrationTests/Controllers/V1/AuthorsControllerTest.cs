using LibraryAPI.Models.Entities;
using LibraryAPI.Models.Requests;
using LibraryAPI.Models.Responses;
using LibraryAPITests.Utilities;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace LibraryAPITests.IntegrationTests.Controllers.V1
{
    [TestClass]
    public class AuthorsControllerTest: TestBase
    {
        private static readonly string url = "/api/v1/authors";
        private string databaseName = Guid.NewGuid().ToString();

        [TestMethod]
        public async Task GetById_Returns404_WhenAuthorDoesNotExists()
        {
            // Preparation 
            
            var factory = BuildWebApplicationFactory(databaseName);
            var client = factory.CreateClient();

            // Test

            var response = await client.GetAsync($"{url}/800");

            // Validation

            var statusCode = response.StatusCode;
            Assert.AreEqual(expected: HttpStatusCode.NotFound, actual: statusCode);
        }

        [TestMethod]
        public async Task GetById_ReturnsAuthor_WhenAuthorExists()
        {
            // Preparation 

            var context = BuildContext(databaseName);

            context.Authors.Add(new AuthorEntity() { Names = "Juan", LastNames = "Suarez" });
            context.Authors.Add(new AuthorEntity() { Names = "Felipe", LastNames = "Gavilan" });

            await context.SaveChangesAsync();

            var factory = BuildWebApplicationFactory(databaseName);
            var client = factory.CreateClient();

            // Test

            var response = await client.GetAsync($"{url}/1");

            // Validation

            response.EnsureSuccessStatusCode();

            var author = JsonSerializer.Deserialize<AuthorWithBooksResponse>(
                await response.Content.ReadAsStringAsync(), jsonSerializerOptions
                )!;

            Assert.AreEqual(expected: 1, actual: author.Id);

        }

        [TestMethod]
        public async Task Create_Returns401_WhenUserIsNotAuthenticated()
        {
            // Preparation

            var factory = BuildWebApplicationFactory(databaseName, false);
            var client = factory.CreateClient();
            var authorRequest = new AuthorRequest
            {
                Names = "Juan",
                LastNames = "Fernandez"
            };

            // Test

            var response = await client.PostAsJsonAsync(url, authorRequest);

            // Validation

            Assert.AreEqual(expected: HttpStatusCode.Unauthorized, actual: response.StatusCode);
        }

        [TestMethod]
        public async Task Create_Returns403_WhenUserIsNotAdmin()
        {
            // Preparation

            var factory = BuildWebApplicationFactory(databaseName, false);
            var token = await BuildUser(databaseName, factory);
            var client = factory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var authorRequest = new AuthorRequest
            {
                Names = "Juan",
                LastNames = "Fernandez"
            };

            // Test

            var response = await client.PostAsJsonAsync(url, authorRequest);

            // Validation

            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: response.StatusCode);
        }

        [TestMethod]
        public async Task Create_Returns201_WhenUserIsAdmin()
        {
            // Preparation

            var factory = BuildWebApplicationFactory(databaseName, false);
            var claims = new List<Claim> { adminClaim };
            var token = await BuildUser(databaseName, factory, claims);
            var client = factory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var authorRequest = new AuthorRequest
            {
                Names = "Juan",
                LastNames = "Fernandez"
            };

            // Test

            var response = await client.PostAsJsonAsync(url, authorRequest);

            // Validation

            response.EnsureSuccessStatusCode();

            Assert.AreEqual(expected: HttpStatusCode.Created, actual: response.StatusCode);
        }
    }
}
