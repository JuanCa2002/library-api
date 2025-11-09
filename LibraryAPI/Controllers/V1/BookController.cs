using AutoMapper;
using LibraryAPI.Configuration;
using LibraryAPI.Models.Entities;
using LibraryAPI.Models.Requests;
using LibraryAPI.Models.Responses;
using LibraryAPI.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("/api/v1/books")]
    [Authorize(Policy = "isAdmin")]
    public class BookController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ITimeLimitedDataProtector _dataProtectionProvider;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string cache = "get-books";
        public BookController(ApplicationDbContext dbContext,
            IMapper mapper, IDataProtectionProvider protectionProvider,
            IOutputCacheStore outputCacheStore)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _dataProtectionProvider = protectionProvider.CreateProtector("BookController")
                .ToTimeLimitedDataProtector();
            _outputCacheStore = outputCacheStore;
        }

        [HttpGet("list/get-token", Name = "GetTokenBooksV1")]
        public ActionResult GetListToken()
        {
            var plainText = Guid.NewGuid().ToString();
            var token = _dataProtectionProvider.Protect(plainText,
                lifetime: TimeSpan.FromSeconds(30));
            var url = Url.RouteUrl("GetBooksUsingTokenV1", new { token }, "https");
            return Ok(url);
        }

        [HttpGet("list/{token}", Name = "GetBooksUsingTokenV1")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BookSimpleResponse>>> GetBooksUsingToken(string token)
        {
            try
            {
                _dataProtectionProvider.Unprotect(token);
            }
            catch
            {
                ModelState.AddModelError(nameof(token), "El token ha expirado");
                return ValidationProblem();
            }

            var books = await _dbContext.Books
                .Include(book => book.Authors)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<BookSimpleResponse>>(books));
        }

        [HttpPost(Name = "CreateBookV1")]
        [ServiceFilter<ValidationBookFilter>()]
        public async Task<ActionResult<BookSimpleResponse>> Create([FromBody] BookRequest request)
        {
            var entity = _mapper.Map<BookEntity>(request);
            AsignOrderAuthors(entity);

            _dbContext.Books.Add(entity);
            await _dbContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cache, default);

            return CreatedAtRoute("GetBookV1", new {id = entity.Id}, _mapper.Map<BookSimpleResponse>(entity));
        }

        private void AsignOrderAuthors(BookEntity entity)
        {
            if(entity.Authors is not null)
            {
                for (int i = 0; i < entity.Authors.Count; i++)
                {
                    entity.Authors[i].Order = i;
                }
            }
        }

        [HttpPut(Name = "UpdateBookV1")]
        [ServiceFilter<ValidationBookFilter>()]
        public async Task<ActionResult<BookWithAuthorResponse>> Update([FromBody] BookUpdateRequest request)
        {
            var currentBook = await _dbContext.Books
                .Include(book => book.Authors)
                .FirstOrDefaultAsync(book => book.Id == request.Id);

            if(currentBook is null)
            {
                return NotFound();
            }

            currentBook = _mapper.Map(request, currentBook);
            AsignOrderAuthors(currentBook);

            await _dbContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cache, default);

            var updatedBook = await _dbContext.Books
                .Include(book => book.Authors)
                .ThenInclude(authorBook => authorBook.Author)
                .FirstOrDefaultAsync(book => book.Id == request.Id);

            return Ok(_mapper.Map<BookWithAuthorResponse>(updatedBook));
        }

        [HttpGet(Name = "GetAllBooksV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<IEnumerable<BookSimpleResponse>>> GetAll(
            [FromQuery] PaginationRequest paginationRequest)
        {
            var queryable = _dbContext.Books.AsQueryable();
            await HttpContext.InsertParamsPaginationHeaders(queryable);

            var books = await queryable
                .OrderBy(book => book.Title)
                .Paginate(paginationRequest)
                .Include(book => book.Authors)
                .ToListAsync();

            return Ok(_mapper.Map<IEnumerable<BookSimpleResponse>>(books));
        }

        [HttpGet("{id:int}", Name = "GetBookV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        public async Task<ActionResult<BookWithAuthorResponse>> GetById([FromRoute] int id)
        {
            var book = await _dbContext.Books
                .Include(book => book.Authors)
                .ThenInclude(authorBook => authorBook.Author)
                .FirstOrDefaultAsync(book => book.Id == id);

            if(book is null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<BookWithAuthorResponse>(book));
        }

        [HttpDelete("{id:int}", Name = "DeleteBookV1")]
        public async Task<ActionResult> Delete([FromRoute] int id) 
        {
            var deletedRegisters = await _dbContext.Books
                .Where(book => book.Id == id)
                .ExecuteDeleteAsync();

            if(deletedRegisters == 0)
            {
                return NotFound();
            }

            await _outputCacheStore.EvictByTagAsync(cache, default);

            return Ok();
        }
    }
}
