namespace Crawl.ECS.Exception;

public class InvalidEntityException : global::System.Exception
{
    public InvalidEntityException()
    {
    }

    public InvalidEntityException(string message) : base(message)
    {
    }

    public InvalidEntityException(string message, global::System.Exception inner) : base(message, inner)
    {
    }
}