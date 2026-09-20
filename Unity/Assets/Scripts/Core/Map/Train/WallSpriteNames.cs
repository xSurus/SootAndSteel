namespace Gamelab.Map.Train
{
    // Texture choice from Src ShootHoleWall.Draw. Names have no folder prefix.
    public static class WallSpriteNames
    {
        public static string Select(bool isTop, float damagePercent, bool isBreached, int variation)
        {
            if (isTop)
            {
                if (isBreached) return "WallTileTopBroken4_Variation" + variation;
                if (damagePercent >= 75f) return "WallTileTopBroken3_Variation" + variation;
                if (damagePercent >= 50f) return "WallTileTopBroken2_Variation" + variation;
                if (damagePercent >= 25f) return "WallTileTopBroken1_Variation" + variation;
                return "WallTileTop";
            }
            if (isBreached) return "WallTileBottomBroken3";
            if (damagePercent >= 66f) return "WallTileBottomBroken2";
            if (damagePercent >= 33f) return "WallTileBottomBroken1";
            return "WallTileBottom";
        }
    }
}
