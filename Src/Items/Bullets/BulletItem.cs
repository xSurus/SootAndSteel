using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Assets;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Components;
using Gamelab.Utils;
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
        AbstractComponent definition = ComponentFactory.CreateDefinition(componentId);
        Type = definition.Type;
        ComponentIds.Add(componentId);
        effects.Add(definition);
        HasBasic = definition.IsBasic;
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
        return effects;
    }

    public bool IsEqual(Item other)
    {
        if (other == null) return false;
        if (other.GetType() != GetType()) return false;
        return ((BulletItem)other).GetEffects().Select(k => k.Guid).SequenceEqual(effects.Select(k => k.Guid));
    }

    public override void Draw(SpriteBatch spriteBatch, Vector2 position, int size, float depth, float rotation = 0f)
    {
        Texture2D baseTex = AssetManager.BlankTexture;
        Color drawColor = color;
        float scale = 1f;
        Rectangle? sourceRect = new Rectangle(0, 0, size, size);
        Vector2 origin = new Vector2(size / 2f, size);

        string baseTexName = (Type == EComponentType.Bullet) ? "Bullet" : $"Basic{Type.ToString()}";

        Texture2D loadedTex = AssetManager.GetItemTexture(baseTexName);
        if (loadedTex != null)
        {
            baseTex = loadedTex;
            drawColor = Color.White;
            scale = size / (float)baseTex.Width;
            sourceRect = null;
            origin = rotation == 0f
                ? new Vector2(baseTex.Width / 2f, baseTex.Height)
                : new Vector2(baseTex.Width / 2f, baseTex.Height / 2f);
        }

        spriteBatch.Draw(
            texture: baseTex,
            position: position,
            sourceRectangle: sourceRect,
            color: drawColor,
            rotation: rotation,
            origin: origin,
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: depth
        );

        string badgeTexName = null;
        if (Type != EComponentType.Bullet && !HasBasic)
        {
            badgeTexName = "Upgrade";
        }
        else if (effects.Any(e => !e.IsBasic))
        {
            badgeTexName = "Upgraded";
        }

        if (!string.IsNullOrEmpty(badgeTexName))
        {
            Texture2D badgeTex = AssetManager.GetItemTexture(badgeTexName);
            if (badgeTex != null)
            {
                float spriteWidth = (sourceRect == null) ? baseTex.Width * scale : size;
                float spriteHeight = (sourceRect == null) ? baseTex.Height * scale : size;

                float xOffset = 0;
                float yOffset = 0;

                Vector2 topRightPos = new Vector2(
                    position.X + (spriteWidth / 2f) - xOffset,
                    position.Y - spriteHeight + yOffset
                );

                float badgeScale = (size * 0.8f) / badgeTex.Width;
                Vector2 badgeOrigin = new Vector2(badgeTex.Width / 2f, badgeTex.Height / 2f);

                float badgeDepth = Math.Max(0f, depth + RenderUtility.Eps);

                spriteBatch.Draw(
                    texture: badgeTex,
                    position: topRightPos,
                    sourceRectangle: null,
                    color: Color.White,
                    rotation: 0f,
                    origin: badgeOrigin,
                    scale: badgeScale,
                    effects: SpriteEffects.None,
                    layerDepth: badgeDepth
                );
            }
        }
    }

    private void ComputeColor()
    {
        ComponentRegistry registry = GamelabGame.Instance.ComponentRegistry;
        List<float> colorComponents = ComponentIds
            .Select(id => registry.Get(id).Color) // Ask the registry for the config, then get Color
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
        List<IBulletEffect> tmp = effects;
        effects = new();
        effects.AddRange(tmp.Where(e => e.IsBasic));
        effects.AddRange(tmp.Where(e => !e.IsBasic));
    }
}