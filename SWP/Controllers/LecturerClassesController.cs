using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.Lecturer;
using SWP.BLL.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SWP.Controllers;

[ApiController]
[Route("api/Lecturer")]
[Authorize(Roles = "lecturer")]
[Produces("application/json")]
public class LecturerClassesController : ControllerBase
{
    private readonly ILecturerService _lecturerService;

    public LecturerClassesController(ILecturerService lecturerService)
    {
        _lecturerService = lecturerService;
    }

    [HttpGet("classes")]
    public async Task<IActionResult> GetMyClasses()
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        var classes = await _lecturerService.GetMyClassesAsync(lecturerId);
        return Ok(new { success = true, data = classes });
    }

    [HttpGet("classes/{classId}")]
    public async Task<IActionResult> GetClassDetail(string classId)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        try
        {
            var detail = await _lecturerService.GetClassDetailAsync(lecturerId, classId);
            return Ok(new { success = true, data = detail });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
    }


    [HttpGet("classes/{classId}/students")]
    public async Task<IActionResult> GetStudents(string classId)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        try
        {
            var data = await _lecturerService.GetClassStudentsAsync(lecturerId, classId);
            return Ok(new { success = true, data });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
    }

    [HttpPut("classes/{classId}/students/{studentId}/promote")]
    public async Task<IActionResult> PromoteStudent(string classId, string studentId, [FromQuery] string role = "assistant")
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        try
        {
            await _lecturerService.PromoteStudentAsync(lecturerId, classId, studentId, role);
            var msg = role.ToLower() == "assistant"
                ? "Da thang cap tro giang thanh cong."
                : "Da ha chuc vu, hoc sinh quay lai vai tro binh thuong.";
            return Ok(new { success = true, message = msg });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { success = false, message = ex.Message }); }
    }

    private string? GetCurrentUserId() =>
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
}
