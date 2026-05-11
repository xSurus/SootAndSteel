using System;
using System.Linq;
using Gamelab.Dialogue;
using Gamelab.Enemies;
using Gamelab.Map.Train;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Stations;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.PhysicalEntities.Stations.Resources;
using Gamelab.PhysicalEntities.Structures;
using Gamelab.Screens;
using Gamelab.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Gamelab.Tutorial;

public class TutorialDirector : ITutorialDirector
{
    private const float TutorialStartTemperatureFraction = 0.7f;
    private const float TutorialTemperatureDecreaseScale = 0.4f;

    private TrainMap trainMap;

    private TrainNose trainNose;

    private CoalResourceStation coalStation;
    private AbstractStation workbench;
    private BulletRack bulletRack;
    private AbstractStation cannon;
    private ShootHoleWall pendingWallToBreach;

    private bool stepCoalDone;
    private bool stepBulletDone;
    private bool stepWallRepairVisible;
    private bool stepWallDone;
    private bool wallEverBreached;
    private bool showFreezeWarning;
    private float time;
    private float skipHoldTimer;
    private EnemyManager enemiesRef;

    private string lastGuidanceSignature;
    private GameplayContext guidanceContext;

    private bool pendingHubTransition;
    private float celebrationTimer;
    private bool pendingHubOutroRequest;

    private const float TutorialAmbushStartDistance = TutorialLevelProvider.AmbushDistance;

    private const float CelebrationHoldSeconds = 8f;
    private const float SkipHoldSeconds = 1.5f;

    public void Initialize(TrainMap map, GameplayContext context)
    {
        trainMap = map;

        trainNose = trainMap.MapObjects.OfType<TrainNose>().FirstOrDefault();
        trainNose?.SetFuelLevel(0f);

        coalStation = trainMap.MapObjects.OfType<CoalResourceStation>().FirstOrDefault();
        workbench = trainMap.MapObjects.OfType<Workbench>().FirstOrDefault();
        bulletRack = trainMap.MapObjects.OfType<BulletRack>().FirstOrDefault();
        cannon = trainMap.MapObjects.OfType<CannonStation>().FirstOrDefault();

        pendingWallToBreach = trainMap.MapObjects.OfType<ShootHoleWall>().FirstOrDefault();

        ApplyColdStart(context);
    }

    private static void ApplyColdStart(GameplayContext context)
    {
        TrainState s = context.State;
        s.CurrentSpeed = TrainSpeedSetting.Stopped;
        s.actualSpeed = 0f;
        float maxT = GamelabGame.Instance.GameplayConfig.TrainMaxTemperature;
        s.Temperature = maxT * TutorialStartTemperatureFraction;
        s.TemperatureDecreaseScale = TutorialTemperatureDecreaseScale;
    }

    public void Update(float dt, GameplayContext ctx, EnemyManager enemies)
    {
        guidanceContext = ctx;
        enemiesRef = enemies;
        time += dt;

        if (pendingWallToBreach != null)
        {
            pendingWallToBreach.TakeDamage(pendingWallToBreach.MaxHealth);
            pendingWallToBreach = null;
        }

        if (pendingHubTransition)
        {
            celebrationTimer -= dt;
            SyncDialogueGuidance(ctx);
            if (celebrationTimer <= 0f)
            {
                pendingHubTransition = false;
                pendingHubOutroRequest = true;
            }
            return;
        }

        if (!stepCoalDone && ctx.State.IsCoalOvenBurning)
            stepCoalDone = true;

        if (!stepBulletDone && bulletRack != null && bulletRack.PeekNextItem() != null)
            stepBulletDone = true;

        if (!wallEverBreached && ctx.State.numberBreachedWalls > 0)
        {
            wallEverBreached = true;
            stepWallRepairVisible = true;
        }

        if (stepWallRepairVisible && !stepWallDone && ctx.State.numberBreachedWalls == 0 && wallEverBreached)
            stepWallDone = true;

        if (stepCoalDone && stepWallDone && ctx.State.CurrentSpeed == TrainSpeedSetting.Stopped)
            ctx.State.CurrentSpeed = TrainSpeedSetting.Slow;

        if (showFreezeWarning && ctx.State.IsCoalOvenBurning)
            showFreezeWarning = false;

        SyncDialogueGuidance(ctx);
    }

    private void SyncDialogueGuidance(GameplayContext ctx)
    {
        if (GamelabGame.Instance.Services.GetService<IDialogueService>() is not DialogueManager dialogue)
            return;

        bool coalEmptyAfterFirstFuel = stepCoalDone && !ctx.State.IsCoalOvenBurning;

        DialogueLine line;
        string signature;
        if (pendingHubTransition)
        {
            signature = "graduation";
            line = new DialogueLine("Clear track",
                "Good job. You made it. You're ready for the rest of the adventure.");
        }
        else if (showFreezeWarning)
        {
            signature = "freeze";
            line = new DialogueLine("Cold snap",
                "The cabin's freezing solid! Pick up coal [A] from the tender and drop it in the firebox. Don't let the temperature hit zero.");
        }
        else if (coalEmptyAfterFirstFuel)
        {
            signature = "coal_empty";
            line = new DialogueLine("Firebox out",
                "The oven's gone cold. Pick up coal [A] from the tender and drop it in the firebox.");
        }
        else
        {
            TutorialBeat beat = GetBeat();
            signature = $"beat:{beat}";
            line = BeatToDialogueLine(beat);
        }

        if (signature == lastGuidanceSignature) return;
        lastGuidanceSignature = signature;
        dialogue.SetTutorialGuidance(line);
    }

    public bool ConsumePendingHubOutroRequest()
    {
        if (!pendingHubOutroRequest)
            return false;
        pendingHubOutroRequest = false;
        (GamelabGame.Instance.Services.GetService<IDialogueService>() as DialogueManager)?.ClearTutorialGuidance();
        GamelabGame.Instance.CurrentRun.TutorialCompleted = true;
        SaveManager.SaveRun(GamelabGame.Instance.CurrentRun);
        return true;
    }

    public void OnTrainFrozen()
    {
        if (!showFreezeWarning)
            showFreezeWarning = true;
    }

    public void OnAllPlayersKnockedOut()
    {
        GamelabGame.Instance.SwitchToScreen(new GameplayScreen(GamelabGame.Instance, new TutorialDirector()));
    }

    public void OnLevelCompleted()
    {
        if (pendingHubTransition)
            return;
        pendingHubTransition = true;
        celebrationTimer = CelebrationHoldSeconds;
        lastGuidanceSignature = "";
    }

    public void DrawWorld(SpriteBatch spriteBatch)
    {
        TutorialBeat beat = GetBeat();
        if (!TryGetHighlightForBeat(beat, out Vector2? pos)) return;
        DrawHighlight(spriteBatch, pos);
    }

    private bool TryGetHighlightForBeat(TutorialBeat beat, out Vector2? position)
    {
        position = beat switch
        {
            TutorialBeat.Coal => trainNose?.Position ?? coalStation?.Position,
            TutorialBeat.Bullet => workbench?.Position,
            TutorialBeat.Wall => trainMap.MapObjects.OfType<ShootHoleWall>().FirstOrDefault(w => w.IsBroken)?.Position,
            TutorialBeat.Shoot => cannon?.Position,
            _ => null
        };
        return position != null;
    }

    private TutorialBeat GetBeat()
    {
        if (stepWallRepairVisible && !stepWallDone)
            return TutorialBeat.Wall;

        if (!stepCoalDone)
            return TutorialBeat.Coal;

        bool hasThreats = enemiesRef != null && enemiesRef.HasActiveThreats;
        float distance = guidanceContext?.State.DistanceTraveled ?? 0f;

        if (!hasThreats)
        {
            if (distance < TutorialAmbushStartDistance)
                return TutorialBeat.Journey;
            return TutorialBeat.RideClear;
        }

        if (!stepBulletDone)
            return TutorialBeat.Bullet;
        return TutorialBeat.Shoot;
    }

    private static DialogueLine BeatToDialogueLine(TutorialBeat beat) => beat switch
    {
        TutorialBeat.Coal => new DialogueLine("Cold boiler",
            "The oven is cold and empty. Pick up coal [A] from the tender, carry it to the firebox, and drop it in [A] to shovel."),
        TutorialBeat.Journey => new DialogueLine("Rolling",
            "Good. Keep moving. You can change the speed of the train by interacting with the speed lever [X]."),
        TutorialBeat.Wall => new DialogueLine("Breached hull",
            "Cold's pouring through the hole. Grab anything in your way [Y] and drag it clear, then hold interact [X] on the wall to weld it shut."),
        TutorialBeat.Bullet => new DialogueLine("Load out",
            "Head to the workbench. Pick up [A] a projectile, a casing, and a propellant and place them on the bench. Hold interact [X] to craft. Then carry the round and drop it [A] on the rack beside the cannon."),
        TutorialBeat.Shoot => new DialogueLine("Return fire",
            "Pick up [A] a bullet from the rack and load it into the cannon. Aim with the stick and press interact [X] to fire. Clear that rifleman."),
        TutorialBeat.RideClear => new DialogueLine("Ambush broken",
            "That's the last of them. Hold your speed until this stretch of line is behind you."),
        _ => throw new ArgumentOutOfRangeException(nameof(beat), beat, "Unexpected tutorial beat."),
    };

    private void DrawHighlight(SpriteBatch spriteBatch, Vector2? position)
    {
        if (position == null) return;
        int tileSize = GamelabGame.Instance.GameplayConfig.TrainTileSize;
        float pulse = 0.6f + 0.4f * MathF.Sin(time * 4f);
        var color = Color.Yellow * pulse;
        var rect = new RectangleF(
            position.Value.X - tileSize / 2f,
            position.Value.Y - tileSize / 2f,
            tileSize,
            tileSize);
        spriteBatch.DrawRectangle(rect, color, 3f);
    }

    private enum TutorialBeat
    {
        Coal,
        Journey,
        Bullet,
        Wall,
        Shoot,
        RideClear
    }
}
