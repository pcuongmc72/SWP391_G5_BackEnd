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
public class LecturerAssignmentsController : ControllerBase
{
    private readonly ILecturerService _lecturerService;

    public LecturerAssignmentsController(ILecturerService lecturerService)
    {
        _lecturerService = lecturerService;
    }

    [HttpGet("classes/{classId}/assignments")]
    public async Task<IActionResult> GetAssignments(string classId)
        => await Read(classId, id => _lecturerService.GetAssignmentsAsync(id, classId));

    [HttpPost("classes/{classId}/assignments")]
    public async Task<IActionResult> CreateAssignment(string classId, [FromBody] UpsertAssignmentDto request)
        => await Write(classId, () => _lecturerService.CreateAssignmentAsync(GetCurrentUserId()!, classId, request));

    [HttpPut("classes/{classId}/assignments/{assignmentId:guid}")]
    public async Task<IActionResult> UpdateAssignment(string classId, Guid assignmentId, [FromBody] UpsertAssignmentDto request)
        => await Write(classId, () => _lecturerService.UpdateAssignmentAsync(GetCurrentUserId()!, classId, assignmentId, request));

    [HttpDelete("classes/{classId}/assignments/{assignmentId:guid}")]
    public async Task<IActionResult> DeleteAssignment(string classId, Guid assignmentId)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        try
        {
            await _lecturerService.DeleteAssignmentAsync(lecturerId, classId, assignmentId);
            return Ok(new { success = true, message = "Da xoa bai tap." });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
    }

    [HttpGet("classes/{classId}/submissions")]
    public async Task<IActionResult> GetSubmissions(string classId)
        => await Read(classId, id => _lecturerService.GetSubmissionsAsync(id, classId));

    [HttpPut("classes/{classId}/submissions/{submissionId:guid}/grade")]
    public async Task<IActionResult> GradeSubmission(string classId, Guid submissionId, [FromBody] GradeSubmissionDto request)
        => await Write(classId, () => _lecturerService.GradeSubmissionAsync(GetCurrentUserId()!, classId, submissionId, request));

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
