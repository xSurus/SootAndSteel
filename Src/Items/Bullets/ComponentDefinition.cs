using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Components;

namespace Gamelab.Items.Bullets;

public class ComponentDefinition
{
    public EComponentType Type { get; }
    public string ComponentId { get; }
    public AbstractComponent Component { get; }

    public ComponentDefinition(EComponentType type, string componentId, AbstractComponent component)
    {
        Type = type;
        ComponentId = componentId;
        Component = component;
    }
}