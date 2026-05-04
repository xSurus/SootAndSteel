using System;
using System.Collections.Generic;
using System.Linq;
using Gamelab.Assets;
using Gamelab.Enemies.Core;
using Gamelab.Particles.Modifiers;
using Gamelab.Particles.Profiles;
using Gamelab.Players;
using Gamelab.Services.Vfx;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Particles;

public class FootprintSystem
{
    private readonly ParticleEmitter snowEmitter;
    private readonly ParticleEmitter woodEmitter;

    private readonly float[] stepTimers;
    private readonly bool[] stepSides;
    private readonly Dictionary<AbstractEnemy, (float timer, bool side)> horseState = new();

    private const float StepInterval = 0.2f;
    private const float StepOffset = 6f;
    private const float FootprintAge = 4f;
    private const float FootprintSize = 2f;

    public FootprintSystem(IVfxService vfxService, int playerCount)
    {
        stepTimers = new float[playerCount];
        stepSides = new bool[playerCount];

        snowEmitter = CreateEmitter(AssetManager.FootprintSnowTexture, Color.White * 0.5f);
        woodEmitter = CreateEmitter(AssetManager.FootprintTrainTexture, Color.White * 0.35f);

        vfxService.AddContinuous(snowEmitter);
        vfxService.AddContinuous(woodEmitter);
    }

    private static ParticleEmitter CreateEmitter(Texture2D texture, Color color)
    {
        var emitter = new ParticleEmitter(256, texture)
        {
            AutoTrigger = false,
            Layer = RenderUtility.FloorLayer + 0.01f,
            Profile = new CircleProfile(radius: 0f, onlyRing: false, radiateOutward: false),
            Parameters = new ParticleReleaseParameters
            {
                MinQuantity = 1, MaxQuantity = 1,
                MinSpeed = 0f, MaxSpeed = 0f,
                MinAge = FootprintAge, MaxAge = FootprintAge,
                MinSize = FootprintSize, MaxSize = FootprintSize,
                Color = color
            }
        };
        emitter.Modifiers.Add(new FadeOutModifier(FootprintAge));
        return emitter;
    }

    public void Update(float dt, IReadOnlyList<Player> players, Func<Vector2, bool> isOnTrain)
    {
        for (int i = 0; i < players.Count; i++)
        {
            Vector2 vel = players[i].PhysicsBody.LinearVelocity;
            if (vel.LengthSquared() < 0.05f)
            {
                stepTimers[i] = 0f;
                continue;
            }

            stepTimers[i] -= dt;
            if (stepTimers[i] <= 0f)
            {
                stepSides[i] = !stepSides[i];
                StampFootprint(players[i].Position, vel, stepSides[i], isOnTrain(players[i].Position) ? woodEmitter : snowEmitter);
                stepTimers[i] = StepInterval;
            }
        }
    }

    public void UpdateHorses(float dt, IReadOnlyList<AbstractEnemy> horses)
    {
        var toRemove = new List<AbstractEnemy>();
        foreach (var key in horseState.Keys)
            if (!horses.Contains(key)) toRemove.Add(key);
        foreach (var key in toRemove)
            horseState.Remove(key);

        foreach (var horse in horses)
        {
            if (!horseState.TryGetValue(horse, out var state))
                state = (0f, false);

            Vector2 vel = horse.PhysicsBody.LinearVelocity;
            if (vel.LengthSquared() < 0.05f)
            {
                horseState[horse] = (0f, state.side);
                continue;
            }

            state.timer -= dt;
            if (state.timer <= 0f)
            {
                state.side = !state.side;
                StampFootprint(horse.Position, vel, state.side, snowEmitter);
                state.timer = StepInterval;
            }

            horseState[horse] = state;
        }
    }

    private static void StampFootprint(Vector2 entityPosition, Vector2 velocity, bool side, ParticleEmitter emitter)
    {
        Vector2 dir = Vector2.Normalize(velocity);
        Vector2 perp = new(-dir.Y, dir.X);
        emitter.Position = entityPosition + perp * (side ? StepOffset : -StepOffset);
        emitter.Parameters.Rotation = MathF.Atan2(dir.Y, dir.X) + MathF.PI / 2f;
        emitter.Emit();
    }
}
