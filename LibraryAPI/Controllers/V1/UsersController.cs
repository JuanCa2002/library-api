using AutoMapper;
using LibraryAPI.Configuration;
using LibraryAPI.Models.Entities;
using LibraryAPI.Models.Requests;
using LibraryAPI.Models.Responses;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/users")]
    public class UsersController: ControllerBase
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;
        public UsersController(UserManager<UserEntity> userManager, 
            IConfiguration configuration, SignInManager<UserEntity> signInManager,
            IUserService userService, ApplicationDbContext dbContext,
            IMapper mapper)

        {
            _userManager = userManager;
            _configuration = configuration;
            _signInManager = signInManager;
            _userService = userService;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        [HttpPost("register", Name = "RegisterUserV1")]
        public async Task<ActionResult<AuthenticationResponse>> Register(
            [FromBody] UserCredentialsRequest request)
        {
            var user = new UserEntity
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password!);

            if(result.Succeeded)
            {
               return await BuildToken(request);
            } 
            else
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return ValidationProblem();
            }
        }

        [HttpPost("login", Name = "LoginUserV1")]
        public async Task<ActionResult<AuthenticationResponse>> Login(
           [FromBody] UserCredentialsRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            
            if(user is null)
            {
                return ReturnIncorrectLogin();
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, 
                request.Password!, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return await BuildToken(request);
            }
            else
            {
                return ReturnIncorrectLogin();
            }

        }

        [HttpPut(Name = "UpdateUserV1")]
        [Authorize]
        public async Task<ActionResult> Update(UpdateUserRequest request)
        {
            var user = await _userService.GetUser();

            if(user is null)
            {
                return NotFound();
            }

            user.BirthDate = request.BirthDate;

            await _userManager.UpdateAsync(user);
            return NoContent();
        }

        [HttpGet(Name = "GetAllUsersV1")]
        [Authorize(Policy = "isAdmin")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
        {
            var users = await _dbContext.Users.ToListAsync();

            return Ok(_mapper.Map<IEnumerable<UserResponse>>(users));
        }

        [HttpGet("renew-token", Name = "RenewTokenV1")]
        [Authorize]
        public async Task<ActionResult<AuthenticationResponse>> Renew()
        {
            var user = await _userService.GetUser();

            if(user is null)
            {
                return NotFound();
            }

            var userCredentials = new UserCredentialsRequest { Email = user.Email! };

            return await BuildToken(userCredentials);
        }

        [HttpPost("make-admin", Name = "MakeAdminV1")]
        [Authorize(Policy = "isAdmin")]
        public async Task<ActionResult> MakeAdmin(EditClaimRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if(user is null)
            {
                return NotFound();
            }

            await _userManager.AddClaimAsync(user, new Claim("isAdmin", "true"));
            return NoContent();
        }

        [HttpPost("remove-admin", Name = "RemoveAdminV1")]
        [Authorize(Policy = "isAdmin")]
        public async Task<ActionResult> RemoveAdmin(EditClaimRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
            {
                return NotFound();
            }

            await _userManager.RemoveClaimAsync(user, new Claim("isAdmin", "true"));
            return NoContent();
        }


        private ActionResult ReturnIncorrectLogin()
        {
            ModelState.AddModelError(string.Empty, "Invalid Login");
            return ValidationProblem();
        }
        
        private async Task<AuthenticationResponse> BuildToken(
            UserCredentialsRequest request)
        {
            var claims = new List<Claim>
            {
                new ("email", request.Email)
            };

            var user = await _userManager.FindByEmailAsync(request.Email);
            var claimsDb = await _userManager.GetClaimsAsync(user!);

            claims.AddRange(claimsDb);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwtKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddDays(1);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiration, signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return new AuthenticationResponse 
            { 
                Token = token,
                ExpireDate = expiration
            };

        }

    }
}
