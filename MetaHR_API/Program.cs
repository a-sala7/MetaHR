using Business.Accounts;
using Business.Announcements;
using Business.Attendances;
using Business.Departments;
using Business.Email;
using Business.EmployeeNotes;
using Business.Employees;
using Business.FileManager;
using Business.JobApplications;
using Business.JobPostings;
using Business.Tickets;
using Business.VacationRequests;
using Common.ConfigurationClasses;
using DataAccess.Data;
using DataAccess.DbInitializer;
using MetaHR_API;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(cfg =>
{
    cfg.Filters.Add(new ValidationActionFilter());
}).AddJsonOptions(opt =>
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

    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
});

string dbConString = "";
dbConString = builder.Configuration.GetValue<string>("METAHR_DB_CONSTRING");

string sendinblueKey = builder.Configuration.GetValue<string>("METAHR_SENDINBLUE_KEY");
sib_api_v3_sdk.Client.Configuration.Default.AddApiKey("api-key", sendinblueKey);

ApiConfiguration apiConfiguration = new();

if (builder.Environment.IsDevelopment())
{
    apiConfiguration = builder.Configuration.GetSection("METAHR_API_CONFIGURATION").Get<ApiConfiguration>();
}
else
{
    apiConfiguration.ValidAudience = builder.Configuration.GetValue<string>("METAHR_APICFG_VALIDAUDIENCE");
    apiConfiguration.ValidIssuer = builder.Configuration.GetValue<string>("METAHR_APICFG_VALIDISSUER");
    apiConfiguration.SecretKey = builder.Configuration.GetValue<string>("METAHR_APICFG_KEY");
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(dbConString);
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
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

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
        byte[] keyBytes = Encoding.UTF8.GetBytes(apiConfiguration.SecretKey);
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
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeNoteRepository, EmployeeNoteRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<IFileManager, S3FileManager>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
builder.Services.AddScoped<IVacationRequestRepository, VacationRequestRepository>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

//builder.Services.AddScoped<IEmailSender, ConsoleEmailLogger>();
builder.Services.AddScoped<IEmailSender, SendinblueEmailSender>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IDbInitializer, DbInitializer>();

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

app.InitializeDatabase(app.Environment.EnvironmentName).GetAwaiter().GetResult();

app.Run();
