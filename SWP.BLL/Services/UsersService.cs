using Microsoft.EntityFrameworkCore;
using SWP.BLL.DTOs.Users;
using SWP.BLL.Interfaces;
using SWP.DAL.Context;
using SWP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using System.IO;
using System.Data;

namespace SWP.BLL.Services
{
    public class UsersService : IUsersService
    {
        private readonly FlippedClassroomContext _context;

        public UsersService(FlippedClassroomContext context)
        {
            _context = context;
        }

        public async Task<UserResponseDto> CreateUserAsync(RegisterRequestDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Id == request.Id))
                throw new InvalidOperationException("Mã định danh (Id) này đã tồn tại trên hệ thống.");

            if (await _context.Users.AnyAsync(u => u.Email == email))
                throw new InvalidOperationException("Email này đã tồn tại trên hệ thống.");

            var newUser = new User
            {
                Id = request.Id,
                Email = email,
                FullName = request.FullName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role.ToLower(),
                AvatarUrl = request.AvatarUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return MapToDto(newUser);
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(string? role, string? searchTerm)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role == role.ToLower());

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(u => u.FullName.Contains(searchTerm)
                                      || u.Email.Contains(searchTerm)
                                      || u.Id.Contains(searchTerm));

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserResponseDto> GetUserByIdAsync(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new KeyNotFoundException("Không tìm thấy người dùng.");

            return MapToDto(user);
        }

        public async Task<UserResponseDto> UpdateUserAsync(string id, UpdateUserDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                throw new KeyNotFoundException("Không tìm thấy người dùng.");

            var email = request.Email.Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != id))
                throw new InvalidOperationException("Email này đã được sử dụng bởi người dùng khác.");

            user.FullName = request.FullName;
            user.Email = email;
            user.AvatarUrl = request.AvatarUrl;
            user.IsActive = request.IsActive;

            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(user);
        }

        public async Task<ImportUsersResultDto> ImportUsersAsync(Stream fileStream, string fileExtension)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var reader = fileExtension.Equals(".csv", StringComparison.OrdinalIgnoreCase)
                ? ExcelReaderFactory.CreateCsvReader(fileStream)
                : ExcelReaderFactory.CreateReader(fileStream);

            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true
                }
            });

            var errors = new List<RowErrorDto>();

            if (result.Tables.Count == 0)
            {
                errors.Add(new RowErrorDto { RowIndex = 1, ErrorMessage = "File không chứa bất kỳ bảng dữ liệu nào." });
                return new ImportUsersResultDto { TotalRows = 0, SuccessCount = 0, Errors = errors };
            }

            var table = result.Tables[0];
            if (table.Rows.Count == 0)
            {
                errors.Add(new RowErrorDto { RowIndex = 1, ErrorMessage = "File không chứa bất kỳ dòng dữ liệu nào ngoài dòng tiêu đề." });
                return new ImportUsersResultDto { TotalRows = 0, SuccessCount = 0, Errors = errors };
            }

            int colId = -1, colEmail = -1, colFullName = -1, colPassword = -1, colRole = -1;
            int colPhone = -1, colAddress = -1, colBio = -1;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                var columnName = table.Columns[i].ColumnName?.Trim().ToLowerInvariant();
                if (columnName == "id" || columnName == "mã định danh" || columnName == "mã số" || columnName == "mã")
                    colId = i;
                else if (columnName == "email")
                    colEmail = i;
                else if (columnName == "fullname" || columnName == "họ và tên" || columnName == "họ tên" || columnName == "tên")
                    colFullName = i;
                else if (columnName == "password" || columnName == "mật khẩu" || columnName == "mật mã")
                    colPassword = i;
                else if (columnName == "role" || columnName == "vai trò")
                    colRole = i;
                else if (columnName == "phone" || columnName == "số điện thoại" || columnName == "sđt")
                    colPhone = i;
                else if (columnName == "address" || columnName == "địa chỉ")
                    colAddress = i;
                else if (columnName == "bio" || columnName == "tiểu sử")
                    colBio = i;
            }

            if (colId == -1) errors.Add(new RowErrorDto { RowIndex = 1, ErrorMessage = "Không tìm thấy cột 'Id' hoặc 'Mã định danh'." });
            if (colEmail == -1) errors.Add(new RowErrorDto { RowIndex = 1, ErrorMessage = "Không tìm thấy cột 'Email'." });
            if (colFullName == -1) errors.Add(new RowErrorDto { RowIndex = 1, ErrorMessage = "Không tìm thấy cột 'FullName' hoặc 'Họ và tên'." });
            if (colPassword == -1) errors.Add(new RowErrorDto { RowIndex = 1, ErrorMessage = "Không tìm thấy cột 'Password' hoặc 'Mật khẩu'." });
            if (colRole == -1) errors.Add(new RowErrorDto { RowIndex = 1, ErrorMessage = "Không tìm thấy cột 'Role' hoặc 'Vai trò'." });

            if (errors.Any())
            {
                return new ImportUsersResultDto { TotalRows = 0, SuccessCount = 0, Errors = errors };
            }

            var seenIdsInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenEmailsInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var rowsToImport = new List<UserImportRow>();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                int excelRow = i + 2; // Hàng tiêu đề là 1, dòng dữ liệu đầu tiên là 2
                var row = table.Rows[i];

                var id = row[colId]?.ToString()?.Trim();
                var email = row[colEmail]?.ToString()?.Trim();
                var fullName = row[colFullName]?.ToString()?.Trim();
                var password = row[colPassword]?.ToString()?.Trim();
                var role = row[colRole]?.ToString()?.Trim();
                var phone = colPhone != -1 ? row[colPhone]?.ToString()?.Trim() : null;
                var address = colAddress != -1 ? row[colAddress]?.ToString()?.Trim() : null;
                var bio = colBio != -1 ? row[colBio]?.ToString()?.Trim() : null;

                bool hasRowError = false;

                if (string.IsNullOrEmpty(id))
                {
                    errors.Add(new RowErrorDto { RowIndex = excelRow, ErrorMessage = "Mã định danh (Id) không được để trống." });
                    hasRowError = true;
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(id, @"^[A-Z]{2}\d{6}$"))
                {
                    errors.Add(new RowErrorDto { RowIndex = excelRow, ErrorMessage = $"Mã định danh '{id}' không hợp lệ. Phải bắt đầu bằng 2 chữ cái in hoa và theo sau là đúng 6 chữ số (VD: HE187159)." });
                    hasRowError = true;
                }

                if (string.IsNullOrEmpty(email))
                {
                    errors.Add(new RowErrorDto { RowIndex = excelRow, ErrorMessage = "Email không được để trống." });
                    hasRowError = true;
                }
                else
                {
                    try
                    {
                        var addr = new System.Net.Mail.MailAddress(email);
                        if (addr.Address != email) throw new Exception();
                    }
                    catch
                    {
                        errors.Add(new RowErrorDto { RowIndex = excelRow, ErrorMessage = $"Email '{email}' không đúng định dạng." });
                        hasRowError = true;
                    }
                }

                if (string.IsNullOrEmpty(fullName))
                {
                    errors.Add(new RowErrorDto { RowIndex = excelRow, ErrorMessage = "Họ và tên không được để trống." });
                    hasRowError = true;
                }

                if (string.IsNullOrEmpty(password))
                {
                    errors.Add(new RowErrorDto { RowIndex = excelRow, ErrorMessage = "Mật khẩu không được để trống." });
                    hasRowError = true;
                }
                else if (password.Length < 6)
                {
                    errors.Add(new RowErrorDto { RowIndex = excelRow, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự." });
                    hasRowError = true;
                }

                if (string.IsNullOrEmpty(role))
                {
                    errors.Add(new RowErrorDto { RowIndex = excelRow, ErrorMessage = "Vai trò (Role) không được để trống." });
                    hasRowError = true;
                }
                else
                {
                    var normalizedRole = role.ToLowerInvariant();
                    if (normalizedRole != "admin" && normalizedRole != "lecturer" && normalizedRole != "student")
                    {
                        errors.Add(new RowErrorDto { RowIndex = excelRow, ErrorMessage = $"Vai trò '{role}' không hợp lệ. Chỉ được phép là: Admin, Lecturer, Student." });
                        hasRowError = true;
                    }
                }

                if (hasRowError) continue;

                // Kiểm tra trùng lặp trong nội bộ file
                if (id != null)
                {
                    if (seenIdsInFile.Contains(id))
                    {
                        errors.Add(new RowErrorDto { RowIndex = excelRow, ErrorMessage = $"Mã định danh '{id}' bị lặp lại nhiều lần trong file." });
                        hasRowError = true;
                    }
                    else
                    {
                        seenIdsInFile.Add(id);
                    }
                }

                if (email != null)
                {
                    if (seenEmailsInFile.Contains(email))
                    {
                        errors.Add(new RowErrorDto { RowIndex = excelRow, ErrorMessage = $"Email '{email}' bị lặp lại nhiều lần trong file." });
                        hasRowError = true;
                    }
                    else
                    {
                        seenEmailsInFile.Add(email);
                    }
                }

                if (!hasRowError)
                {
                    rowsToImport.Add(new UserImportRow
                    {
                        RowIndex = excelRow,
                        Id = id!,
                        Email = email!,
                        FullName = fullName!,
                        Password = password!,
                        Role = role!,
                        Phone = phone,
                        Address = address,
                        Bio = bio
                    });
                }
            }

            // Kiểm tra trùng lặp với database (chỉ với những dòng không có lỗi định dạng ban đầu)
            if (rowsToImport.Any())
            {
                var idsToCheck = rowsToImport.Select(r => r.Id).ToList();
                var emailsToCheck = rowsToImport.Select(r => r.Email.ToLowerInvariant()).ToList();

                var existingIds = await _context.Users
                    .Where(u => idsToCheck.Contains(u.Id))
                    .Select(u => u.Id)
                    .ToListAsync();

                var existingEmails = await _context.Users
                    .Where(u => emailsToCheck.Contains(u.Email.ToLower()))
                    .Select(u => u.Email)
                    .ToListAsync();

                var existingIdSet = new HashSet<string>(existingIds, StringComparer.OrdinalIgnoreCase);
                var existingEmailSet = new HashSet<string>(existingEmails, StringComparer.OrdinalIgnoreCase);

                foreach (var row in rowsToImport)
                {
                    if (existingIdSet.Contains(row.Id))
                    {
                        errors.Add(new RowErrorDto { RowIndex = row.RowIndex, ErrorMessage = $"Mã định danh '{row.Id}' đã tồn tại trong cơ sở dữ liệu." });
                    }
                    if (existingEmailSet.Contains(row.Email))
                    {
                        errors.Add(new RowErrorDto { RowIndex = row.RowIndex, ErrorMessage = $"Email '{row.Email}' đã tồn tại trong cơ sở dữ liệu." });
                    }
                }
            }

            // Nếu có bất kỳ lỗi nào trong toàn bộ quá trình duyệt file
            if (errors.Any())
            {
                return new ImportUsersResultDto
                {
                    TotalRows = table.Rows.Count,
                    SuccessCount = 0,
                    Errors = errors.OrderBy(e => e.RowIndex).ToList()
                };
            }

            // Thực hiện ghi danh sách tài khoản hợp lệ vào Database
            var usersToInsert = new List<User>();
            foreach (var row in rowsToImport)
            {
                var newUser = new User
                {
                    Id = row.Id,
                    Email = row.Email.Trim().ToLowerInvariant(),
                    FullName = row.FullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(row.Password),
                    Role = row.Role.ToLowerInvariant(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Phone = row.Phone,
                    Address = row.Address,
                    Bio = row.Bio
                };
                usersToInsert.Add(newUser);
            }

            _context.Users.AddRange(usersToInsert);
            await _context.SaveChangesAsync();

            return new ImportUsersResultDto
            {
                TotalRows = table.Rows.Count,
                SuccessCount = usersToInsert.Count,
                Errors = new List<RowErrorDto>()
            };
        }

        private class UserImportRow
        {
            public int RowIndex { get; set; }
            public string Id { get; set; } = null!;
            public string Email { get; set; } = null!;
            public string FullName { get; set; } = null!;
            public string Password { get; set; } = null!;
            public string Role { get; set; } = null!;
            public string? Phone { get; set; }
            public string? Address { get; set; }
            public string? Bio { get; set; }
        }

        

        // Hàm phụ trợ map từ Entity sang DTO
        private static UserResponseDto MapToDto(User user) => new()
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
