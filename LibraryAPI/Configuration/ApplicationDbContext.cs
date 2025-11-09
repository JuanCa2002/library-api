using LibraryAPI.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LibraryAPI.Configuration
{
    public class ApplicationDbContext : IdentityDbContext<UserEntity>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CommentEntity>().HasQueryFilter(comment => !comment.IsDeleted);
        }

        public DbSet<AuthorEntity> Authors { get; set; }
        public DbSet<BookEntity> Books { get; set; }
        public DbSet<CommentEntity> Comments { get; set; }
        public DbSet<AuthorBookEntity> AuthorsBooks { get; set; }
        public DbSet<ErrorEntity> Errors { get; set; }
    }
}
