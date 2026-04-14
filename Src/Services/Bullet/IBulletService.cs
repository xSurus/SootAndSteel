using Gamelab.Items.Bullets;
using Gamelab.PhysicalEntities.Bullets.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gamelab.Services.Bullet;

public interface IBulletService
{
    public void Render(SpriteBatch spriteBatch);
    public void EmitBullet(BulletItem bullet, Vector2 position, Vector2 direction, IBulletEmitter cannonStation);

    public void EmitAdditionalBullet(BulletItem bullet, Vector2 position, Vector2 direction,
        IBulletEmitter cannonStation);
}