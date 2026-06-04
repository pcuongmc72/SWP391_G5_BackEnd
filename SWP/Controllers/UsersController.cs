using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SWP.BLL.DTOs.Users;
using SWP.BLL.Interfaces;

namespace SWP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")] 
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _usersService.CreateUserAsync(request);
            return CreatedAtAction(nameof(GetUserById), new { id = result.Id },
                new { success = true, message = "Tạo tài khoản thành công.", data = result });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? role, [FromQuery] string? searchTerm)
    {
        var users = await _usersService.GetAllUsersAsync(role, searchTerm);
        return Ok(new { success = true, data = users });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        try
        {
            var user = await _usersService.GetUserByIdAsync(id);
            return Ok(new { success = true, data = user });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto request)
    {
        try
        {
            var result = await _usersService.UpdateUserAsync(id, request);
            return Ok(new { success = true, message = "Cập nhật thành công.", data = result });
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