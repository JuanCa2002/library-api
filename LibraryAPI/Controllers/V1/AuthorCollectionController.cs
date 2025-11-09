using AutoMapper;
using LibraryAPI.Configuration;
using LibraryAPI.Models.Entities;
using LibraryAPI.Models.Requests;
using LibraryAPI.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/author-collection")]
    [Authorize(Policy = "isAdmin")]
    public class AuthorCollectionController: ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        public AuthorCollectionController(ApplicationDbContext dbContext,
            IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpGet("{ids}", Name = "GetAuthorsByIdsV1")]
        [AllowAnonymous]
        public async Task<ActionResult<List<AuthorWithBooksResponse>>> GetByIds(string ids)
        {
            var collectionIds = new List<int>();
            foreach (var id in ids.Split(","))
            {
                if (int.TryParse(id, out int idInt))
                {
                    collectionIds.Add(idInt);
                }  
            }

            if (!collectionIds.Any())
            {
                ModelState.AddModelError(nameof(ids),
                    "Ningun id fue encontrado");
                return ValidationProblem();
            }

            var authors = await _dbContext.Authors
                .Include(author => author.Books)
                .ThenInclude(authorBook => authorBook.Book)
                .Where(author => collectionIds.Contains(author.Id))
                .ToListAsync();

            if(authors.Count != collectionIds.Count)
            {
                return NotFound();
            }

            var authorsWithBook = _mapper.Map<List<AuthorWithBooksResponse>>(authors);
            return Ok(authorsWithBook);
        }

        [HttpPost(Name = "CreateAuthorsV1")]
        public async Task<ActionResult> Create(IEnumerable<AuthorRequest> requests)
        {
            var authors = _mapper.Map<IEnumerable<AuthorEntity>>(requests);
            _dbContext.Authors.AddRange(authors); 

            await _dbContext.SaveChangesAsync();

            var authorResponses = _mapper.Map<IEnumerable<AuthorResponse>>(authors);
            var ids = authorResponses.Select(author => author.Id);
            var idsString = string.Join(",", ids);


            return CreatedAtRoute("GetAuthorsByIdsV1", new {ids = idsString}, authorResponses);
        }
    }
}
