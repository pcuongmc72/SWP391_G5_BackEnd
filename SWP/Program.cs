using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SWP.BLL.Interfaces;
using SWP.BLL.Services;
using SWP.DAL.Context;
using SWP.DAL.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<FlippedClassroomContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── JWT Authentication ───────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSection["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSection["Issuer"],
        ValidAudience            = jwtSection["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(key),
        ClockSkew                = TimeSpan.Zero
    };
});

// ─── DI Services ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();

// ─── Controllers ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();

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
        Title = "EduTraining API",
        Version = "v1",
        Description = "API hệ thống EduTraining – Authentication & Authorization"
    });

    // Cấu hình mới: Tự động nhận diện Bearer Token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http, 
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Chỉ cần dán thẳng JWT token vào đây, KHÔNG cần gõ chữ Bearer"
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
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IAcademicTermsService, AcademicTermsService>();
builder.Services.AddScoped<ICoursesService, CoursesService>();
builder.Services.AddScoped<IClassesService, ClassesService>();
builder.Services.AddScoped<IClassStudentsService, ClassStudentsService>();
builder.Services.AddScoped<IMaterialsService, MaterialsService>();
builder.Services.AddScoped<IAssignmentsService, AssignmentsService>();
builder.Services.AddScoped<ISubmissionsService, SubmissionsService>();

// ─── Build & Middleware ───────────────────────────────────────────────────────
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<FlippedClassroomContext>();
    try
    {
        // 1. Tạo bảng nếu chưa tồn tại bằng Raw SQL DDL
        var createTablesSql = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Materials')
            BEGIN
                CREATE TABLE Materials (
                    Id NVARCHAR(50) NOT NULL PRIMARY KEY,
                    ClassId VARCHAR(20) NOT NULL,
                    Title NVARCHAR(255) NOT NULL,
                    Description NVARCHAR(1000) NULL,
                    Type NVARCHAR(50) NOT NULL,
                    Url NVARCHAR(1000) NOT NULL,
                    FileSize NVARCHAR(50) NULL,
                    UploadedAt DATETIME NOT NULL DEFAULT SYSDATETIME(),
                    CONSTRAINT FK_Materials_Class FOREIGN KEY (ClassId) REFERENCES Classes(Id) ON DELETE CASCADE
                );
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MaterialCompletions')
            BEGIN
                CREATE TABLE MaterialCompletions (
                    MaterialId NVARCHAR(50) NOT NULL,
                    StudentId VARCHAR(20) NOT NULL,
                    CompletedAt DATETIME NOT NULL DEFAULT SYSDATETIME(),
                    CONSTRAINT PK_MaterialCompletions PRIMARY KEY (MaterialId, StudentId),
                    CONSTRAINT FK_MaterialCompletions_Material FOREIGN KEY (MaterialId) REFERENCES Materials(Id) ON DELETE CASCADE,
                    CONSTRAINT FK_MaterialCompletions_Student FOREIGN KEY (StudentId) REFERENCES Users(Id) ON DELETE CASCADE
                );
            END";
        
        await context.Database.ExecuteSqlRawAsync(createTablesSql);

        // DDL cho Assignments & Submissions
        var assignmentsSql = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Assignments')
            BEGIN
                CREATE TABLE Assignments (
                    Id NVARCHAR(50) NOT NULL PRIMARY KEY,
                    ClassId VARCHAR(20) NOT NULL,
                    Title NVARCHAR(255) NOT NULL,
                    Description NVARCHAR(2000) NULL,
                    DueDate DATE NULL,
                    MaxPoints INT NOT NULL DEFAULT 100,
                    CreatedAt DATETIME NOT NULL DEFAULT SYSDATETIME(),
                    CONSTRAINT FK_Assignments_Class FOREIGN KEY (ClassId) REFERENCES Classes(Id) ON DELETE CASCADE
                );
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Submissions')
            BEGIN
                CREATE TABLE Submissions (
                    Id NVARCHAR(50) NOT NULL PRIMARY KEY,
                    AssignmentId NVARCHAR(50) NOT NULL,
                    StudentId VARCHAR(20) NOT NULL,
                    FileName NVARCHAR(500) NULL,
                    FileUrl NVARCHAR(1000) NULL,
                    StudentNotes NVARCHAR(2000) NULL,
                    SubmittedAt DATETIME NOT NULL DEFAULT SYSDATETIME(),
                    Status NVARCHAR(20) NOT NULL DEFAULT 'SUBMITTED',
                    Grade DECIMAL(5,2) NULL,
                    Feedback NVARCHAR(2000) NULL,
                    GradedAt DATETIME NULL,
                    CONSTRAINT FK_Submissions_Assignment FOREIGN KEY (AssignmentId) REFERENCES Assignments(Id) ON DELETE CASCADE,
                    CONSTRAINT FK_Submissions_Student FOREIGN KEY (StudentId) REFERENCES Users(Id)
                );
            END";
        await context.Database.ExecuteSqlRawAsync(assignmentsSql);

        // Seed Assignments nếu chưa có
        if (!await context.Assignments.AnyAsync())
        {
            var classes = await context.Classes.ToListAsync();
            foreach (var cls in classes)
            {
                var prefix = cls.Id;
                var assignments = new List<SWP.DAL.Models.Assignment>
                {
                    new SWP.DAL.Models.Assignment
                    {
                        Id          = $"asg-{prefix}-1",
                        ClassId     = cls.Id,
                        Title       = "Bài tập 1: Phân tích và thiết kế hệ thống",
                        Description = "Sinh viên thực hiện phân tích yêu cầu và thiết kế sơ đồ UML (Use Case, Class Diagram) cho hệ thống quản lý thư viện. Nộp file PDF báo cáo.",
                        DueDate     = DateOnly.FromDateTime(DateTime.Now.AddDays(7)),
                        MaxPoints   = 100,
                        CreatedAt   = DateTime.Now.AddDays(-14)
                    },
                    new SWP.DAL.Models.Assignment
                    {
                        Id          = $"asg-{prefix}-2",
                        ClassId     = cls.Id,
                        Title       = "Bài tập 2: Xây dựng RESTful API",
                        Description = "Nhóm sinh viên xây dựng ít nhất 5 API endpoints cho module quản lý người dùng, sử dụng ASP.NET Core hoặc Node.js. Kèm tài liệu Swagger.",
                        DueDate     = DateOnly.FromDateTime(DateTime.Now.AddDays(14)),
                        MaxPoints   = 100,
                        CreatedAt   = DateTime.Now.AddDays(-7)
                    },
                    new SWP.DAL.Models.Assignment
                    {
                        Id          = $"asg-{prefix}-3",
                        ClassId     = cls.Id,
                        Title       = "Đồ án cuối kỳ: Hệ thống LMS Flipped Classroom",
                        Description = "Nhóm phát triển đầy đủ hệ thống LMS gồm: quản lý người dùng, lớp học, học liệu, bài tập và điểm số. Demo live và nộp source code lên GitHub.",
                        DueDate     = DateOnly.FromDateTime(DateTime.Now.AddDays(-3)), // Đã quá hạn
                        MaxPoints   = 100,
                        CreatedAt   = DateTime.Now.AddDays(-30)
                    }
                };
                context.Assignments.AddRange(assignments);
            }
            await context.SaveChangesAsync();
        }

        // 2. Tự động seed tài liệu học tập mẫu nếu chưa có dữ liệu nào
        if (!await context.Materials.AnyAsync())
        {
            var classes = await context.Classes.ToListAsync();
            foreach (var cls in classes)
            {
                var prefix = cls.Id;
                var items = new List<Material>
                {
                    new Material
                    {
                        Id = $"mat-{prefix}-1",
                        ClassId = cls.Id,
                        Title = "Tuần 1: Giới thiệu Kiến trúc & Phương pháp Flipped Classroom",
                        Description = "Video bài giảng tổng quan về mô hình lớp học đảo ngược, so sánh lớp học truyền thống và đảo ngược. Sinh viên cần xem kỹ trước giờ học.",
                        Type = "video",
                        Url = "https://www.w3schools.com/html/mov_bbb.mp4",
                        FileSize = "45.2 MB",
                        UploadedAt = DateTime.Now.AddDays(-10)
                    },
                    new Material
                    {
                        Id = $"mat-{prefix}-2",
                        ClassId = cls.Id,
                        Title = "Tuần 1: Slide lý thuyết cơ bản và nội dung tự học",
                        Description = "Slide PDF về phương pháp tiếp cận chủ động, sơ đồ tự học 4 bước. Đọc kỹ phần chuẩn bị câu hỏi phản biện.",
                        Type = "pdf",
                        Url = "#",
                        FileSize = "3.1 MB",
                        UploadedAt = DateTime.Now.AddDays(-9)
                    },
                    new Material
                    {
                        Id = $"mat-{prefix}-3",
                        ClassId = cls.Id,
                        Title = "Tuần 2: Hướng dẫn tự học và cài đặt môi trường",
                        Description = "Video hướng dẫn thiết lập phần mềm, IDE và kết nối môi trường thực nghiệm. Thực hiện theo từng bước.",
                        Type = "video",
                        Url = "https://www.w3schools.com/html/mov_bbb.mp4",
                        FileSize = "62.5 MB",
                        UploadedAt = DateTime.Now.AddDays(-5)
                    },
                    new Material
                    {
                        Id = $"mat-{prefix}-4",
                        ClassId = cls.Id,
                        Title = "Tuần 2: Mini Quiz nghiệm thu kiến thức tự học",
                        Description = "Bài trắc nghiệm ngắn 2 câu hỏi cốt lõi để ghi nhận chuyên cần và tiến trình tự học.",
                        Type = "quiz",
                        Url = "#",
                        FileSize = "0.2 MB",
                        UploadedAt = DateTime.Now.AddDays(-4)
                    }
                };

                context.Materials.AddRange(items);
            }
            await context.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[LMS SEED ERROR] Database migration & seed failed: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlippedClassroom"));
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
