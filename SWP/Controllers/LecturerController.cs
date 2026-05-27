using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.Lecturer;
using SWP.BLL.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SWP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "lecturer")]
[Produces("application/json")]
public class LecturerController : ControllerBase
{
    private readonly ILecturerService _lecturerService;

    public LecturerController(ILecturerService lecturerService)
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

    [HttpGet("classes/{classId}/workspace")]
    public async Task<IActionResult> GetClassWorkspace(string classId)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        try
        {
            var data = await _lecturerService.GetClassWorkspaceAsync(lecturerId, classId);
            return Ok(new { success = true, data });
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

    // ─── Sessions (Schedule) ───────────────────────────────────────
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

    // ─── Materials ─────────────────────────────────────────────────
    [HttpGet("classes/{classId}/materials")]
    public async Task<IActionResult> GetMaterials(string classId)
        => await Read(classId, id => _lecturerService.GetMaterialsAsync(id, classId));

    [HttpPost("classes/{classId}/materials")]
    public async Task<IActionResult> CreateMaterial(string classId, [FromBody] UpsertMaterialDto request)
        => await Write(classId, () => _lecturerService.CreateMaterialAsync(GetCurrentUserId()!, classId, request));

    [HttpPut("classes/{classId}/materials/{materialId:guid}")]
    public async Task<IActionResult> UpdateMaterial(string classId, Guid materialId, [FromBody] UpsertMaterialDto request)
        => await Write(classId, () => _lecturerService.UpdateMaterialAsync(GetCurrentUserId()!, classId, materialId, request));

    [HttpDelete("classes/{classId}/materials/{materialId:guid}")]
    public async Task<IActionResult> DeleteMaterial(string classId, Guid materialId)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        try
        {
            await _lecturerService.DeleteMaterialAsync(lecturerId, classId, materialId);
            return Ok(new { success = true, message = "Da xoa hoc lieu." });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
    }

    [HttpPost("classes/{classId}/materials/{materialId:guid}/complete-all")]
    public async Task<IActionResult> MarkMaterialComplete(string classId, Guid materialId)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId is null) return Unauthorized();
        try
        {
            await _lecturerService.MarkMaterialCompleteAsync(lecturerId, classId, materialId);
            return Ok(new { success = true, message = "Da cap nhat hoan thanh cho ca lop." });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
    }

    // ─── Assignments ───────────────────────────────────────────────
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

    // ─── Submissions / Grading ─────────────────────────────────────
    [HttpGet("classes/{classId}/submissions")]
    public async Task<IActionResult> GetSubmissions(string classId)
        => await Read(classId, id => _lecturerService.GetSubmissionsAsync(id, classId));

    [HttpPut("classes/{classId}/submissions/{submissionId:guid}/grade")]
    public async Task<IActionResult> GradeSubmission(string classId, Guid submissionId, [FromBody] GradeSubmissionDto request)
        => await Write(classId, () => _lecturerService.GradeSubmissionAsync(GetCurrentUserId()!, classId, submissionId, request));

    // ─── Feedbacks ─────────────────────────────────────────────────
    [HttpGet("classes/{classId}/feedbacks")]
    public async Task<IActionResult> GetFeedbacks(string classId)
        => await Read(classId, id => _lecturerService.GetFeedbacksAsync(id, classId));

    [HttpPut("classes/{classId}/feedbacks/{feedbackId:guid}/respond")]
    public async Task<IActionResult> RespondFeedback(string classId, Guid feedbackId, [FromBody] RespondFeedbackDto request)
        => await Write(classId, () => _lecturerService.RespondFeedbackAsync(GetCurrentUserId()!, classId, feedbackId, request));

    // ─── Discussion ────────────────────────────────────────────────
    [HttpGet("classes/{classId}/threads")]
    public async Task<IActionResult> GetThreads(string classId)
        => await Read(classId, id => _lecturerService.GetThreadsAsync(id, classId));

    [HttpPost("classes/{classId}/threads")]
    public async Task<IActionResult> CreateThread(string classId, [FromBody] UpsertThreadDto request)
        => await Write(classId, () => _lecturerService.CreateThreadAsync(GetCurrentUserId()!, classId, request));

    [HttpPost("classes/{classId}/threads/{threadId:guid}/replies")]
    public async Task<IActionResult> CreateReply(string classId, Guid threadId, [FromBody] CreateReplyDto request)
        => await Write(classId, () => _lecturerService.CreateReplyAsync(GetCurrentUserId()!, classId, threadId, request));

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
