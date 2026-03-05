using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using volvcontrol_api.Authorization;
using volvcontrol_api.domain.Interfaces.Repository;
using volvcontrol_api.domain.Interfaces.Service;
using volvcontrol_api.Middlewares;
using volvcontrol_api.repository.repo;
using volvcontrol_api.service.services;
using volvcontrol_api.service.firebase;
using volvcontrol_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' não configurada.");

builder.Services.AddScoped<IUserRepository>(_ => new UserRepository(connectionString));
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<ICompanyRepository>(_ => new CompanyRepository(connectionString));
builder.Services.AddScoped<ICompanyService, CompanyService>();

builder.Services.AddScoped<IClientRepository>(_ => new ClientRepository(connectionString));
builder.Services.AddScoped<IClientService, ClientService>();

builder.Services.AddScoped<IEquipmentRepository>(_ => new EquipmentRepository(connectionString));
builder.Services.AddScoped<IEquipmentService, EquipmentService>();

builder.Services.AddScoped<IImageRepository>(_ => new ImageRepository(connectionString));
builder.Services.AddScoped<IImageService, ImageService>();

// Firebase Storage e PhotoStorage (pastas: Company/IdCliente/tipo/IdEntidade)
var firebaseCredentialsPath = builder.Configuration["Firebase:CredentialsPath"];
if (!string.IsNullOrWhiteSpace(firebaseCredentialsPath))
{
    var contentRoot = builder.Environment.ContentRootPath;
    var fullPath = Path.IsPathRooted(firebaseCredentialsPath)
        ? firebaseCredentialsPath
        : Path.GetFullPath(Path.Combine(contentRoot, firebaseCredentialsPath));
    builder.Services.AddScoped<IFirebaseStorageService>(_ => new FirebaseStorageService(fullPath));
    builder.Services.AddScoped<IPhotoStorage, PhotoStorage>();
}

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "VolvControlApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "VolvControlApi";
builder.Services.AddSingleton(new JwtTokenService(builder.Configuration));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Exige que o usuário tenha position_description = ADM para acessar o endpoint.
    options.AddPolicy(AuthConstants.AdmOnlyPolicy, policy =>
        policy.RequireClaim(AuthConstants.PositionDescriptionClaim, AuthConstants.PositionAdm));
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT. Obtenha em POST /api/User/login"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
