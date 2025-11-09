using AutoMapper;
using LibraryAPI.Configuration;
using LibraryAPI.Mappings;
using LibraryAPI.Models.Requests;
using LibraryAPI.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LibraryAPITests.Utilities
{
    public class TestBase
    {
        protected readonly JsonSerializerOptions jsonSerializerOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
        protected readonly Claim adminClaim = new Claim("isAdmin", "1");
        protected ApplicationDbContext BuildContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName).Options;

            var dbContext = new ApplicationDbContext(options);
            return dbContext;
        }

        protected IMapper ConfigureAutoMapper()
        {
            var config = new MapperConfiguration(options =>
            {
                options.AddProfile(new AutoMapperProfile());
            });

            return config.CreateMapper();
        }

        protected WebApplicationFactory<Program> BuildWebApplicationFactory(
            string databaseName, bool ignoreSecurity = true)
        {
            var factory = new WebApplicationFactory<Program>();
            factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    ServiceDescriptor serviceDescriptor = services.SingleOrDefault(
                        defaultValue => defaultValue.ServiceType ==
                        typeof(IDbContextOptionsConfiguration<ApplicationDbContext>))!;

                    if (serviceDescriptor is not null)
                    {
                        services.Remove(serviceDescriptor);
                    }

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(databaseName);
                    });

                    if (ignoreSecurity)
                    {
                        services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();

                        services.AddControllers(options =>
                        {
                            options.Filters.Add(new FakeUserFilter());
                        });
                    }
                });
            });

            return factory;
        }

        protected async Task<string> BuildUser(string databaseName,
            WebApplicationFactory<Program> factory) => await BuildUser(databaseName, factory, [],
                "test@email.com");

        protected async Task<string> BuildUser(string databaseName,
           WebApplicationFactory<Program> factory,
           IEnumerable<Claim> claims) => await BuildUser(databaseName, factory, claims,
               "test@email.com");

        protected async Task<string> BuildUser(string databaseName, 
            WebApplicationFactory<Program> factory, IEnumerable<Claim> claims, string email)
        {
            var registreUrl = "/api/v1/users/register";
            string token = string.Empty;
            token = await GetToken(email, registreUrl, factory);

            if(claims.Any())
            {
                var context = BuildContext(databaseName);
                var user = await context.Users.Where(user => user.Email == email).FirstAsync();
                Assert.IsNotNull(user);

                var userClaims = claims.Select(claim => new IdentityUserClaim<string>
                {
                    UserId = user.Id,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value
                });

                context.UserClaims.AddRange(userClaims);
                await context.SaveChangesAsync();
                var urlLogin = "/api/v1/users/login";
                token = await GetToken(email, urlLogin, factory);
            }

            return token;
        }

        private async Task<string> GetToken(string email, string url, 
            WebApplicationFactory<Program> factory)
        {
            var password = "aA123456!";
            var credentials = new UserCredentialsRequest
            {
                Email = email,
                Password = password
            };
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync(url, credentials);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var authenticationResponse = System.Text.Json.JsonSerializer.Deserialize<AuthenticationResponse>(
                content, jsonSerializerOptions)!;

            Assert.IsNotNull(authenticationResponse.Token);
            return authenticationResponse.Token;
        }

    }
}
