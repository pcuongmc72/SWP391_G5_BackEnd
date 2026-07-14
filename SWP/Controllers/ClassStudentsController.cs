using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.ClassStudents;
using SWP.BLL.Interfaces;

namespace SWP.Controllers;

[ApiController]
[Route("api/classes/{classId}/students")]
[Authorize(Roles = "admin")] 
[Produces("application/json")]
public class ClassStudentsController : ControllerBase
{
    private readonly IClassStudentsService _studentService;

    public ClassStudentsController(IClassStudentsService studentService)
    {
        _studentService = studentService;
    }

    // 1. GET /api/classes/SE1908/students
    [HttpGet]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetStudents(string classId)
    {
        try
        {
            var result = await _studentService.GetStudentsInClassAsync(classId);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    // 2. POST /api/classes/SE1908/students
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddStudent(string classId, [FromBody] AddStudentRequestDto request)
    {
        try
        {
            var result = await _studentService.AddStudentToClassAsync(classId, request);
            return Ok(new { success = true, message = "Đã thêm học viên vào lớp thành công.", data = result });
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

    // 3. DELETE /api/classes/SE1908/students/HE187159
    [HttpDelete("{studentId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveStudent(string classId, string studentId)
    {
        try
        {
            await _studentService.RemoveStudentFromClassAsync(classId, studentId);
            return Ok(new { success = true, message = "Đã xóa học viên khỏi lớp." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }
}