using LibraryAPI.Controllers.V1;
using LibraryAPI.Models.Entities;
using LibraryAPI.Models.Requests;
using LibraryAPI.Models.Responses;
using LibraryAPI.Services;
using LibraryAPITests.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.JsonPatch.Operations;
using NSubstitute;

namespace LibraryAPITests.UnitTests.Controllers.V1
{
    [TestClass]
    public class AuthorsControllerTest: TestBase
    {
        IStorageFiles? storageFiles = null;
        ILogger<AuthorsController>? logger = null;
        IOutputCacheStore? outputCacheStore = null;
        private AuthorsController? controller = null;
        private const string container = "authors";
        private const string cache = "get-authors";
        private string databaseName = Guid.NewGuid().ToString();

        [TestInitialize]
        public void SetUp()
        {
            // Preparation

            var context = BuildContext(databaseName);
            var mapper = ConfigureAutoMapper();
            storageFiles = Substitute.For<IStorageFiles>();
            logger = Substitute.For<ILogger<AuthorsController>>();
            outputCacheStore = Substitute.For<IOutputCacheStore>();
            controller = new AuthorsController(context, mapper, logger,
                storageFiles, outputCacheStore);
        }

        [TestMethod]
        public async Task GetById_Return404_WhenAuthorByIdDoesNotExists()
        {   
            // Test

            var response = await controller!.GetById(1);
            var result = response.Result as StatusCodeResult;

            // Validation

            Assert.AreEqual(expected: 404, actual: result!.StatusCode);
        }

        [TestMethod]
        public async Task GetById_ReturnAuthor_WhenAuthorByIdExists()
        {
            // Preparation

            var context = BuildContext(databaseName);

            context.Authors.Add(new AuthorEntity
            {
                Names = "Juan Camilo",
                LastNames = "Torres Beltrán"
            });

            context.Authors.Add(new AuthorEntity
            {
                Names = "Leonel Andres",
                LastNames = "Messi"
            });

            await context.SaveChangesAsync();

            // Test

            var response = await controller!.GetById(1);

            // Validation

            var okResult = response.Result as OkObjectResult;
            Assert.IsNotNull(okResult, "Expected an OkObjectResult");
            Assert.AreEqual(200, okResult.StatusCode);

            var result = okResult.Value as AuthorWithBooksResponse;
            Assert.IsNotNull(result, "Expected a valid AuthorDTO");
            Assert.AreEqual(1, result.Id);
        }

        [TestMethod]
        public async Task GetAll_MustReturnAllAuthors_WithPagination()
        {
            // Preparation

            var context = BuildContext(databaseName);

            context.Authors.Add(new AuthorEntity
            {
                Names = "Juan Camilo",
                LastNames = "Torres Beltrán"
            });

            context.Authors.Add(new AuthorEntity
            {
                Names = "Leonel Andres",
                LastNames = "Messi"
            });

            await context.SaveChangesAsync();

            controller!.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var pagination = new PaginationRequest(1, 3);

            // Test

            var response = await controller.GetAll(pagination);

            // Validation

            var result = response.Result as OkObjectResult;
            Assert.IsNotNull(result, "Expected an CreatedAtRouteResult");
            Assert.AreEqual(expected: 200, actual: result.StatusCode);

            var value = result.Value as IEnumerable<AuthorResponse>;
            Assert.IsNotNull(value, "Expected an IEnumerable<AuthorResponse>");
            Assert.AreEqual(expected: 2, value.Count());

            logger!.Received(1).LogInformation("Getting Authors List");

        }

        [TestMethod]
        public async Task Create_MustCreateAuthor_WhenAuthorIsSend()
        {
            // Preparation

            var newAuthor = new AuthorRequest
            {
                Names = "New",
                LastNames = "Author"
            };

            // Test

            var response = await controller!.Create(newAuthor);

            // Validation

            var resultAtRoute = response.Result as CreatedAtRouteResult;
            Assert.IsNotNull(resultAtRoute, "Expected an CreatedAtRouteResult");
            Assert.AreEqual(expected: 201, actual: resultAtRoute.StatusCode);

            var secondContex = BuildContext(databaseName);
            var quantity = await secondContex.Authors.CountAsync();
            Assert.AreEqual(expected: 1, actual: quantity);

        }

        [TestMethod]
        public async Task Update_Return404_WhenAuthorDoesNotExists()
        {

            // Test

            var emptyAuthorUpdateRequest = new AuthorUpdateRequest
            {
                Id = 1,
                Names = "NONE",
                LastNames = "NONE"
            };
            var response = await controller!.Update(request: emptyAuthorUpdateRequest);

            // Validation

            var result = response.Result as StatusCodeResult;
            Assert.IsNotNull(result, "Expected an StatusCodeResult");
            Assert.AreEqual(expected: 404, result.StatusCode);
        }

        [TestMethod]
        public async Task Update_UpdateAuthor_WhenItDoesNotHavePicture()
        {
            // Preparation

            var context = BuildContext(databaseName);

            context.Authors.Add(new AuthorEntity
            {
                Names = "Juan",
                LastNames = "Alesso"
            });

            await context.SaveChangesAsync();

            // Test

            var updatedAuthor = new AuthorUpdateRequest
            {
                Id = 1,
                Names = "Pedro",
                LastNames = "Sanchez"
            };
            var response = await controller!.Update(request: updatedAuthor);

            // Validation

            var result = response.Result as OkObjectResult;
            Assert.IsNotNull(result, "Expected an StatusCodeResult");
            Assert.AreEqual(expected: 200, result.StatusCode);

            var value = result.Value as AuthorResponse;
            Assert.IsNotNull(value, "Expected an AuthorResponse");
            Assert.AreEqual(expected: "Pedro Sanchez", actual: value.FullName);

            await outputCacheStore!.Received(1).EvictByTagAsync(cache, default);
            await storageFiles!.DidNotReceiveWithAnyArgs().Edit(default, default!, default!);
        }

        [TestMethod]
        public async Task Update_UpdateAuthor_WhenItHasPicture()
        {
            // Preparation

            var context = BuildContext(databaseName);

            var previousUrl = "URL-1";
            var newUrl = "NEW-URL";

            storageFiles!.Edit(default, default!, default!)
                .ReturnsForAnyArgs(newUrl);

            context.Authors.Add(new AuthorEntity
            {
                Names = "Juan",
                LastNames = "Alesso",
                Picture = previousUrl
            });

            await context.SaveChangesAsync();

            // Test

            var formFile = Substitute.For<IFormFile>();

            var updatedAuthor = new AuthorUpdateRequest
            {
                Id = 1,
                Names = "Pedro",
                LastNames = "Sanchez",
                Picture = formFile
            };
            var response = await controller!.Update(request: updatedAuthor);

            // Validation

            var result = response.Result as OkObjectResult;
            Assert.IsNotNull(result, "Expected an StatusCodeResult");
            Assert.AreEqual(expected: 200, result.StatusCode);

            var value = result.Value as AuthorResponse;
            Assert.IsNotNull(value, "Expected an AuthorResponse");
            Assert.AreEqual(expected: "Pedro Sanchez", actual: value.FullName);
            Assert.AreEqual(expected: newUrl, actual: value.Picture);

            await outputCacheStore!.Received(1).EvictByTagAsync(cache, default);
            await storageFiles!.Received(1).Edit(previousUrl, container, formFile);
        }

        [TestMethod]
        public async Task Patch_Return400_WhenDocumentIsNull()
        {
            // Test

            var response = await controller!.Patch(1, document: null!);

            // Validation

            var result = response as StatusCodeResult;
            Assert.IsNotNull(result, "Expected an StatusCodeResult");
            Assert.AreEqual(expected: 400, result.StatusCode);
        }

        [TestMethod]
        public async Task Patch_Return404_WhenAuthorDoesNotExists()
        {
            // Preparation

            var document = new JsonPatchDocument<AuthorPatchRequest>();

            // Test

            var response = await controller!.Patch(1, document);

            // Validation

            var result = response as StatusCodeResult;
            Assert.IsNotNull(result, "Expected an StatusCodeResult");
            Assert.AreEqual(expected: 404, result.StatusCode);
        }

        [TestMethod]
        public async Task Patch_ReturnValidationProblem_WhenThereIsValidationErrors()
        {
            // Preparation

            var context = BuildContext(databaseName);
            context.Authors.Add(new AuthorEntity { Names = "Felipe", LastNames = "Giraldo" });
            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller!.ObjectValidator = objectValidator;

            var errorMessage = "error message";
            controller.ModelState.AddModelError("", errorMessage);

            var document = new JsonPatchDocument<AuthorPatchRequest>();

            // Test

            var response = await controller!.Patch(1, document);

            // Validation

            var result = response as ObjectResult;
            var problemDetails = result!.Value as ValidationProblemDetails;

            Assert.IsNotNull(result, "Expected an ObjectResult");
            Assert.IsNotNull(problemDetails, "Expected an ValidationProblemDetails");
            Assert.AreEqual(expected: 1, actual: problemDetails.Errors.Keys.Count);
            Assert.AreEqual(expected: errorMessage, actual: problemDetails.Errors.Values.First().First());
        }

        [TestMethod]
        public async Task Patch_UpdateAField_WhenItSendsAnOperation()
        {
            // Preparation

            var context = BuildContext(databaseName);
            context.Authors.Add(new AuthorEntity { Names = "Felipe", LastNames = "Giraldo" });
            await context.SaveChangesAsync();

            var objectValidator = Substitute.For<IObjectModelValidator>();
            controller!.ObjectValidator = objectValidator;

            var document = new JsonPatchDocument<AuthorPatchRequest>();
            document.Operations.Add(new Operation<AuthorPatchRequest>("replace", "/names", null, "Haland"));

            // Test

            var response = await controller!.Patch(1, document);

            // Validation

            var result = response as StatusCodeResult;
            Assert.IsNotNull(result, "Expected an StatusCodeResult");
            Assert.AreEqual(expected: 204, result.StatusCode);

            await outputCacheStore!.Received(1).EvictByTagAsync(cache, default);

            var secondContext = BuildContext(databaseName);
            var updatedAuthor = await secondContext.Authors
                .FirstOrDefaultAsync(authors => authors.Id == 1);

            Assert.IsNotNull(updatedAuthor, "Expected author not null");
            Assert.AreEqual(expected: "Haland", actual: updatedAuthor.Names);
            Assert.AreEqual(expected: "Giraldo", actual: updatedAuthor.LastNames);
        }

        [TestMethod]
        public async Task Delete_Return404_WhenAuthorWasNotFound()
        {
            // Test

            var response = await controller!.Delete(1);

            // Validation

            var result = response as StatusCodeResult;
            Assert.IsNotNull(result, "Expected an StatusCodeResult");
            Assert.AreEqual(expected: 404, result.StatusCode);
        }

        [TestMethod]
        public async Task Delete_DeleteSuccessfullyAuthor_WhenItExists()
        {
            // Preparation

            var context = BuildContext(databaseName);
            context.Authors.Add(new AuthorEntity { Names = "Johan", LastNames = "Sanchez", Picture = "URL-1" });
            await context.SaveChangesAsync();

            // Test

            var response = await controller!.Delete(1);

            // Validation

            var secondContext = BuildContext(databaseName);
            var author = await secondContext.Authors
                .FirstOrDefaultAsync(author => author.Id == 1);

            var result = response as OkResult;
            Assert.IsNotNull(result, "Expected an OkResult");
            Assert.AreEqual(expected: 200, result.StatusCode);

            Assert.IsNull(author, "Author is null because was deleted");

            await storageFiles!.Received(1).Delete("URL-1", container);
            await outputCacheStore!.Received(1).EvictByTagAsync(cache, default);
        }
    }
}
