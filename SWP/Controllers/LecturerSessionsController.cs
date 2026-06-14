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
public class LecturerSessionsController : ControllerBase
{
    private readonly ILecturerService _lecturerService;

    public LecturerSessionsController(ILecturerService lecturerService)
    {
        _lecturerService = lecturerService;
    }

    [HttpGet("classes/{classId}/sessions")]
    public async Task<IActionResult> GetSessions(string classId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        try
        {
            var sessions = await _lecturerService.GetSessionsAsync(lecturerId, classId, from, to);
            return Ok(new { success = true, data = sessions });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
    }

    [HttpGet("classes/{classId}/sessions/{sessionId:guid}")]
    public async Task<IActionResult> GetSession(string classId, Guid sessionId)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        try
        {
            var session = await _lecturerService.GetSessionAsync(lecturerId, classId, sessionId);
            return Ok(new { success = true, data = session });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
    }

    [HttpPost("classes/{classId}/sessions")]
    public async Task<IActionResult> CreateSession(string classId, [FromBody] UpsertClassSessionDto request)
        => await SessionMutation(classId, () => _lecturerService.CreateSessionAsync(GetCurrentUserId()!, classId, request), "Tao buoi hoc thanh cong.");

    [HttpPut("classes/{classId}/sessions/{sessionId:guid}")]
    public async Task<IActionResult> UpdateSession(string classId, Guid sessionId, [FromBody] UpsertClassSessionDto request)
        => await SessionMutation(classId, () => _lecturerService.UpdateSessionAsync(GetCurrentUserId()!, classId, sessionId, request), "Cap nhat buoi hoc thanh cong.");

    [HttpDelete("classes/{classId}/sessions/{sessionId:guid}")]
    public async Task<IActionResult> DeleteSession(string classId, Guid sessionId)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        try
        {
            await _lecturerService.DeleteSessionAsync(lecturerId, classId, sessionId);
            return Ok(new { success = true, message = "Da xoa buoi hoc." });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
    }

    private async Task<IActionResult> SessionMutation(string classId, Func<Task<ClassSessionDto>> action, string message)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        try
        {
            var session = await action();
            return Ok(new { success = true, message, data = session });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { success = false, message = ex.Message }); }
    }

    private string? GetCurrentUserId() =>
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
}
