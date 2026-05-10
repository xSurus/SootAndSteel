using System;
using System.Collections.Generic;
using System.IO;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;

namespace Gamelab.Assets;

public static class AssetManager
{
    private static readonly Logger logger = new("Assets");

    public static Texture2D BlankTexture { get; private set; }
    public static Texture2D SmokeTexture { get; private set; }
    public static Texture2D SparkTexture { get; private set; }

    public static Texture2D[] TileTexture { get; private set; } = new Texture2D[2];
    public static Texture2D[] TrainTrackTexture { get; private set; } = new Texture2D[2];
    public static Texture2D HubTexture { get; private set; }
    public static Texture2D TitleTexture { get; private set; }
    public static Texture2D SnowPatchTexture { get; private set; }
    public static Texture2D IcePatchTexture { get; private set; }

    public static Dictionary<string, Texture2D> CharacterTextures { get; private set; } = new();
    public static Dictionary<string, Texture2D> StationTextures { get; private set; } = new();
    public static Dictionary<string, Texture2D> HubDecorationTextures { get; private set; } = new();
    public static Dictionary<string, Texture2D> DecorationTextures { get; private set; } = new();
    public static Dictionary<string, Texture2D> WallTextures { get; private set; } = new();
    public static Dictionary<string, Texture2D> StructureTextures { get; private set; } = new();
    public static Dictionary<string, Texture2D> NPCTextures { get; private set; } = new();
    public static Dictionary<string, Texture2D> ItemTextures { get; private set; } = new();
    public static SpriteSheet EnemySpriteSheet { get; private set; }
    public static SpriteSheet PlayerSpriteSheet { get; private set; }
    public static SpriteSheet ConveyorSpriteSheet { get; private set; }
    public static Texture2D[] PlayerCannonTopTextures { get; private set; } = new Texture2D[4];
    public static Texture2D[] PlayerCannonBottomTextures { get; private set; } = new Texture2D[4];

    public static void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        GenerateProceduralTextures(graphicsDevice);

        TileTexture[0] = content.Load<Texture2D>("Train_Tile_A");
        TileTexture[1] = content.Load<Texture2D>("Train_Tile_B");
        TrainTrackTexture[0] = content.Load<Texture2D>("Rail_Tile_01");
        TrainTrackTexture[1] = content.Load<Texture2D>("Rail_Tile_02");
        HubTexture = content.Load<Texture2D>("Hub");
        TitleTexture = content.Load<Texture2D>("Title");
        SnowPatchTexture = content.Load<Texture2D>("Snow_Tile");
        IcePatchTexture = content.Load<Texture2D>("Ice_Tile");

        LoadDictionary(content, CharacterTextures, "Characters");
        LoadDictionary(content, StationTextures, "Stations");
        LoadDictionary(content, HubDecorationTextures, "Hub");
        LoadDictionary(content, DecorationTextures, "Decorations");
        LoadDictionary(content, WallTextures, "Walls");
        LoadDictionary(content, StructureTextures, "Structures");
        LoadDictionary(content, NPCTextures, "NPCs");
        LoadDictionary(content, ItemTextures, "Items");
        LoadEnemyAnimations(content);
        LoadPlayerAnimations(content);
        LoadPlayerCannonTextures(content);
        LoadConveyorAnimations(content);
    }

    private static void LoadDictionary(ContentManager content, Dictionary<string, Texture2D> dict, string folder)
    {
        string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, content.RootDirectory, folder);

        if (!Directory.Exists(directoryPath))
        {
            logger.Info($"[AssetManager] Folder not found: {directoryPath}");
            return;
        }

        string[] files = Directory.GetFiles(directoryPath, "*.xnb");

        foreach (string filePath in files)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);

            try
            {
                dict[name] = content.Load<Texture2D>($"{folder}/{name}");
                logger.Info($"Loaded {folder} texture: '{name}'");
            }
            catch (InvalidCastException)
            {
                logger.Info($"Skipped {folder} file: '{name}' (It is not a standard Texture2D).");
            }
            catch (ContentLoadException)
            {
                logger.Info($"Failed to load {folder} texture: '{name}'. Using BlankTexture.");
                dict[name] = BlankTexture;
            }
        }
    }

    private static void GenerateProceduralTextures(GraphicsDevice graphicsDevice)
    {
        BlankTexture = new Texture2D(graphicsDevice, 1, 1);
        BlankTexture.SetData([Color.White]);

        int smokeSize = 64;
        SmokeTexture = new Texture2D(graphicsDevice, smokeSize, smokeSize);
        Color[] smokeData = new Color[smokeSize * smokeSize];
        Vector2 smokeCenter = new Vector2(smokeSize / 2f);
        float smokeRadius = smokeSize / 2f;
        for (int y = 0; y < smokeSize; y++)
        {
            for (int x = 0; x < smokeSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), smokeCenter);
                float alpha = Math.Clamp(1f - (distance / smokeRadius), 0f, 1f);
                smokeData[y * smokeSize + x] = new Color(Color.White, alpha);
            }
        }

        SmokeTexture.SetData(smokeData);

        int sparkSize = 8;
        SparkTexture = new Texture2D(graphicsDevice, sparkSize, sparkSize);
        Color[] sparkData = new Color[sparkSize * sparkSize];
        Vector2 sparkCenter = new Vector2(sparkSize / 2f);
        float sparkRadius = sparkSize / 2f;
        for (int y = 0; y < sparkSize; y++)
        {
            for (int x = 0; x < sparkSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), sparkCenter);
                sparkData[y * sparkSize + x] = distance <= sparkRadius ? Color.White : Color.Transparent;
            }
        }

        SparkTexture.SetData(sparkData);
    }

    public static void LoadEnemyAnimations(ContentManager content)
    {
        Texture2DAtlas enemyAtlas = content.Load<Texture2DAtlas>("Enemies/EnemyAtlas");
        EnemySpriteSheet = new SpriteSheet("EnemySheet", enemyAtlas);

        TimeSpan frameDuration = TimeSpan.FromSeconds(1f / 12f);
        EnemySpriteSheet.DefineAnimation("Run", builder =>
        {
            builder.IsLooping(true)
                .AddFrame("horse01", frameDuration)
                .AddFrame("horse02", frameDuration)
                .AddFrame("horse03", frameDuration)
                .AddFrame("horse04", frameDuration)
                .AddFrame("horse05", frameDuration)
                .AddFrame("horse06", frameDuration)
                .AddFrame("horse07", frameDuration)
                .AddFrame("horse08", frameDuration)
                .AddFrame("horse09", frameDuration)
                .AddFrame("horse010", frameDuration);
        });

        TimeSpan poseDuration = TimeSpan.FromSeconds(1);
        EnemySpriteSheet.DefineAnimation("RifleHeadless", b => b.AddFrame("RifleHeadless", poseDuration));

        EnemySpriteSheet.DefineAnimation("RifleIdle", b => b.AddFrame("RifleIdle", poseDuration));
        EnemySpriteSheet.DefineAnimation("RifleWide", b => b.AddFrame("RifleWide", poseDuration));
        EnemySpriteSheet.DefineAnimation("RifleSemi", b => b.AddFrame("RifleSemi", poseDuration));
        EnemySpriteSheet.DefineAnimation("RifleMiddle", b => b.AddFrame("RifleMiddle", poseDuration));
    }

    public static void LoadPlayerAnimations(ContentManager content)
    {
        Texture2DAtlas totalAtlas = content.Load<Texture2DAtlas>("Players/AllPlayersAtlas");
        PlayerSpriteSheet = new SpriteSheet("PlayerSheet", totalAtlas);
        TimeSpan frameDuration = TimeSpan.FromSeconds(0.2f);
        int totalPlayers = 4;
        int framesPerPlayer = 4;

        for (int p = 0; p < totalPlayers; p++)
        {
            PlayerSpriteSheet.DefineAnimation($"Player{p}_Idle", builder =>
            {
                builder.IsLooping(true);

                for (int f = 1; f <= framesPerPlayer; f++)
                {
                    builder.AddFrame($"Player{p}_Idle{f}", frameDuration);
                }
            });
        }
    }

    private static void LoadPlayerCannonTextures(ContentManager content)
    {
        char[] letters = ['A', 'B', 'C', 'D'];
        for (int i = 0; i < 4; i++)
        {
            PlayerCannonTopTextures[i] = content.Load<Texture2D>($"Players/CannonTop{letters[i]}");
            PlayerCannonBottomTextures[i] = content.Load<Texture2D>($"Players/CannonBottom{letters[i]}");
        }
    }

    public static void LoadConveyorAnimations(ContentManager content)
    {
        Texture2DAtlas conveyorAtlas = content.Load<Texture2DAtlas>("Stations/ConveyorTop");
        ConveyorSpriteSheet = new SpriteSheet("ConveyorSheet", conveyorAtlas);

        TimeSpan frameDuration = TimeSpan.FromSeconds(0.1f);

        ConveyorSpriteSheet.DefineAnimation("Run", builder =>
        {
            builder.IsLooping(true)
                .AddFrame("Conveyor1", frameDuration)
                .AddFrame("Conveyor2", frameDuration)
                .AddFrame("Conveyor3", frameDuration)
                .AddFrame("Conveyor4", frameDuration)
                .AddFrame("Conveyor5", frameDuration)
                .AddFrame("Conveyor6", frameDuration);
        });
    }

    public static Texture2D GetStructureTexture(string type) =>
        StructureTextures.TryGetValue(type, out var tex) ? tex : BlankTexture;

    public static Texture2D GetNPCTexture(string type) =>
        NPCTextures.TryGetValue(type, out var tex) ? tex : BlankTexture;

    public static Texture2D GetItemTexture(string type) => ItemTextures.TryGetValue(type, out var tex) ? tex : null;

    public static Texture2D GetStationTexture(string type) =>
        StationTextures.TryGetValue(type, out var tex) ? tex : BlankTexture;

    public static Texture2D GetHubDecorationTexture(string type) =>
        HubDecorationTextures.TryGetValue(type, out var tex) ? tex : BlankTexture;

    public static Texture2D GetDecorationTexture(string type) =>
        DecorationTextures.TryGetValue(type, out var tex) ? tex : BlankTexture;

    public static Texture2D GetWallTexture(string type) =>
        WallTextures.TryGetValue(type, out var tex) ? tex : BlankTexture;

    public static void UnloadContent()
    {
        BlankTexture?.Dispose();
        BlankTexture = null;

        SparkTexture?.Dispose();
        SparkTexture = null;

        SmokeTexture?.Dispose();
        SmokeTexture = null;

        StationTextures.Clear();
        HubDecorationTextures.Clear();
        DecorationTextures.Clear();
        WallTextures.Clear();
        CharacterTextures.Clear();
        StructureTextures.Clear();
        NPCTextures.Clear();
        ItemTextures.Clear();
    }
}