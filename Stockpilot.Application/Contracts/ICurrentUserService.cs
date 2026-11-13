using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Contracts;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string FullName { get; }
}