using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using SWP.BLL.Interfaces;

namespace SWP.Controllers;

[ApiController]
[Route("api/student-classes")]
[Authorize(Roles = "student")]
[Produces("application/json")]
public class StudentClassesController : ControllerBase
{
    private readonly IStudentClassesService _studentClassesService;
    private readonly IClassStudentsService _classStudentsService; // <-- Thêm service này

    // Cập nhật Constructor để nhận cả 2 Service
    public StudentClassesController(
        IStudentClassesService studentClassesService,
        IClassStudentsService classStudentsService)
    {
        _studentClassesService = studentClassesService;
        _classStudentsService = classStudentsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyClasses([FromQuery] Guid? academicTermId)
    {
        var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(studentId))
        {
            return Unauthorized(new { success = false, message = "Token không hợp lệ." });
        }

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

    /// <summary>
    /// GET /api/student-classes/{classId}/students
    /// Cho phép sinh viên xem danh sách các bạn cùng lớp của mình.
    /// </summary>
    [HttpGet("{classId}/students")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMyClassStudents(string classId)
    {
        try
        {
            // Lấy danh sách học sinh trong lớp (gọi hàm có sẵn ở BLL)
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
}
