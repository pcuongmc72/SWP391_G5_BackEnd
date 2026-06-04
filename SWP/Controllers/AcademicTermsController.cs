using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.AcademicTerms;
using SWP.BLL.Interfaces;

namespace SWP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,student")]
[Produces("application/json")]
public class AcademicTermsController : ControllerBase
{
    private readonly IAcademicTermsService _termService;

    public AcademicTermsController(IAcademicTermsService termService)
    {
        _termService = termService;
    }

    // GET: /api/academic-terms
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _termService.GetAllTermsAsync();
        return Ok(new { success = true, data = result });
    }

    // GET: /api/academic-terms/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "admin,student")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _termService.GetTermByIdAsync(id);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    // POST: /api/academic-terms
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] AcademicTermRequestDto request)
    {
        try
        {
            var result = await _termService.CreateTermAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                new { success = true, message = "Thêm học kỳ mới thành công.", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    // PUT: /api/academic-terms/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AcademicTermRequestDto request)
    {
        try
        {
            var result = await _termService.UpdateTermAsync(id, request);
            return Ok(new { success = true, message = "Cập nhật học kỳ thành công.", data = result });
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

    // DELETE: /api/academic-terms/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _termService.DeleteTermAsync(id);
            return Ok(new { success = true, message = "Xóa học kỳ thành công khỏi hệ thống." });
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
}