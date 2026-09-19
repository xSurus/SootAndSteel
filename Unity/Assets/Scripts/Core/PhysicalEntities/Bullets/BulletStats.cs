namespace Gamelab.PhysicalEntities.Bullets
{
    public struct BulletStats
    {
        public float Speed { get; set; }
        public float Damage { get; set; }
        public float Pierce { get; set; }
        public float Size { get; set; }
        public float Spread { get; set; }
        public float Lifetime { get; set; }

        public static BulletStats CannonDefault()
        {
            return new BulletStats
            {
                Speed = 800f,
                Damage = 50f,
                Pierce = 1f,
                Size = 12f,
                Spread = 0.2f,
                Lifetime = 5f
            };
        }
    }
}
