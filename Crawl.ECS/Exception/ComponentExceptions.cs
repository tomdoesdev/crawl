namespace Crawl.ECS.Exception;

public class IdPoolExhaustedException : global::System.Exception
{
    public IdPoolExhaustedException()
    {
    }

    public IdPoolExhaustedException(string message) : base(message)
    {
    }

    public IdPoolExhaustedException(string message, global::System.Exception inner) : base(message, inner)
    {
    }
}

public class ComponentNotFoundException : NotFoundException
{
    public ComponentNotFoundException()
    {
    }

    public ComponentNotFoundException(string message) : base(message)
    {
    }

    public ComponentNotFoundException(string message, global::System.Exception inner) : base(message, inner)
    {
    }
}