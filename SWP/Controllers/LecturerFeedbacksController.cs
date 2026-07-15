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
public class LecturerFeedbacksController : ControllerBase
{
    private readonly ILecturerService _lecturerService;

    public LecturerFeedbacksController(ILecturerService lecturerService)
    {
        _lecturerService = lecturerService;
    }

    [HttpGet("classes/{classId}/feedbacks")]
    public async Task<IActionResult> GetFeedbacks(string classId)
        => await Read(classId, id => _lecturerService.GetFeedbacksAsync(id, classId));


    [HttpPut("classes/{classId}/feedbacks/{feedbackId:guid}/respond")]
    public async Task<IActionResult> RespondFeedback(string classId, Guid feedbackId, [FromBody] RespondFeedbackDto request)
        => await Write(classId, () => _lecturerService.RespondFeedbackAsync(GetCurrentUserId()!, classId, feedbackId, request));

    private async Task<IActionResult> Read<T>(string classId, Func<string, Task<IReadOnlyList<T>>> action)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        try
        {
            var data = await action(lecturerId);
            return Ok(new { success = true, data });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
    }

    private async Task<IActionResult> Write<T>(string classId, Func<Task<T>> action)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        try
        {
            var data = await action();
            return Ok(new { success = true, data });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { success = false, message = ex.Message }); }
    }

    private string? GetCurrentUserId() =>
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
}
