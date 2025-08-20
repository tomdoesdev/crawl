namespace Crawl.Game.ECS;

/// <summary>
/// Components are pure data containers
/// No behavior, no methods (except maybe simple properties)
/// </summary>
public interface IComponent
{
    /// <summary>
    /// Unique ID for this specific component instance
    /// </summary>
    uint ComponentId { get; set; }
    
    /// <summary>
    /// Which Entity owns this component
    /// </summary>
    uint EntityId { get; set; }
}