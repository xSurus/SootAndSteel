using System;
using System.Numerics;

namespace Gamelab.PhysicalEntities.Bullets
{
    public static class BulletTargetingMath
    {
        public static float GetDistanceToLine(Vector2 a, Vector2 b, Vector2 p)
        {
            Vector2 dir = b - a;
            float lengthSquared = dir.LengthSquared();

            if (lengthSquared == 0)
            {
                return Vector2.Distance(p, a);
            }

            float t = Vector2.Dot(p - a, dir) / lengthSquared;
            if (t < 0)
            {
                return float.MaxValue;
            }

            Vector2 projection = a + t * dir;
            return Vector2.Distance(p, projection);
        }
    }
}
