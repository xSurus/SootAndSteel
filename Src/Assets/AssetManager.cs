using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Gamelab.Utils.Logging;

namespace Gamelab.Assets;

public static class AssetManager
{
    private static readonly Logger logger = new ("Assets");

    public static Texture2D BlankTexture { get; private set; }
    public static Texture2D PlayerTexture { get; private set; }
    public static Texture2D TitleTexture { get; private set; }
    public static Texture2D[] TileTexture { get; private set; }
    public static Texture2D[] TrainTrackTexture { get; private set; }
    public static Texture2D EnemyTexture { get; private set; }
    public static Texture2D SmokeTexture { get; private set; }
    public static Texture2D SparkTexture { get; private set; }
    public static Texture2D HubTexture { get; private set; }
    public static System.Collections.Generic.Dictionary<string, Texture2D> StationTextures { get; private set; }
    = new System.Collections.Generic.Dictionary<string, Texture2D>();
    public static System.Collections.Generic.Dictionary<string, Texture2D> WallTextures { get; private set; }
    = new System.Collections.Generic.Dictionary<string, Texture2D>();
    public static System.Collections.Generic.Dictionary<string, Texture2D> CharacterTextures { get; private set; }
    = new System.Collections.Generic.Dictionary<string, Texture2D>();
    public static System.Collections.Generic.Dictionary<string, Texture2D> StructureTextures { get; private set; }
    = new System.Collections.Generic.Dictionary<string, Texture2D>();
    public static System.Collections.Generic.Dictionary<string, Texture2D> NPCTextures { get; private set; }
    = new System.Collections.Generic.Dictionary<string, Texture2D>();
    public static System.Collections.Generic.Dictionary<string, Texture2D> EnemyTextures { get; private set; }
    = new System.Collections.Generic.Dictionary<string, Texture2D>();

    public static void LoadContent(GraphicsDevice graphicsDevice)
    {
        // temporary textures used for player and stations before real assets are used

        // single white pixel that can be differently colored and scaled later for stations etc for testing
        BlankTexture = new Texture2D(graphicsDevice, 1, 1);
        BlankTexture.SetData([Color.White]);

        LoadPlayerTexture(graphicsDevice);
        LoadTitleTexture(graphicsDevice);
        LoadTileTexture(graphicsDevice);
        LoadTrainTrackTexture(graphicsDevice);
        LoadWallTextures(graphicsDevice);
        LoadParticleTextures(graphicsDevice);
        LoadStructureTextures(graphicsDevice);
        LoadStationTextures(graphicsDevice);
        LoadHubTexture(graphicsDevice);
        LoadNPCTextures(graphicsDevice);
        LoadEnemyTextures(graphicsDevice);
        // TODO add texture loading from json
    }

    private static void LoadPlayerTexture(GraphicsDevice graphicsDevice)
    {   

        string dir = System.IO.Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "Assets", "Characters");

        if (!System.IO.Directory.Exists(dir))
        {
            System.Console.WriteLine($"[AssetManager] Characters folder not found: {dir}");
            return;
        }

        foreach (string path in System.IO.Directory.GetFiles(dir, "*.png"))
        {
            string key = System.IO.Path.GetFileNameWithoutExtension(path);
            using System.IO.Stream stream = System.IO.File.OpenRead(path);
            CharacterTextures[key] = Texture2D.FromStream(graphicsDevice, stream);
            logger.Info($"Loading character texture for '{key}'");
        }

        // player texture
        int textureSize = 128;
        PlayerTexture = new Texture2D(graphicsDevice, textureSize, textureSize);
        Color[] data = new Color[textureSize * textureSize];
        Vector2 center = new Vector2(textureSize / 2f);
        float radius = textureSize / 2f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                data[y * textureSize + x] = distance <= radius ? Color.White : Color.Transparent;
            }
        }

        PlayerTexture.SetData(data);

        // enemy texture (square with darker center)
        int enemyTextureSize = 64;
        EnemyTexture = new Texture2D(graphicsDevice, enemyTextureSize, enemyTextureSize);
        Color[] enemyData = new Color[enemyTextureSize * enemyTextureSize];

        for (int y = 0; y < enemyTextureSize; y++)
        {
            for (int x = 0; x < enemyTextureSize; x++)
            {
                int border = 4;
                bool isBorder = x < border || x >= enemyTextureSize - border ||
                                y < border || y >= enemyTextureSize - border;
                enemyData[y * enemyTextureSize + x] = isBorder ? Color.DarkRed : Color.White;
            }
        }

        EnemyTexture.SetData(enemyData);
    }

    public static Texture2D GetPlayerTexture(string type)
    {
        return CharacterTextures.TryGetValue(type, out var tex) ? tex : BlankTexture;
    }

    private static Texture2D LoadTexture(GraphicsDevice gd, string name)
    {
        string path = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Assets", name);
        
        if (!System.IO.File.Exists(path))
        {
            Texture2D errorTex = new Texture2D(gd, 1, 1);
            errorTex.SetData(new Color[] { Color.HotPink });
            return errorTex;
        }

        using (System.IO.Stream stream = System.IO.File.OpenRead(path))
        {
            return Texture2D.FromStream(gd, stream);
        }
    }

    private static void LoadStationTextures(GraphicsDevice graphicsDevice)
    {
        string dir = System.IO.Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "Assets", "Stations");

        if (!System.IO.Directory.Exists(dir))
        {
            System.Console.WriteLine($"[AssetManager] Stations folder not found: {dir}");
            return;
        }

        foreach (string path in System.IO.Directory.GetFiles(dir, "*.png"))
        {
            string key = System.IO.Path.GetFileNameWithoutExtension(path);
            using System.IO.Stream stream = System.IO.File.OpenRead(path);
            StationTextures[key] = Texture2D.FromStream(graphicsDevice, stream);
            logger.Info($"Loading station texture for '{key}'");
        }
    }

    private static void LoadEnemyTextures(GraphicsDevice graphicsDevice)
    {
        string dir = System.IO.Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "Assets", "Enemies");

        if (!System.IO.Directory.Exists(dir))
        {
            System.Console.WriteLine($"[AssetManager] Enemies folder not found: {dir}");
            return;
        }

        foreach (string path in System.IO.Directory.GetFiles(dir, "*.png"))
        {
            string key = System.IO.Path.GetFileNameWithoutExtension(path);
            using System.IO.Stream stream = System.IO.File.OpenRead(path);
            EnemyTextures[key] = Texture2D.FromStream(graphicsDevice, stream);
        }
    }

    public static Texture2D GetEnemyTexture(string type)
    {
        return EnemyTextures.TryGetValue(type, out var tex) ? tex : BlankTexture;
    }

    private static void LoadStructureTextures(GraphicsDevice graphicsDevice)
    {
        string dir = System.IO.Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "Assets", "Structures");

        if (!System.IO.Directory.Exists(dir))
        {
            System.Console.WriteLine($"[AssetManager] Structures folder not found: {dir}");
            return;
        }

        foreach (string path in System.IO.Directory.GetFiles(dir, "*.png"))
        {
            string key = System.IO.Path.GetFileNameWithoutExtension(path);
            using System.IO.Stream stream = System.IO.File.OpenRead(path);
            StructureTextures[key] = Texture2D.FromStream(graphicsDevice, stream);
            logger.Info($"Loading structure texture for '{key}'");
        }
    }

    public static Texture2D GetStructureTexture(string type)
    {
        return StructureTextures.TryGetValue(type, out var tex) ? tex : BlankTexture;
    }

    private static void LoadNPCTextures(GraphicsDevice graphicsDevice)
    {
        string dir = System.IO.Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "Assets", "NPCs");

        if (!System.IO.Directory.Exists(dir))
        {
            System.Console.WriteLine($"[AssetManager] NPCs folder not found: {dir}");
            return;
        }

        foreach (string path in System.IO.Directory.GetFiles(dir, "*.png"))
        {
            string key = System.IO.Path.GetFileNameWithoutExtension(path);
            using System.IO.Stream stream = System.IO.File.OpenRead(path);
            NPCTextures[key] = Texture2D.FromStream(graphicsDevice, stream);
            logger.Info($"Loading NPC texture for '{key}'");
        }
    }

    public static Texture2D GetNPCTexture(string type)
    {
        return NPCTextures.TryGetValue(type, out var tex) ? tex : BlankTexture;
    }


    public static Texture2D GetStationTexture(string type)
    {
        return StationTextures.TryGetValue(type, out var tex) ? tex : BlankTexture;
    }

    private static void LoadTrainTrackTexture(GraphicsDevice graphicsDevice)
    {
        TrainTrackTexture = new Texture2D[2];
        TrainTrackTexture[0] = LoadTexture(graphicsDevice, "Rail_Tile_01.png");
        TrainTrackTexture[1] = LoadTexture(graphicsDevice, "Rail_Tile_02.png");
    }

    private static void LoadHubTexture(GraphicsDevice graphicsDevice)
    {
        HubTexture = LoadTexture(graphicsDevice, "Hub.png");
    }

    private static void LoadTitleTexture(GraphicsDevice graphicsDevice)
    {
        TitleTexture = LoadTexture(graphicsDevice, "Title.png");
    }

    private static void LoadTileTexture(GraphicsDevice graphicsDevice)
    {
        TileTexture = new Texture2D[2];
        TileTexture[0] = LoadTexture(graphicsDevice, "Train_Tile_A.png");
        TileTexture[1] = LoadTexture(graphicsDevice, "Train_Tile_B.png");
    }

    private static void LoadWallTextures(GraphicsDevice graphicsDevice)
    {
        string dir = System.IO.Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "Assets", "Walls");

        if (!System.IO.Directory.Exists(dir))
        {
            System.Console.WriteLine($"[AssetManager] Stations folder not found: {dir}");
            return;
        }

        foreach (string path in System.IO.Directory.GetFiles(dir, "*.png"))
        {
            string key = System.IO.Path.GetFileNameWithoutExtension(path);
            using System.IO.Stream stream = System.IO.File.OpenRead(path);
            WallTextures[key] = Texture2D.FromStream(graphicsDevice, stream);
            logger.Info($"Loading wall texture for '{key}'");
        }
    }

    public static Texture2D GetWallTexture(string type)
    {
        return WallTextures.TryGetValue(type, out var tex) ? tex : BlankTexture;
    }

    private static void LoadParticleTextures(GraphicsDevice graphicsDevice)
    {
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

    public static void UnloadContent()
    {
        BlankTexture?.Dispose();
        BlankTexture = null;

        PlayerTexture?.Dispose();
        PlayerTexture = null;

        TitleTexture?.Dispose();
        TitleTexture = null;

        TileTexture[0]?.Dispose();
        TileTexture[0] = null;
        
        TileTexture[1]?.Dispose();
        TileTexture[1] = null;

        TrainTrackTexture[0]?.Dispose();
        TrainTrackTexture[0] = null;
        
        TrainTrackTexture[1]?.Dispose();
        TrainTrackTexture[1] = null;

        EnemyTexture?.Dispose();
        EnemyTexture = null;

        SparkTexture?.Dispose();
        SparkTexture = null;

        SmokeTexture?.Dispose();
        SmokeTexture = null;

        HubTexture?.Dispose();
        HubTexture = null;

        foreach (var tex in StationTextures.Values)
            tex?.Dispose();
        StationTextures.Clear();

        foreach (var tex in WallTextures.Values)
            tex?.Dispose();
        WallTextures.Clear();

        foreach (var tex in CharacterTextures.Values)
            tex?.Dispose();
        CharacterTextures.Clear();

        foreach (var tex in StructureTextures.Values)
            tex?.Dispose();
        StructureTextures.Clear();

        foreach (var tex in NPCTextures.Values)
            tex?.Dispose();
        NPCTextures.Clear();

        foreach (var tex in EnemyTextures.Values)
            tex?.Dispose();
        EnemyTextures.Clear();
    }
}