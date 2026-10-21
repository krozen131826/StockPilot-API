using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Identity.Models;

public class AuthServiceResponseModel
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public long TokenExpiration { get; set; }
}
