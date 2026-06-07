using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.Interfaces;

namespace SWP.Controllers;

[ApiController]
[Route("api/student-learning-materials")]
[Produces("application/json")]
public class StudentLearningMaterialsController : ControllerBase
{
    private readonly IStudentLearningMaterialsService _service;

    public StudentLearningMaterialsController(IStudentLearningMaterialsService service)
    {
        _service = service;
    }

    private string? GetCurrentUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value
        ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    /// <summary>GET /api/student-learning-materials/class/{classId} — Lấy danh sách học liệu của lớp kèm trạng thái hoàn thành của sinh viên hiện tại.</summary>
    [HttpGet("class/{classId}")]
    [Authorize]
    public async Task<IActionResult> GetByClass(string classId)
    {
        var studentId = GetCurrentUserId();
        if (string.IsNullOrEmpty(studentId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            var result = await _service.GetMaterialsForStudentAsync(classId, studentId);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>POST /api/student-learning-materials/{materialId}/complete — Đánh dấu hoàn thành học liệu.</summary>
    [HttpPost("{materialId}/complete")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> Complete(Guid materialId)
    {
        var studentId = GetCurrentUserId();
        if (string.IsNullOrEmpty(studentId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            await _service.MarkAsCompletedAsync(materialId, studentId);
            return Ok(new { success = true, message = "Đã đánh dấu hoàn thành." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>DELETE /api/student-learning-materials/{materialId}/complete — Bỏ đánh dấu hoàn thành học liệu.</summary>
    [HttpDelete("{materialId}/complete")]
    [Authorize(Roles = "student")]
    public async Task<IActionResult> Uncomplete(Guid materialId)
    {
        var studentId = GetCurrentUserId();
        if (string.IsNullOrEmpty(studentId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });

        try
        {
            await _service.UnmarkAsCompletedAsync(materialId, studentId);
            return Ok(new { success = true, message = "Đã bỏ hoàn thành." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
