namespace Crawl.ECS.Entity;

public class InvalidEntityException : Exception
{
    public InvalidEntityException()
    {
    }

    public InvalidEntityException(string message) : base(message)
    {
    }

    public InvalidEntityException(string message, Exception inner) : base(message, inner)
    {
    }
}