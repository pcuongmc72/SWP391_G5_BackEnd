using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.Classes;
using SWP.BLL.Interfaces;

namespace SWP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ClassesController : ControllerBase
{
    private readonly IClassesService _classesService;

    public ClassesController(IClassesService classesService)
    {
        _classesService = classesService;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? academicTermId)
    {
        var result = await _classesService.GetAllClassesAsync(academicTermId);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<IEnumerable<ClassResponseDto>>> GetByUser(string userId, [FromQuery] string role, [FromQuery] Guid? academicTermId)
    {
        var result = await _classesService.GetClassesByUserAsync(userId, role, academicTermId);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var result = await _classesService.GetClassByIdAsync(id);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] ClassRequestDto request)
    {
        try
        {
            var result = await _classesService.CreateClassAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                new { success = true, message = "Tạo lớp học thành công.", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(string id, [FromBody] ClassRequestDto request)
    {
        try
        {
            var result = await _classesService.UpdateClassAsync(id, request);
            return Ok(new { success = true, message = "Cập nhật lớp học thành công.", data = result });
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

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _classesService.DeleteClassAsync(id);
            return Ok(new { success = true, message = "Xóa lớp học thành công." });
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

    [HttpGet("{id}/materials")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetClassMaterials(string id)
    {
        try
        {
            var result = await _classesService.GetClassMaterialsAsync(id);
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
}