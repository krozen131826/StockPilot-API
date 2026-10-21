using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Identity.Models;

public class AuthServiceRequestModel
{
    public string UserKey { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
