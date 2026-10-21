using System;
using System.Collections.Generic;
using System.Text;

namespace Stockpilot.Application.Exceptions;

public class NotFoundExceptions : Exception
{
    public NotFoundExceptions(string name)
            : base($"{name} doesn't exist")
    {
    }
}
