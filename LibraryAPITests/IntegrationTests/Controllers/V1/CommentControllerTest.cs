using LibraryAPI.Models.Entities;
using LibraryAPITests.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LibraryAPITests.IntegrationTests.Controllers.V1
{
    [TestClass]
    public class CommentControllerTest: TestBase
    {
        private readonly string url = "/api/v1/books/1/comments";
        private string databaseName = Guid.NewGuid().ToString();

        private async Task CreateTestData()
        {
            var context = BuildContext(databaseName);
            var author = new AuthorEntity
            {
                Names = "Cristiano",
                LastNames = "Ronaldo"
            };
            context.Authors.Add(author);
            await context.SaveChangesAsync();

            var book = new BookEntity
            {
                Title = "Alicia en el país de las maravillas"
            };

            book.Authors.Add(new AuthorBookEntity
            {
                Author = author,
            });

            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        [TestMethod]
        public async Task Delete_Returns204_WhenUserDeleteHisOwnComment()
        {
            // Preparation

            await CreateTestData();

            var factory = BuildWebApplicationFactory(databaseName, false);
            var token = await BuildUser(databaseName, factory);

            var context = BuildContext(databaseName);

            var user = await context.Users.FirstAsync();

            var comment = new CommentEntity
            {
                Body = "Hello",
                UserId = user!.Id,
                BookId = 1
            };

            context.Comments.Add(comment);

            await context.SaveChangesAsync();

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Test

            var response = await client.DeleteAsync($"{url}/{comment.Id}");

            // Validation

            Assert.AreEqual(expected: HttpStatusCode.NoContent, actual: response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_Returns403_WhenUserTryToDeleteACommentFromOtherUser()
        {
            // Preparation

            await CreateTestData();

            var factory = BuildWebApplicationFactory(databaseName, false);
            var ownerEmailComment = "owner@email.com";
            await BuildUser(databaseName, factory, [] ,ownerEmailComment);

            var context = BuildContext(databaseName);

            var ownerCommentUser = await context.Users.FirstAsync();

            var comment = new CommentEntity
            {
                Body = "Hello",
                UserId = ownerCommentUser!.Id,
                BookId = 1
            };

            context.Comments.Add(comment);

            await context.SaveChangesAsync();

            var tokenDifferentUser = 
                await BuildUser(databaseName, factory, [], "differentUser@email.com"); 

            var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenDifferentUser);

            // Test

            var response = await client.DeleteAsync($"{url}/{comment.Id}");

            // Validation

            Assert.AreEqual(expected: HttpStatusCode.Forbidden, actual: response.StatusCode);
        }
    }
}
