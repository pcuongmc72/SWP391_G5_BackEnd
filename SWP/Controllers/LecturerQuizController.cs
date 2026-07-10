using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.Quizzes;
using SWP.BLL.Interfaces;

namespace SWP.Controllers;

[ApiController]
[Route("api/Lecturer/quizzes")]
[Authorize(Roles = "lecturer")]
[Produces("application/json")]
public class LecturerQuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public LecturerQuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizDto dto)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        try
        {
            var data = await _quizService.CreateQuizAsync(lecturerId, dto);
            return Ok(new { success = true, data });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{quizId:guid}")]
    public async Task<IActionResult> GetQuizDetails(Guid quizId)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        try
        {
            var data = await _quizService.GetQuizDetailsForLecturerAsync(lecturerId, quizId);
            return Ok(new { success = true, data });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{quizId:guid}")]
    public async Task<IActionResult> UpdateQuiz(Guid quizId, [FromBody] UpdateQuizDto dto)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        try
        {
            var data = await _quizService.UpdateQuizAsync(lecturerId, quizId, dto);
            return Ok(new { success = true, data });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpDelete("{quizId:guid}")]
    public async Task<IActionResult> DeleteQuiz(Guid quizId)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        try
        {
            await _quizService.DeleteQuizAsync(lecturerId, quizId);
            return Ok(new { success = true, message = "Đã xóa bài trắc nghiệm." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("class/{classId}")]
    public async Task<IActionResult> GetQuizzesByClass(string classId)
    {
        try
        {
            var data = await _quizService.GetQuizzesByClassAsync(classId);
            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{quizId:guid}/attempts")]
    public async Task<IActionResult> GetClassAttempts(Guid quizId)
    {
        var lecturerId = GetCurrentUserId();
        if (lecturerId == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        try
        {
            var data = await _quizService.GetClassAttemptsAsync(lecturerId, quizId);
            return Ok(new { success = true, data });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    private string? GetCurrentUserId() =>
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
}
