using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Models.Entities
{
    public class UserEntity: IdentityUser
    {
        public DateTime BirthDate { get; set; }
    }
}
