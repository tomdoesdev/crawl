namespace Crawl.ECS.System;

public class StageNotFoundException : NotFoundException
{
    public StageNotFoundException()
    {
    }

    public StageNotFoundException(string message) : base(message)
    {
    }

    public StageNotFoundException(string message, Exception inner) : base(message, inner)
    {
    }
}