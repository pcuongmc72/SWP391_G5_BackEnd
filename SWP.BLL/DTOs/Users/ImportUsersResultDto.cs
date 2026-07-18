using System.Collections.Generic;

namespace SWP.BLL.DTOs.Users;

public class ImportUsersResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public List<RowErrorDto> Errors { get; set; } = new();
}
