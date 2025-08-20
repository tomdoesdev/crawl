namespace Crawl.Exceptions;

public class ComponentNotFoundException : NotFoundException
{
    public ComponentNotFoundException()
    {
    }

    public ComponentNotFoundException(string message) : base(message)
    {
    }

    public ComponentNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class ComponentExistsException : ExistsException
{
    public ComponentExistsException()
    {
    }

    public ComponentExistsException(string message) : base(message)
    {
    }

    public ComponentExistsException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class StorageNotFoundException : Exception
{
    public StorageNotFoundException()
    {
    }

    public StorageNotFoundException(string message) : base(message)
    {
    }

    public StorageNotFoundException(string message, Exception inner) : base(message, inner)
    {
    }
}