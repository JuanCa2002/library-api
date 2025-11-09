using AutoMapper;
using LibraryAPI.Models.Entities;
using LibraryAPI.Models.Requests;
using LibraryAPI.Models.Responses;

namespace LibraryAPI.Mappings
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            //Author Mappers
            CreateMap<AuthorRequest, AuthorEntity>();
            CreateMap<AuthorWithPictureRequest, AuthorEntity>()
                .ForMember(entity => entity.Picture, opt => opt.Ignore());
            CreateMap<AuthorEntity, AuthorResponse>()
                .ForMember(response => response.FullName, opt => opt.MapFrom(entity => $"{entity.Names} {entity.LastNames}"));
            CreateMap<AuthorEntity, AuthorWithBooksResponse>()
                .ForMember(response => response.FullName, opt => opt.MapFrom(entity => $"{entity.Names} {entity.LastNames}"))
                .ForMember(response => response.Books, opt => opt.MapFrom(entity => TransformToBooksResponse(entity.Books)));
            CreateMap<AuthorEntity, AuthorPatchRequest>().ReverseMap();   

            //Book Mappers
            CreateMap<BookRequest, BookEntity>()
                .ForMember(entity => entity.Authors, opt => opt.MapFrom(request => TransformAuthors(request.AuthorIds)));
            CreateMap<BookUpdateRequest, BookEntity>()
                .ForMember(entity => entity.Authors, opt => opt.MapFrom(request => TransformAuthors(request.AuthorIds)));
            CreateMap<BookEntity, BookSimpleResponse>();
            CreateMap<BookEntity, BookWithAuthorResponse>().
                ForMember(response => response.Authors, opt => opt.MapFrom(entity => TransformToAuthorsResponse(entity.Authors)));
            CreateMap<BookRequest, AuthorBookEntity>()
                .ForMember(entity => entity.Book, opt => opt.MapFrom(request => new BookEntity() { Title = request.Title }));

            //Comment Mappers
            CreateMap<CommentRequest, CommentEntity>();
            CreateMap<CommentEntity, CommentReponse>()
                .ForMember(response => response.UserEmail, opt => opt.MapFrom(entity => entity.User!.Email));
            CreateMap<CommentEntity, CommentPatchRequest>().ReverseMap();

            //User Mappers
            CreateMap<UserEntity, UserResponse>();
        }

        private List<AuthorBookEntity> TransformAuthors(List<int> authorIds)
        {
            return [.. authorIds.Select(authorId => new AuthorBookEntity()
            {
                AuthorId = authorId,
            })];
        }

        private List<AuthorResponse> TransformToAuthorsResponse(List<AuthorBookEntity> authorBookEntities)
        {
            return [.. authorBookEntities.Select(authorBookEntity => new AuthorResponse()
            {
                Id = authorBookEntity.AuthorId,
                FullName = $"{authorBookEntity.Author!.Names} {authorBookEntity.Author!.LastNames}"
            })];
        }

        private List<BookSimpleResponse> TransformToBooksResponse(List<AuthorBookEntity> authorBookEntities)
        {
            return [.. authorBookEntities.Select(authorBookEntity => new BookSimpleResponse()
            {
                Id = authorBookEntity.BookId,
                Title = authorBookEntity.Book!.Title
            })];
        }
    }
}
