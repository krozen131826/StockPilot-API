using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Exceptions;

public class UnAuthorizedExceptions : Exception
{
    public UnAuthorizedExceptions(string message) : base(message)
    {

    }
}
