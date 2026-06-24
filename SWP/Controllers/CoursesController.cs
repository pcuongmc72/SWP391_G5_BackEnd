using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.Courses;
using SWP.BLL.Interfaces;

namespace SWP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CoursesController : ControllerBase
{
    private readonly ICoursesService _coursesService;

    public CoursesController(ICoursesService coursesService)
    {
        _coursesService = coursesService;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _coursesService.GetAllCoursesAsync();
        return Ok(new { success = true, data = result });
    }

    [HttpGet("user/{userId}")]
    [Authorize(Roles = "admin,lecturer,student")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetByUser(string userId, [FromQuery] string role)
    {
        var result = await _coursesService.GetCoursesByUserAsync(userId, role);
        return Ok(new { success = true, data = result });
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin,lecturer,student")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _coursesService.GetCourseByIdAsync(id);
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
    public async Task<IActionResult> Create([FromBody] CourseRequestDto request)
    {
        try
        {
            var result = await _coursesService.CreateCourseAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                new { success = true, message = "Thêm môn học thành công.", data = result });
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
    public async Task<IActionResult> Update(Guid id, [FromBody] CourseRequestDto request)
    {
        try
        {
            var result = await _coursesService.UpdateCourseAsync(id, request);
            return Ok(new { success = true, message = "Cập nhật môn học thành công.", data = result });
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
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _coursesService.DeleteCourseAsync(id);
            return Ok(new { success = true, message = "Đã xóa môn học khỏi hệ thống." });
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