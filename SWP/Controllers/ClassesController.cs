using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.Classes;
using SWP.BLL.Interfaces;

namespace SWP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ClassesController : ControllerBase
{
    private readonly IClassesService _classesService;

    public ClassesController(IClassesService classesService)
    {
        _classesService = classesService;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? academicTermId)
    {
        var result = await _classesService.GetAllClassesAsync(academicTermId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// GET /api/classes/my-classes
    /// Trả về danh sách lớp học của sinh viên đang đăng nhập.
    /// Yêu cầu: JWT hợp lệ (bất kỳ role nào).
    /// Query params tùy chọn: academicTermId (Guid), year (string, VD: "2024-2025"), semester (string: "1"|"2"|"3")
    /// </summary>
    [HttpGet("my-classes")]
    [Authorize]  // Không giới hạn role — student, lecturer đều gọi được
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyClasses(
        [FromQuery] Guid? academicTermId,
        [FromQuery] string? year)
    {
        // Lấy studentId từ JWT claim (giống pattern trong AuthController)
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value
                  ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ hoặc không tìm thấy ID người dùng." });

        var result = await _classesService.GetMyClassesAsync(userId, academicTermId, year);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("/api/grades/my-classes")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyClassesGrades()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ hoặc không tìm thấy ID người dùng." });

        var result = await _classesService.GetMyClassesGradesAsync(userId);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("/api/classes/{classId}/grades")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetClassGrades(string classId, [FromServices] IAssignmentsService assignmentsService)
    {
        if (string.IsNullOrEmpty(classId))
            return BadRequest(new { success = false, message = "classId là bắt buộc." });

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ hoặc không tìm thấy ID người dùng." });

        var result = await assignmentsService.GetAssignmentsByClassAsync(classId, userId);
        return Ok(new { success = true, data = result });
    }

    private string? GetCurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var result = await _classesService.GetClassByIdAsync(id);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] ClassRequestDto request)
    {
        try
        {
            var result = await _classesService.CreateClassAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                new { success = true, message = "Tạo lớp học thành công.", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(string id, [FromBody] ClassRequestDto request)
    {
        try
        {
            var result = await _classesService.UpdateClassAsync(id, request);
            return Ok(new { success = true, message = "Cập nhật lớp học thành công.", data = result });
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

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _classesService.DeleteClassAsync(id);
            return Ok(new { success = true, message = "Xóa lớp học thành công." });
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
}