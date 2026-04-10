using System;
using System.Collections.Generic;
using Gamelab.Players;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using nkast.Aether.Physics2D.Dynamics;
using nkast.Aether.Physics2D.Dynamics.Contacts;

namespace Gamelab.PhysicalEntities.Triggers;

public sealed class ExitZone : IDisposable
{
    private readonly Body body;
    private readonly HashSet<Player> inside = new();

    public ExitZone(World world, Rectangle boundsPixels)
    {
        Vector2 centerPixels = new Vector2(boundsPixels.Center.X, boundsPixels.Center.Y);
        body = world.CreateRectangle(
            boundsPixels.Width.ToMeters(),
            boundsPixels.Height.ToMeters(),
            1f,
            centerPixels.ToMeters(),
            0f,
            BodyType.Static
        );

        foreach (var fixture in body.FixtureList)
        {
            fixture.IsSensor = true;
            fixture.OnCollision += OnCollision;
            fixture.OnSeparation += OnSeparation;
        }
    }

    private bool OnCollision(Fixture sender, Fixture other, Contact contact)
    {
        if (other.Body.Tag is Player player)
        {
            inside.Add(player);
        }

        return false;
    }

    private void OnSeparation(Fixture sender, Fixture other, Contact contact)
    {
        if (other.Body.Tag is Player player)
        {
            inside.Remove(player);
        }
    }

    public bool HaveAllInside(IEnumerable<Player> players)
    {
        foreach (var player in players)
        {
            if (!inside.Contains(player)) return false;
        }

        return true;
    }

    public void Dispose()
    {
        if (body?.World == null)
        {
            return;
        }

        foreach (var fixture in body.FixtureList)
        {
            fixture.OnCollision -= OnCollision;
            fixture.OnSeparation -= OnSeparation;
        }

        body.World.Remove(body);
    }
}

