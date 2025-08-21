namespace Crawl.ECS.Exception;

public class StageNotFoundException : NotFoundException
{
    public StageNotFoundException()
    {
    }

    public StageNotFoundException(string message) : base(message)
    {
    }

    public StageNotFoundException(string message, global::System.Exception inner) : base(message, inner)
    {
    }
}