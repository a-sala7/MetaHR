using Business.Accounts;
using Business.JobPostings;
using Common.ConfigurationClasses;
using DataAccess.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
   {
       opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
   });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Valid format: \"bearer <token>\"",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    o.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
            },
            new string[] { }
        }
    });
});

string dbConString = builder.Configuration.GetValue<string>("METAHR_DB_CONSTRING");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(dbConString);
    options.EnableSensitiveDataLogging(true);
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    //lockout defaults
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

var apiConfiguration = builder.Configuration.GetSection("METAHR_API_CONFIGURATION").Get<ApiConfiguration>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "CORSAllowAll",
        policyBuilder =>
        {
            policyBuilder
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(opt =>
    {
        byte[] keyBytes = Encoding.ASCII.GetBytes(apiConfiguration.SecretKey);
        opt.RequireHttpsMetadata = false;
        opt.SaveToken = true;
        opt.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidIssuer = apiConfiguration.ValidIssuer,
            ValidAudience = apiConfiguration.ValidAudience,
            ClockSkew = TimeSpan.Zero,
            ValidateLifetime = true,
            RequireExpirationTime = true
        };
    });

builder.Services.AddSingleton(apiConfiguration);
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IJobPostingRepository, JobPostingRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

app.UseCors("CORSAllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
  
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
