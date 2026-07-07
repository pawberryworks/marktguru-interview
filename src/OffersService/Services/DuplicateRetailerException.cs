using System;

namespace OffersService.Services;

public class DuplicateRetailerException : Exception
{
    public DuplicateRetailerException(string message) : base(message) { }
}
