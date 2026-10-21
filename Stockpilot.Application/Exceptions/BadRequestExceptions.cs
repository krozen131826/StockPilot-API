using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Exceptions;

public class BadRequestExceptions : Exception
{
    public BadRequestExceptions(string message) : base(message)
    {

    }
}