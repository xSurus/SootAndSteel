using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gamelab.Assets;
using Gamelab.PhysicalEntities.Bullets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Items.Bullets;

public class BulletItem : Item
{
    public EComponentType Type { get; set; }
    public List<string> ComponentIds = [];
    private List<IBulletEffect> effects = [];
    private Color color;
    public bool HasBasic { get; protected set; } = false;

    public BulletItem(string componentId) : base("Bullet")
    {
        ComponentDefinition definition = ComponentRegistry.GetDefinition(componentId);
        Type = definition.Component.Type;
        ComponentIds.Add(componentId);
        effects.Add(definition.Component);
        HasBasic = definition.Component.IsBasic;
        ComputeColor();
    }

    public BulletItem(params string[] effects) : this(effects.Select(x => new BulletItem(x)).ToArray())
    {
    }

    public BulletItem(params BulletItem[] components) : base("Bullet")
    {
        if (components == null) return;
        if (!components.Any()) return;
        
        // Copy
        if (components.Count() == 1) 
        {
            Type = components[0].Type;
            ComponentIds.AddRange(components[0].ComponentIds);
            effects.AddRange(components[0].effects);
            ComputeColor();
            return;
        }

        List<EComponentType> types = components.Select(c => c.Type).Distinct().ToList();
        
        // Upgrade to existing type
        if (types.Count == 1)
        {
            Type = components[0].Type;
            foreach (var c in components)
            {
                ComponentIds.AddRange(c.ComponentIds);
                effects.AddRange(c.effects);
                HasBasic = HasBasic || c.HasBasic;
            }

            ComputeColor();
            EnsureBasicIsFirst();
            return;
        }

        if (types.Count == 3 && types.All(t => t != EComponentType.Bullet))
        {
            Type = EComponentType.Bullet;
            foreach (var c in components)
            {
                ComponentIds.AddRange(c.ComponentIds);
                effects.AddRange(c.effects);
                HasBasic = HasBasic || c.HasBasic;
            }

            ComputeColor();
            EnsureBasicIsFirst();
            return;
        }
        
        throw new Exception("Invalid combination of components for combining bullet item");
    }

    public List<IBulletEffect> GetEffects()
    {
        return [..effects];
    }

    public bool IsEqual(Item other)
    {
        if (other == null) return false;
        if (other.GetType() != GetType()) return false;
        return ((BulletItem) other).ComponentIds.SequenceEqual(ComponentIds);
    }
    
    public override void Draw(SpriteBatch spriteBatch, Vector2 position, int size)
    {
        Vector2 origin = new Vector2(size / 2f, size / 2f);
        Rectangle sourceRect = new Rectangle(0, 0, size, size);

        spriteBatch.Draw(
            texture: AssetManager.BlankTexture,
            position: position,
            sourceRectangle: sourceRect,
            color: color,
            rotation: 0f,
            origin: origin,
            scale: 1f,
            effects: SpriteEffects.None,
            layerDepth: 0f
        );
    }

    private void ComputeColor()
    {
        List<float> colorComponents = ComponentIds
            .Select(ComponentRegistry.GetColor)
            .Select(x => (List<float>)[x.R, x.G, x.B])
            .Aggregate((a, b) => [a[0] + b[0], a[1] + b[1], a[2] + b[2]]);
        color = new Color(
            (byte)(colorComponents[0] / ComponentIds.Count),
            (byte)(colorComponents[1] / ComponentIds.Count),
            (byte)(colorComponents[2] / ComponentIds.Count)
        );
    }

    private void EnsureBasicIsFirst()
    {
        if (effects.First().IsBasic) return;
        int basicIdx = effects.FindIndex(x => x.IsBasic);
        IBulletEffect basicEffect = effects[basicIdx];
        effects.RemoveAt(basicIdx);
        effects.Insert(0, basicEffect);
    }
}