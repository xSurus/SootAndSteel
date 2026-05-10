using System;
using System.Collections.Generic;
using Gamelab.Enemies.Types;
using Gamelab.Items.Bullets;
using Gamelab.Map;
using Gamelab.Map.Train.State;
using Gamelab.PhysicalEntities.Stations.Cannon;
using Gamelab.Screens.Camera;
using Gamelab.Services.Bullet;
using Gamelab.Services.Vfx;
using Gamelab.UI;
using Gamelab.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Gamelab.Screens;

public class ShootingRangeScreen(GamelabGame game) : GamelabGameScreen(game)
{
    private Logger logger = new Logger("ShootingRangeScreen");
    private ShootingRangePanel panel;
    private GameplayContext gameplayContext;
    private CameraDirector cameraDirector;
    private OrthographicCamera camera;
    private WorldScroller worldScroller;

    private CannonStation cannon;

    private List<DummyEnemy> enemyInstances = [];
    private float enemySpawnCooldown;
    
    private float accumulator;

    private float shootingInterval = 2.0f;
    private float timeSinceLastShot;
    
    public override void LoadContent()
    {
        base.LoadContent();
        
        logger.Info(virtualScreenSize.X + "x" + virtualScreenSize.Y);
        gameplayContext = new GameplayContext(virtualScreenSize, Game.CurrentRun);
        Services.AddService(gameplayContext);
        
        InitializeCamera();

        worldScroller = new WorldScroller();
        
        cannon = new CannonStation(new Vector2(640, 540));
        
        panel = new ShootingRangePanel();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (panel.NumOfEnemies > enemyInstances.Count && enemySpawnCooldown <= 0f)
        {
            SpawnDummyEnemy();
            PositionEnemies();
        }

        if (panel.NumOfEnemies <= enemyInstances.Count)
        {
            enemySpawnCooldown = 1f;
        }

        if (enemySpawnCooldown > 0f)
        {
            enemySpawnCooldown -= dt;
        }
        
        timeSinceLastShot += dt;
        if (timeSinceLastShot > shootingInterval)
        {
            Fire();
            timeSinceLastShot -= shootingInterval;
        }

        UpdatePhysics(dt);

        foreach (var enemy in enemyInstances)
        {
            if (enemy.ShouldRemove) enemy.RemovePhysicsBody();
        }
        enemyInstances = enemyInstances.FindAll(enemy => !enemy.ShouldRemove);
    }

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        spriteBatch.Begin(
            sortMode: SpriteSortMode.FrontToBack,
            blendState: BlendState.AlphaBlend,
            transformMatrix: camera.GetViewMatrix()
        );

        foreach (var enemy in enemyInstances)
        {
            enemy.Draw(spriteBatch);
        }
        
        worldScroller.Draw(spriteBatch);
        cannon.Draw(spriteBatch);
        Services.GetService<IBulletService>().Render(spriteBatch);
        Services.GetService<IVfxService>().Render(spriteBatch);
        
        spriteBatch.End();
        base.Draw(gameTime);
    }
    
    private void InitializeCamera()
    {
        camera = new OrthographicCamera(viewportAdapter);
        cameraDirector = new CameraDirector(virtualScreenSize);
        cameraDirector.SnapToCenter(camera, new Rectangle(0, 0, 1920, 1080), virtualScreenSize.X, virtualScreenSize.Y,
            allowOffWorldOverflow: true, maxZoom: Game.GameplayConfig.CameraMaxZoom);
    }

    private void Fire()
    {
        if (cannon != null)
        {
            cannon.HeldItem = new BulletItem(
                ComponentIds.BasicCasing,
                ComponentIds.BasicProjectile,
                ComponentIds.BasicPropellant,
                ComponentIds.MatryoshkaProjectile,
                ComponentIds.ScatterCasing,
                ComponentIds.HeavyPropellant,
                ComponentIds.HomingPropellant
            );
            cannon.FireCannon();
        }
    }

    private void SpawnDummyEnemy()
    {
        var enemy = new DummyEnemy(new Vector2(10, 5));
        enemyInstances.Add(enemy);
        enemySpawnCooldown = 1f;
        logger.Info("Spawned enemy");
    }

    private void PositionEnemies()
    {
        Vector2 circleCenter = new Vector2(2.0f * virtualScreenSize.X / 3.0f, virtualScreenSize.Y / 2.0f);
        float radius = 200.0f;
        double anglePerEnemy = 2.0f * Math.PI / panel.NumOfEnemies;

        int i = 0;
        foreach (var enemy in enemyInstances)
        {
            enemy.Position = circleCenter + radius * new Vector2(
                (float)Math.Cos(i * anglePerEnemy),
                (float)Math.Sin(i * anglePerEnemy));
            i++;
        }
    }

    private void UpdatePhysics(float dt)
    {
        accumulator += Math.Min(dt, Game.GameplayConfig.MaxAccumulatedDeltaSeconds);
        float fixedDt = Game.GameplayConfig.FixedTimeStep;
        
        while (accumulator >= fixedDt)
        {
            cannon?.Update(fixedDt);
            gameplayContext.PhysicsWorld.Step(fixedDt);
            accumulator -= fixedDt;
        }
    }

    public override void UnloadContent()
    {
        foreach (var enemy in enemyInstances)
        {
            enemy.RemovePhysicsBody();
        }
        enemyInstances.Clear();
        
        Services.RemoveService(typeof(GameplayContext));
        
        base.UnloadContent();
    }
}