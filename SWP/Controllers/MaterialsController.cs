using System;
using System.Collections.Generic;
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
public class MaterialsController : ControllerBase
{
    private readonly IMaterialsService _materialsService;

    public MaterialsController(IMaterialsService materialsService)
    {
        _materialsService = materialsService;
    }

    /// <summary>
    /// GET /api/materials?classId={classId}
    /// Lấy danh sách tài liệu học tập của một lớp.
    /// Đối chiếu để biết học viên hiện tại đã hoàn thành chưa.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetByClass([FromQuery] string classId)
    {
        if (string.IsNullOrWhiteSpace(classId))
            return BadRequest(new { success = false, message = "Mã lớp học classId là bắt buộc." });

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value
                  ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ hoặc không tìm thấy ID người dùng." });

        var result = await _materialsService.GetMaterialsByClassAsync(classId, userId);
        return Ok(new { success = true, data = result });
    }

    /// <summary>
    /// POST /api/materials/{materialId}/complete
    /// Đánh dấu một tài liệu là đã hoàn tất bởi học viên hiện tại.
    /// </summary>
    [HttpPost("{materialId}/complete")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Complete(string materialId)
    {
        if (string.IsNullOrWhiteSpace(materialId))
            return BadRequest(new { success = false, message = "Mã tài liệu materialId là bắt buộc." });

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value
                  ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { success = false, message = "Token không hợp lệ hoặc không tìm thấy ID người dùng." });

        var success = await _materialsService.MarkMaterialCompleteAsync(materialId, userId);
        if (!success)
            return NotFound(new { success = false, message = "Không tìm thấy tài liệu học tập với mã được cung cấp." });

        return Ok(new { success = true, message = "Đánh dấu hoàn thành tài liệu thành công." });
    }
}
