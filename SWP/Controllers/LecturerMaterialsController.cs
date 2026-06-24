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
public class LecturerMaterialsController : ControllerBase
{
    private readonly ILecturerService _lecturerService;

    public LecturerMaterialsController(ILecturerService lecturerService)
    {
        _lecturerService = lecturerService;
    }

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
