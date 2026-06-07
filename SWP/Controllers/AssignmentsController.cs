using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.Interfaces;

namespace SWP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentsService _assignmentsService;

    public AssignmentsController(IAssignmentsService assignmentsService)
    {
        _assignmentsService = assignmentsService;
    }

    /// <summary>
    /// GET /api/assignments?classId={classId}
    /// Trả về danh sách bài tập của lớp học, kèm trạng thái nộp bài của sinh viên đang đăng nhập.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetByClass([FromQuery] string classId)
    {
        if (string.IsNullOrEmpty(classId))
            return BadRequest(new { success = false, message = "classId là bắt buộc." });

        var studentId = GetCurrentUserId();
        if (studentId == null)
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        var result = await _assignmentsService.GetAssignmentsByClassAsync(classId, studentId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// GET /api/assignments/{id}
    /// Trả về chi tiết một bài tập.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(string id)
    {
        var studentId = GetCurrentUserId();
        if (studentId == null)
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            var result = await _assignmentsService.GetAssignmentByIdAsync(id, studentId);
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
