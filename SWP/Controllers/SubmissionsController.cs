using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.Submissions;
using SWP.BLL.Interfaces;

namespace SWP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionsService _submissionsService;

    public SubmissionsController(ISubmissionsService submissionsService)
    {
        _submissionsService = submissionsService;
    }

    /// <summary>
    /// GET /api/submissions/my-submissions?classId={classId}
    /// Lấy danh sách bài nộp của sinh viên hiện tại (có thể lọc theo lớp).
    /// </summary>
    [HttpGet("my-submissions")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMySubmissions([FromQuery] string? classId)
    {
        var studentId = GetCurrentUserId();
        if (studentId == null)
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        var result = await _submissionsService.GetMySubmissionsAsync(studentId, classId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// POST /api/submissions
    /// Nộp bài tập (tạo mới hoặc cập nhật nếu đã nộp trước đó).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Submit([FromBody] SubmitAssignmentRequestDto request)
    {
        if (request.AssignmentId == Guid.Empty)
            return BadRequest(new { success = false, message = "AssignmentId là bắt buộc." });

        var studentId = GetCurrentUserId();
        if (studentId == null)
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            var result = await _submissionsService.SubmitAssignmentAsync(studentId, request);
            return Ok(new { success = true, message = "Nộp bài thành công.", data = result });
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

    [HttpGet("/api/submissions/{submissionId}/grade-detail")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetGradeDetail(string submissionId)
    {
        if (string.IsNullOrEmpty(submissionId))
            return BadRequest(new { success = false, message = "submissionId là bắt buộc." });

        var studentId = GetCurrentUserId();
        if (studentId == null)
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            var result = await _submissionsService.GetSubmissionGradeDetailAsync(submissionId, studentId);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    private string? GetCurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
}
