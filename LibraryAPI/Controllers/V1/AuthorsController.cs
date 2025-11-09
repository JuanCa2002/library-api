using AutoMapper;
using LibraryAPI.Configuration;
using LibraryAPI.Models.Entities;
using LibraryAPI.Models.Requests;
using LibraryAPI.Models.Responses;
using LibraryAPI.Services;
using LibraryAPI.Utilities;
using LibraryAPI.Utilities.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq.Dynamic.Core;

namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/authors")]
    [Authorize(Policy = "isAdmin")]
    [AddHeadersFilter("controller", "authors")]
    public class AuthorsController: ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthorsController> _logger;
        private readonly IStorageFiles _storageFiles;
        private readonly IOutputCacheStore _outputCacheStore;
        private const string container = "authors";
        private const string cache = "get-authors";
        public AuthorsController(ApplicationDbContext dbContext,
            IMapper mapper, ILogger<AuthorsController> logger,
            IStorageFiles storageFiles, IOutputCacheStore outputCacheStore)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
            _storageFiles = storageFiles;
            _outputCacheStore = outputCacheStore;
        }

        [HttpGet(Name = "GetAllAuthorsV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        [ServiceFilter<ActionFilter>()]
        [ServiceFilter<HATEOSAuthorsAttribute>()]
        [AddHeadersFilter("action", "get-authors")]
        [EndpointSummary("Get all authors")]
        [EndpointDescription("Get the full list of the authors")]
        [ProducesResponseType<AuthorResponse>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<AuthorResponse>>> GetAll(
            [FromQuery] PaginationRequest paginationRequest)
        {
            _logger.LogInformation("Getting Authors List");

            var queryable = _dbContext.Authors.AsQueryable();
            await HttpContext.InsertParamsPaginationHeaders(queryable);

            var authors = await queryable
                .OrderBy(author => author.Names)
                .Paginate(paginationRequest)
                .ToListAsync();

            return Ok(_mapper.Map<List<AuthorResponse>>(authors));
        }

        [HttpGet("{id:int}", Name = "GetAuthorV1")]
        [AllowAnonymous]
        [OutputCache(Tags = [cache])]
        [ServiceFilter<HATEOSAuthorAttribute>()]
        [EndpointSummary("Get author by ID")]
        [EndpointDescription("Get an author by its ID, if exists")]
        [ProducesResponseType<AuthorWithBooksResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthorWithBooksResponse>> GetById(
            [FromRoute]
            [Description("Unique identifier of the author")] int id)
        {
            var author = await _dbContext.Authors
                .Include(author => author.Books)
                .ThenInclude(authorBook => authorBook.Book)
                .FirstOrDefaultAsync(author => author.Id == id);

            if( author is null)
            {
                return NotFound();
            }

            var authorWithBooks = _mapper.Map<AuthorWithBooksResponse>(author);

            return Ok(authorWithBooks);

        }

        [HttpGet("filter", Name = "GetAuthorsByFilterV1")]
        [AllowAnonymous]
        public async Task<ActionResult> Filter(
            [FromQuery] AuthorFilterRequest filter)
        {
            var queryable = _dbContext.Authors.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Names)) 
            {
                queryable = queryable.Where(author => author.Names.Contains(filter.Names));
            }

            if (!string.IsNullOrEmpty(filter.LastNames))
            {
                queryable = queryable
                    .Where(author => author.LastNames.Contains(filter.LastNames));
            }

            if (filter.IncludeBooks)
            {
                queryable = queryable
                    .Include(author => author.Books)
                    .ThenInclude(authorBook => authorBook.Book);
            }

            if(filter.HasPicture.HasValue)
            {
                if (filter.HasPicture.Value)
                {
                    queryable = queryable
                        .Where(author => author.Picture != null);
                }
                else
                {
                    queryable = queryable
                        .Where(author => author.Picture == null);
                }
            }

            if (filter.HasBooks.HasValue)
            {
                if (filter.HasBooks.Value)
                {
                    queryable = queryable
                        .Where(author => author.Books.Any());
                }
                else
                {
                    queryable = queryable
                        .Where(author => !author.Books.Any());
                }
            }

            if (!string.IsNullOrEmpty(filter.BookTitle))
            {
                queryable = queryable
                    .Where(author => author.Books
                        .Any(authorBook => authorBook.Book!.Title.Contains(filter.BookTitle)));
            }

            if (!string.IsNullOrEmpty(filter.OrderField))
            {
                var orderType = filter.AscendantOrder ? "ascending" : "descending";

                try
                {
                    queryable = queryable
                        .OrderBy($"{filter.OrderField} {orderType}");
                }
                catch(Exception ex)
                {
                    queryable = queryable.OrderBy(author => author.Names);
                    _logger.LogError(ex.Message, ex);
                }

            }
            else
            {
                queryable = queryable.OrderBy(author => author.Names);
            }

            var authorsResult = await queryable
                .Paginate(filter.Pagination)
                .ToListAsync();

            dynamic? authors = null;

            if (filter.IncludeBooks)
            { 
               authors = _mapper.Map<IEnumerable<AuthorWithBooksResponse>>(authorsResult);
            } 
            else
            {
               authors = _mapper.Map<IEnumerable<AuthorResponse>>(authorsResult);
            }
            return Ok(authors);
        }

        [HttpPost(Name = "CreateAuthorV1")]
        [EndpointSummary("Create a new author")]
        [EndpointDescription("Create a new author providing the required data")]
        [ProducesResponseType<AuthorResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<AuthorResponse>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthorResponse>> Create([FromBody] AuthorRequest request)
        {
            var entity = _mapper.Map<AuthorEntity>(request);
            _dbContext.Authors.Add(entity);

            await _dbContext.SaveChangesAsync();

            await _outputCacheStore.EvictByTagAsync(cache, default);

            return CreatedAtRoute("GetAuthorV1", new {id = entity.Id}, _mapper.Map<AuthorResponse>(entity));
        }

        [HttpPost("with-picture", Name = "CreateAuthorWithPictureV1")]
        [EndpointSummary("Create a new author")]
        [EndpointDescription("Create a new author providing the required data")]
        [ProducesResponseType<AuthorResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<AuthorResponse>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthorResponse>> CreateWithPicture(
            [FromForm] AuthorWithPictureRequest request)
        {
            var entity = _mapper.Map<AuthorEntity>(request);

            if(request.Picture is not null)
            {
                var url = await _storageFiles.Store(container, request.Picture);
                entity.Picture = url;
            }

            _dbContext.Authors.Add(entity);

            await _dbContext.SaveChangesAsync();

            await _outputCacheStore.EvictByTagAsync(cache, default);

            return CreatedAtRoute("GetAuthorV1", new { id = entity.Id }, _mapper.Map<AuthorResponse>(entity));
        }

        [HttpPut(Name = "UpdateAuthorV1")]
        [EndpointSummary("Update an existing author")]
        [EndpointDescription("Update an existing author with its new data")]
        [ProducesResponseType<AuthorResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType<AuthorResponse>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthorResponse>> Update([FromForm] AuthorUpdateRequest request)
        {
            var author = await _dbContext.Authors
                .FirstOrDefaultAsync(author => author.Id == request.Id);

            if (author is null)
            {
                return NotFound();
            }

            author.Names = request.Names;
            author.LastNames = request.LastNames;
            author.Identification = request.Identification;

            if(request.Picture is not null)
            {
                var currentUrl = await _dbContext.Authors
                    .Where(author => author.Id == request.Id)
                    .Select(author => author.Picture)
                    .FirstAsync();

                var url = await _storageFiles.Edit(currentUrl, container, request.Picture);
                author.Picture = url;
            }

            await _dbContext.SaveChangesAsync();
            await _outputCacheStore.EvictByTagAsync(cache, default);
            return Ok(_mapper.Map<AuthorResponse>(author));
        }

        [HttpPatch("{id:int}", Name = "UpdateAttributesAuthorV1")]
        [EndpointSummary("Update some attributes of an existing author")]
        [EndpointDescription("Update some existing attributes of an existing author")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<AuthorResponse>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<AuthorPatchRequest> document)
        {
            if(document is null)
            {
                return BadRequest();
            }

            var author = await _dbContext.Authors
                .FirstOrDefaultAsync(author => author.Id == id);

            if(author is null)
            {
                return NotFound();
            }

            var authorPatch = _mapper.Map<AuthorPatchRequest>(author);

            document.ApplyTo(authorPatch, ModelState);

            var isValid = TryValidateModel(authorPatch);

            if (!isValid) 
            {
                return ValidationProblem();
            }

            _mapper.Map(authorPatch, author);

            await _dbContext.SaveChangesAsync();

            await _outputCacheStore.EvictByTagAsync(cache, default);

            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "DeleteAuthorV1")]
        [EndpointSummary("Delete an existing author")]
        [EndpointDescription("Delete an existing author by its given ID")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType<AuthorResponse>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Delete(
            [FromRoute]
            [Description("Unique identifier of the author to delete")] int id)
        {
            var author = await _dbContext.Authors
                .FirstOrDefaultAsync(author => author.Id == id);

            if (author is null)
            {
                return NotFound();
            }

            _dbContext.Authors.Remove(author);

            await _dbContext.SaveChangesAsync();
            await _storageFiles.Delete(author.Picture, container);
            await _outputCacheStore.EvictByTagAsync(cache, default);

            return Ok();
        }
    }
}
