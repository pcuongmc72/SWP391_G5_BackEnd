using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SWP.BLL.Interfaces;
using SWP.BLL.Services;
using SWP.DAL.Context;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ────────────────────────────────────────────────────────────────
<<<<<<< HEAD
builder.Services.AddDbContext<EduTrainingDbContext>(options =>
=======
builder.Services.AddDbContext<FlippedClassroomContext>(options =>
>>>>>>> origin/thuanpdhe187333
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── JWT Authentication ───────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
<<<<<<< HEAD
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
=======
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
>>>>>>> origin/thuanpdhe187333
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
<<<<<<< HEAD
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSection["Issuer"],
        ValidAudience            = jwtSection["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(key),
        ClockSkew                = TimeSpan.Zero
=======
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
>>>>>>> origin/thuanpdhe187333
    };
});

// ─── DI Services ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
<<<<<<< HEAD

// ─── Controllers ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
=======
builder.Services.AddScoped<ILecturerService, LecturerService>();

// ─── Controllers ─────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        // Hỗ trợ serialize DateOnly/TimeOnly (System.Text.Json không hỗ trợ mặc định trước .NET 7)
        opts.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        opts.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
        opts.JsonSerializerOptions.Converters.Add(new NullableTimeOnlyJsonConverter());
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
>>>>>>> origin/thuanpdhe187333

// ─── CORS ────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ─── Swagger với hỗ trợ JWT ──────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
<<<<<<< HEAD
        Title       = "EduTraining API",
        Version     = "v1",
=======
        Title = "EduTraining API",
        Version = "v1",
>>>>>>> origin/thuanpdhe187333
        Description = "API hệ thống EduTraining – Authentication & Authorization"
    });

    // Thêm định nghĩa Bearer token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập JWT token theo định dạng: Bearer {token}",
<<<<<<< HEAD
        Name        = "Authorization",
        In          = ParameterLocation.Header,
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
=======
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
>>>>>>> origin/thuanpdhe187333
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─── Build & Middleware ───────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EduTraining API v1"));
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

<<<<<<< HEAD
app.Run();
=======
app.Run();
>>>>>>> origin/thuanpdhe187333
