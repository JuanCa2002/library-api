

using LibraryAPI.Controllers.V1;
using LibraryAPI.Models.Requests;
using LibraryAPI.Models.Responses;
using LibraryAPITests.Utilities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LibraryAPITests.UnitTests.Controllers.V1
{
    [TestClass]
    public class BookControllerTest: TestBase
    {
        ILogger<BookController>? logger = null;
        IOutputCacheStore? outputCacheStore = null;
        IDataProtectionProvider? protectionProvider = null;
        private BookController? controller = null;
        private const string cache = "get-books";
        private string databaseName = Guid.NewGuid().ToString();

        [TestInitialize]
        public void SetUp()
        {
            // Preparation

            var context = BuildContext(databaseName);
            var mapper = ConfigureAutoMapper();
            logger = Substitute.For<ILogger<BookController>>();
            outputCacheStore = Substitute.For<IOutputCacheStore>();
            protectionProvider = Substitute.For<IDataProtectionProvider>();
            controller = new BookController(context, mapper, protectionProvider, 
                outputCacheStore);
        }

        [TestMethod]
        public async Task GetAll_DoesNotReturnBooks_WhenThereAreNotBooks()
        {
            // Preparation

            var pagination = new PaginationRequest(1, 10);

            // Test

            controller!.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var response = await controller!.GetAll(pagination);

            // Validation

            var result = response.Result as OkObjectResult;
            Assert.IsNotNull(result, "Expected an OkObjectResult");
            Assert.AreEqual(expected: 200, actual: result.StatusCode);

            var value = result.Value as IEnumerable<BookSimpleResponse>;
            Assert.IsNotNull(value, "Expected an IEnumerable<BookSimpleResponse>");
            Assert.AreEqual(expected: 0, actual: value.Count());
        }
    }
}
