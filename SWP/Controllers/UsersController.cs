using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.Users;
using SWP.BLL.Interfaces;
using System.IO;

namespace SWP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")] 
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _usersService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { id = result.Id },
                new { success = true, message = "Tạo tài khoản thành công.", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? role, [FromQuery] string? searchTerm)
    {
        var users = await _usersService.GetAllUsersAsync(role, searchTerm);
        return Ok(new { success = true, data = users });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        try
        {
            var user = await _usersService.GetUserByIdAsync(id);
            return Ok(new { success = true, data = user });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto request)
    {
        try
        {
            var result = await _usersService.UpdateUserAsync(id, request);
            return Ok(new { success = true, message = "Cập nhật thành công.", data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportUsers(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "Vui lòng chọn một file Excel hoặc CSV." });

        var extension = Path.GetExtension(file.FileName);
        if (extension != ".xlsx" && extension != ".xls" && extension != ".csv")
            return BadRequest(new { success = false, message = "Định dạng file không hỗ trợ. Vui lòng tải lên file .xlsx, .xls hoặc .csv." });

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _usersService.ImportUsersAsync(stream, extension);
            if (result.Errors != null && result.Errors.Any())
            {
                return BadRequest(new { success = false, message = "Nhập dữ liệu thất bại do có lỗi validation.", data = result });
            }
            return Ok(new { success = true, message = $"Nhập thành công {result.SuccessCount} tài khoản.", data = result });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi hệ thống: " + ex.Message });
        }
    }

    [HttpGet("import-template")]
    public IActionResult GetImportTemplate()
    {
        var csvHeader = "Id,Email,FullName,Password,Role,Phone,Address,Bio\nHE187159,student@fpt.edu.vn,Nguyen Van A,student123,Student,0912345678,Hanoi,Sinh viên kì 5\nGV123456,lecturer@fpt.edu.vn,Nguyen Van B,lecturer123,Lecturer,0987654321,Hanoi,Giảng viên bộ môn Kỹ thuật phần mềm";
        var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csvHeader)).ToArray();
        return File(bytes, "text/csv", "user_import_template.csv");
    }
}