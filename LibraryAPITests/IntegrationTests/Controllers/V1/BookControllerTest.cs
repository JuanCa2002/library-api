using LibraryAPI.Models.Requests;
using LibraryAPITests.Utilities;
using System.Net;

namespace LibraryAPITests.IntegrationTests.Controllers.V1
{
    [TestClass]
    public class BookControllerTest: TestBase
    {
        private readonly string url = "/api/v1/books";
        private string databaseName = Guid.NewGuid().ToString();

        [TestMethod]
        public async Task Create_Returns400_WhenAuthorsIdsIsEmpty()
        {
            // Preparation

            var factory = BuildWebApplicationFactory(databaseName);
            var client = factory.CreateClient();
            var bookRequest = new BookRequest
            {
                Title = "The hunger games",
                AuthorIds = []
            };

            // Test

            var response = await client.PostAsJsonAsync(url, bookRequest);

            // Validation

            Assert.AreEqual(expected: HttpStatusCode.BadRequest, actual: response.StatusCode);
        }
    }
}
