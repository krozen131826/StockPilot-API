using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Identity.Models;

public class TokenModel
{
    public string Token { get; set; } = string.Empty;
    public long TokenExpiration { get; set; }
}