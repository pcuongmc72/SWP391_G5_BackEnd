using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using SWP.BLL.DTOs.Lecturer;
using SWP.BLL.Interfaces;

namespace SWP.Controllers;

[ApiController]
[Route("api/student-classes")]
[Authorize(Roles = "student")]
[Produces("application/json")]
public class StudentClassesController : ControllerBase
{
    private readonly IStudentClassesService _studentClassesService;
    private readonly IClassStudentsService _classStudentsService;

    public StudentClassesController(
        IStudentClassesService studentClassesService,
        IClassStudentsService classStudentsService)
    {
        _studentClassesService = studentClassesService;
        _classStudentsService = classStudentsService;
    }

    // ─── GET /api/student-classes  ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetMyClasses([FromQuery] Guid? academicTermId)
    {
        var studentId = GetCurrentStudentId();
        if (string.IsNullOrEmpty(studentId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            var result = await _studentClassesService.GetClassesForStudentAsync(studentId, academicTermId);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ─── GET /api/student-classes/{classId}/students ───────────────────────────
    /// <summary>Cho phép sinh viên xem danh sách bạn cùng lớp.</summary>
    [HttpGet("{classId}/students")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMyClassStudents(string classId)
    {
        try
        {
            var result = await _classStudentsService.GetStudentsInClassAsync(classId);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ─── GET /api/student-classes/{classId}/roadmap ────────────────────────────
    /// <summary>Trả về lộ trình học tập của lớp, nhóm theo Chapter, kèm trạng thái hoàn thành.</summary>
    [HttpGet("{classId}/roadmap")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMyClassRoadmap(string classId)
    {
        var studentId = GetCurrentStudentId();
        if (string.IsNullOrEmpty(studentId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            var result = await _studentClassesService.GetClassRoadmapAsync(studentId, classId);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ─── POST /api/student-classes/materials/{materialId}/complete ─────────────
    /// <summary>Đánh dấu học liệu là đã hoàn thành.</summary>
    [HttpPost("materials/{materialId:guid}/complete")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CompleteMaterial(Guid materialId)
    {
        var studentId = GetCurrentStudentId();
        if (string.IsNullOrEmpty(studentId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            var success = await _studentClassesService.CompleteMaterialAsync(studentId, materialId);
            return Ok(new { success, message = "Đánh dấu hoàn thành thành công." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ─── POST /api/student-classes/materials/{materialId}/uncomplete ───────────
    /// <summary>Hủy đánh dấu hoàn thành học liệu.</summary>
    [HttpPost("materials/{materialId:guid}/uncomplete")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UncompleteMaterial(Guid materialId)
    {
        var studentId = GetCurrentStudentId();
        if (string.IsNullOrEmpty(studentId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            var success = await _studentClassesService.UncompleteMaterialAsync(studentId, materialId);
            return Ok(new { success, message = "Hủy đánh dấu hoàn thành thành công." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ─── GET /api/student-classes/{classId}/assignments ─────────────────────────
    /// <summary>Trả về danh sách bài tập của lớp kèm trạng thái nộp bài của sinh viên hiện tại.</summary>
    [HttpGet("{classId}/assignments")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetMyAssignments(string classId)
    {
        var studentId = GetCurrentStudentId();
        if (string.IsNullOrEmpty(studentId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            var result = await _studentClassesService.GetStudentAssignmentsAsync(studentId, classId);
            return Ok(new { success = true, data = result });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ─── POST /api/student-classes/{classId}/assignments/{assignmentId}/submit ──────
    /// <summary>Sinh viên nộp hoặc cập nhật bài tập. Cho phép nộp lại khi chưa chấm điểm.</summary>
    [HttpPost("{classId}/assignments/{assignmentId:guid}/submit")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SubmitAssignment(
        string classId, Guid assignmentId, [FromBody] SubmitAssignmentRequestDto request)
    {
        var studentId = GetCurrentStudentId();
        if (string.IsNullOrEmpty(studentId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            var result = await _studentClassesService.SubmitAssignmentAsync(studentId, classId, assignmentId, request);
            return Ok(new { success = true, data = result, message = "Nộp bài thành công." });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // ─── Support Feedbacks ───────────────────────────────────────────────────

    [HttpGet("{classId}/feedbacks")]
    public async Task<IActionResult> GetFeedbacks(string classId)
    {
        var studentId = GetCurrentStudentId();
        if (studentId == null) return Unauthorized();

        try
        {
            var data = await _studentClassesService.GetFeedbacksAsync(studentId, classId);
            return Ok(new { success = true, data });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
    }

    [HttpPost("{classId}/feedbacks")]
    public async Task<IActionResult> CreateFeedback(string classId, [FromBody] SWP.BLL.DTOs.Lecturer.CreateFeedbackDto request)
    {
        var studentId = GetCurrentStudentId();
        if (studentId == null) return Unauthorized();

        try
        {
            var data = await _studentClassesService.CreateFeedbackAsync(studentId, classId, request);
            return Ok(new { success = true, data });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
    }

    [HttpPut("{classId}/feedbacks/{feedbackId:guid}/respond")]
    public async Task<IActionResult> RespondFeedbackAsAssistant(string classId, Guid feedbackId, [FromBody] SWP.BLL.DTOs.Lecturer.RespondFeedbackDto request)
    {
        var studentId = GetCurrentStudentId();
        if (studentId == null) return Unauthorized();

        try
        {
            var data = await _studentClassesService.RespondFeedbackAsAssistantAsync(studentId, classId, feedbackId, request);
            return Ok(new { success = true, data });
        }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { success = false, message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { success = false, message = ex.Message }); }
    }

    // ─── Helper ────────────────────────────────────────────────────────────────
    private string? GetCurrentStudentId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
}
