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
[Route("api/Student/quizzes")]
[Authorize(Roles = "student")]
[Produces("application/json")]
public class StudentQuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    public StudentQuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    [HttpGet("{quizId:guid}")]
    public async Task<IActionResult> GetQuizDetails(Guid quizId)
    {
        var studentId = GetCurrentUserId();
        if (studentId == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        try
        {
            var data = await _quizService.GetQuizDetailsForStudentAsync(studentId, quizId);
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

    [HttpPost("{quizId:guid}/attempts")]
    public async Task<IActionResult> StartAttempt(Guid quizId)
    {
        var studentId = GetCurrentUserId();
        if (studentId == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        try
        {
            var data = await _quizService.StartAttemptAsync(studentId, quizId);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("{quizId:guid}/attempts/{attemptId:guid}/submit")]
    public async Task<IActionResult> SubmitAttempt(Guid quizId, Guid attemptId, [FromBody] SubmitQuizDto dto)
    {
        var studentId = GetCurrentUserId();
        if (studentId == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        try
        {
            var data = await _quizService.SubmitAttemptAsync(studentId, attemptId, dto);
            return Ok(new { success = true, data });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("{quizId:guid}/attempts")]
    public async Task<IActionResult> GetMyAttempts(Guid quizId)
    {
        var studentId = GetCurrentUserId();
        if (studentId == null) return Unauthorized(new { success = false, message = "Chưa đăng nhập." });

        try
        {
            var data = await _quizService.GetMyAttemptsAsync(studentId, quizId);
            return Ok(new { success = true, data });
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
