namespace Crawl.ECS.Exception;

public class ConflictException : global::System.Exception
{
    public ConflictException()
    {
    }

    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, global::System.Exception inner) : base(message, inner)
    {
    }
}

public class NotFoundException : global::System.Exception
{
    public NotFoundException()
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, global::System.Exception inner) : base(message, inner)
    {
    }
}