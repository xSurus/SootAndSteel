using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets;
using Gamelab.PhysicalEntities.Bullets.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Services.Bullet;

public interface IBulletService
{
    public void Render(SpriteBatch spriteBatch);
    public BulletEntity EmitBullet(BulletItem bullet, Vector2 position, Vector2 direction, IBulletEmitter cannonStation);
    public BulletEntity EmitAdditionalBullet(BulletEntity bullet, IBulletEffect spawningEffect, float delay = 0f);
}