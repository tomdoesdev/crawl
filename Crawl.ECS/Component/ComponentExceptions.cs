namespace Crawl.ECS.Component;

public class IdPoolExhaustedException : Exception
{
    public IdPoolExhaustedException()
    {
    }

    public IdPoolExhaustedException(string message) : base(message)
    {
    }

    public IdPoolExhaustedException(string message, Exception inner) : base(message, inner)
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

    public ComponentNotFoundException(string message, Exception inner) : base(message, inner)
    {
    }
}