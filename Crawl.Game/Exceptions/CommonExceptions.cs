namespace Crawl.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception inner) : base(message, inner)
    {
    }
}

public class ExistsException : Exception
{
    public ExistsException()
    {
    }

    public ExistsException(string message) : base(message)
    {
    }

    public ExistsException(string message, Exception inner) : base(message, inner)
    {
    }
}