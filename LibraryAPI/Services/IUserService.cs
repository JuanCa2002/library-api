using LibraryAPI.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace LibraryAPI.Services
{
    public interface IUserService
    {
        Task<UserEntity?> GetUser();
    }
}
