namespace Crawl.ECS;

public interface IComponent {}

public interface ISystem
{
    void Update(double deltaTime);
}
