using LibraryAPI.Configuration;
using LibraryAPI.Mappings;
using LibraryAPI.Middlewares;
using LibraryAPI.Models.Entities;
using LibraryAPI.Services;
using LibraryAPI.Utilities;
using LibraryAPI.Utilities.V1;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Services Area

builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddDataProtection();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(CORSOptions =>
    {
        CORSOptions.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
        .WithExposedHeaders("total-quantity");
    });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalFilterMeasureExecutionTime>();
    options.Conventions.Add(new GroupByVersion());
}).AddNewtonsoftJson();

builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql("name=DefaultConnection");
});

builder.Services.AddIdentityCore<UserEntity>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<UserManager<UserEntity>>();
builder.Services.AddScoped<SignInManager<UserEntity>>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IStorageFiles, StorageFileAzure>();
builder.Services.AddScoped<ActionFilter>();
builder.Services.AddScoped<ValidationBookFilter>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<LibraryAPI.Services.V1.ILinksGenerator,
    LibraryAPI.Services.V1.LinksGenerator>();
builder.Services.AddScoped<HATEOSAuthorAttribute>();
builder.Services.AddScoped<HATEOSAuthorsAttribute>();

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = 
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwtKey"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("isAdmin", policy => policy.RequireClaim("isAdmin"));
});

builder.Services.AddSwaggerGen(options =>
{
    // Version 1

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Library API",
        Description = "This a WEB API to work with authors and books data",
        Contact = new OpenApiContact
        {
            Email = "camilotorresb104@gmail.com",
            Name = "Juan Camilo Torres Beltrán"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/license/mit/")
        }
    });

    // Version 2

    options.SwaggerDoc("v2", new OpenApiInfo
    {
        Version = "v2",
        Title = "Library API",
        Description = "This a WEB API to work with authors and books data",
        Contact = new OpenApiContact
        {
            Email = "camilotorresb104@gmail.com",
            Name = "Juan Camilo Torres Beltrán"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/license/mit/")
        }
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.OperationFilter<FilterAuthorizationConfiguration>();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}

// Middleware Area

app.UseExceptionHandler(exceptionHandlerApp => exceptionHandlerApp.Run(async context =>
{
    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = exceptionHandlerFeature?.Error!;

    var error = new ErrorEntity()
    {
        Message = exception.Message,
        StackTrace = exception.StackTrace,
        Date = DateTime.UtcNow
    };

    var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();
    dbContext.Errors.Add(error);

    await dbContext.SaveChangesAsync();
    await Results.InternalServerError(new
    {
        type = "error",
        message = "Internal server error ocurred",
        status = 500
    }).ExecuteAsync(context);
}));

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Library API V1");
    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Library API V2");
});

app.UseCors();

app.UseOutputCache();

app.UseLogRequest();

app.MapControllers();

app.Run();

public partial class Program { }
