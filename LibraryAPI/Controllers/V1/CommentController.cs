using AutoMapper;
using LibraryAPI.Configuration;
using LibraryAPI.Models.Entities;
using LibraryAPI.Models.Requests;
using LibraryAPI.Models.Responses;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/books/{bookId:int}/comments")]
    [Authorize]
    public class CommentController: ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string cache = "get-comments";

        public CommentController(ApplicationDbContext dbContext, IMapper mapper,
            IUserService userService, IOutputCacheStore outputCacheStore)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _userService = userService;
            _outputCacheStore = outputCacheStore;
        }

        [HttpGet(Name = "GetAllCommentsV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<List<CommentReponse>>> GetAll(int bookId)
        {
            var existBook = await _dbContext.Books.AnyAsync(book => book.Id == bookId);

            if (!existBook)
            {
                return NotFound();
            }

            var commentsByBook = await _dbContext.Comments
                .Include(comment => comment.User)
                .Where(comment => comment.BookId == bookId)
                .OrderByDescending(comment => comment.PublishedDate)
                .ToListAsync();

            return Ok(_mapper.Map<List<CommentReponse>>(commentsByBook));
        }

        [HttpGet("{id}", Name = "GetCommentByIdV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<CommentReponse>> GetById([FromRoute] Guid id)
        {
            var comment = await _dbContext.Comments
                .Include(comment => comment.User)
                .FirstOrDefaultAsync(comment => comment.Id == id);

            if(comment is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CommentReponse>(comment));
        }

        [HttpPost(Name = "CreateCommentV1")]
        public async Task<ActionResult<CommentReponse>> Create([FromRoute] int bookId,
            [FromBody] CommentRequest request)
        {
            var existBook = await _dbContext.Books.AnyAsync(book => book.Id == bookId);

            if(!existBook)
            {
                return NotFound();
            }

            var user = await _userService.GetUser();
            
            if(user is null)
            {
                return NotFound();
            }

            var comment = _mapper.Map<CommentEntity>(request);
            comment.BookId = bookId;
            comment.UserId = user.Id;
            comment.PublishedDate = DateTime.UtcNow;

            _dbContext.Comments.Add(comment);
            await _dbContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cache, default);

            return CreatedAtRoute("GetCommentByIdV1", new { id = comment.Id, bookId }, _mapper.Map<CommentReponse>(comment));
        }

        [HttpPatch("{id}", Name = "UpdateCommentAttributesV1")]
        public async Task<ActionResult> Patch(Guid id, int bookId ,JsonPatchDocument<CommentPatchRequest> document)
        {
            var existBook = await _dbContext.Books.AnyAsync(book => book.Id == bookId);

            if (!existBook)
            {
                return NotFound();
            }

            var user = await _userService.GetUser();

            if (user is null)
            {
                return NotFound();
            }

            if (document is null)
            {
                return BadRequest();
            }

            var comment = await _dbContext.Comments
                .FirstOrDefaultAsync(comment => comment.Id == id);

            if (comment is null)
            {
                return NotFound();
            }

            if(comment.UserId != user.Id)
            {
                return Forbid();
            }

            var commentPatch = _mapper.Map<CommentPatchRequest>(comment);

            document.ApplyTo(commentPatch, ModelState);

            var isValid = TryValidateModel(commentPatch);

            if (!isValid)
            {
                return ValidationProblem();
            }

            _mapper.Map(commentPatch, comment);

            await _dbContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }

        [HttpDelete("{id}", Name = "DeleteCommentV1")]
        public async Task<ActionResult> Delete([FromRoute] Guid id, [FromRoute] int bookId)
        {
            var existBook = await _dbContext.Books.AnyAsync(book => book.Id == bookId);

            if(!existBook)
            {
                return NotFound();
            }

            var user = await _userService.GetUser();

            if (user is null)
            {
                return NotFound();
            }

            var comment = await _dbContext.Comments
                .FirstOrDefaultAsync(comment => comment.Id == id);

            if(comment is null)
            {
                return NotFound();  
            }

            if(comment.UserId != user.Id)
            {
                return Forbid();
            }

            comment.IsDeleted = true;
            _dbContext.Comments.Update(comment);
            await _dbContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }

    }
}
