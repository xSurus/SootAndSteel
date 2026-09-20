using System.Globalization;
using Gamelab.Levels;
using NUnit.Framework;

namespace Gamelab.Tests.Levels
{
    /// <summary>
    /// Golden values printed by the real Src ProceduralLevelGenerator (net10.0, default LevelGenerationConfig with
    /// RandomSeed set). Row: seed|level|threatScale|spacingScale|distance|levelSeed|eventCount, then events
    /// "distance,type,ammo,side" (round-trip floats). An empty event string pins only the count.
    /// </summary>
    public class LevelGoldenTests
    {
        private static readonly object[] Cases =
        {
            new object[] { "1|1|1|1|15000|7920|1", "" },
            new object[] { "1|1|1.5|0.6|15000|7920|1", "" },
            new object[] { "1|2|1|1|19000|15839|1", "8311.921,Rifle,Basic,Top" },
            new object[] { "1|2|1.5|0.6|19000|15839|2", "6211.081,Rifle,Basic,Top;8311.921,Rifle,Basic,Top" },
            new object[] { "1|3|1|1|23000|23758|2", "" },
            new object[] { "1|3|1.5|0.6|23000|23758|3", "" },
            new object[] { "1|5|1|1|31000|39596|4", "3199.8975,Rifle,Basic,Top;6293.669,Rifle,Basic,Bottom;11238.501,Rifle,Burst,Bottom;20332.566,Rifle,Basic,Bottom" },
            new object[] { "1|5|1.5|0.6|31000|39596|6", "3199.8975,Rifle,Basic,Top;6293.669,Rifle,Basic,Bottom;11238.501,Rifle,Burst,Bottom;17113.969,Rifle,Basic,Top;20332.566,Rifle,Basic,Bottom;27509.207,Rifle,Scatter,Top" },
            new object[] { "1|9|1|1|47000|71272|6", "" },
            new object[] { "1|9|1.5|0.6|47000|71272|10", "" },
            new object[] { "1|15|1|1|71000|118786|11", "15007.112,Rifle,Scatter,Bottom;19426.29,Rifle,Piercing,Bottom;24400.951,Rifle,Heavy,Bottom;26760.125,Rifle,Frangible,Top;29243.729,Rifle,Frangible,Bottom;29671.475,Rifle,Scatter,Top;31394.295,Rifle,Piercing,Bottom;38154.176,Rifle,Piercing,Bottom;41537.617,Rifle,Heavy,Top;51686.465,Rifle,Frangible,Top;53516.758,Rifle,Scatter,Bottom" },
            new object[] { "1|15|1.5|0.6|71000|118786|16", "2934.5967,Rifle,Basic,Top;3851.0496,Rifle,Basic,Bottom;11159.006,Rifle,Frangible,Top;15007.112,Rifle,Scatter,Bottom;19426.29,Rifle,Piercing,Bottom;24182.146,Rifle,RapidFire,Bottom;24400.951,Rifle,Scatter,Bottom;26760.125,Rifle,Frangible,Top;29243.729,Rifle,Frangible,Bottom;29671.475,Rifle,Scatter,Top;31394.295,Rifle,Piercing,Bottom;34845.64,Rifle,RapidFire,Top;38154.176,Rifle,Piercing,Bottom;41537.617,Rifle,Heavy,Top;51686.465,Rifle,Frangible,Top;53516.758,Rifle,Scatter,Bottom" },
            new object[] { "42|1|1|1|15000|7961|1", "" },
            new object[] { "42|1|1.5|0.6|15000|7961|1", "" },
            new object[] { "42|2|1|1|19000|15880|1", "" },
            new object[] { "42|2|1.5|0.6|19000|15880|2", "" },
            new object[] { "42|3|1|1|23000|23799|2", "" },
            new object[] { "42|3|1.5|0.6|23000|23799|3", "" },
            new object[] { "42|5|1|1|31000|39637|4", "" },
            new object[] { "42|5|1.5|0.6|31000|39637|5", "" },
            new object[] { "42|9|1|1|47000|71313|6", "2891.4165,Rifle,Burst,Bottom;9451.994,Rifle,Basic,Bottom;21034.984,Rifle,Heavy,Bottom;23028.908,Rifle,Homing,Top;36996.41,Rifle,Basic,Bottom;37690.82,Rifle,Scatter,Top" },
            new object[] { "42|9|1.5|0.6|47000|71313|10", "2891.4165,Rifle,Burst,Bottom;8348.639,Rifle,Basic,Top;9451.994,Rifle,Basic,Bottom;21034.984,Rifle,Heavy,Bottom;23028.908,Rifle,Homing,Top;25390.463,Rifle,Basic,Bottom;35599.64,Rifle,Piercing,Top;35972.156,Rifle,Basic,Bottom;36996.41,Rifle,Basic,Bottom;37690.82,Rifle,Scatter,Top" },
            new object[] { "42|15|1|1|71000|118827|10", "" },
            new object[] { "42|15|1.5|0.6|71000|118827|15", "" },
            new object[] { "12345|1|1|1|15000|20264|1", "3680.91,Rifle,Basic,Top" },
            new object[] { "12345|1|1.5|0.6|15000|20264|1", "3680.91,Rifle,Basic,Top" },
            new object[] { "12345|2|1|1|19000|28183|1", "" },
            new object[] { "12345|2|1.5|0.6|19000|28183|2", "" },
            new object[] { "12345|3|1|1|23000|36102|2", "" },
            new object[] { "12345|3|1.5|0.6|23000|36102|3", "" },
            new object[] { "12345|5|1|1|31000|51940|3", "" },
            new object[] { "12345|5|1.5|0.6|31000|51940|5", "" },
            new object[] { "12345|9|1|1|47000|83616|6", "25237.035,Rifle,Basic,Bottom;28438.662,Rifle,Heavy,Bottom;34915.85,Rifle,Burst,Bottom;36496.535,Rifle,Frangible,Top;38176.523,Rifle,Heavy,Top;43694.152,Rifle,Frangible,Bottom" },
            new object[] { "12345|9|1.5|0.6|47000|83616|9", "10368.059,Rifle,Piercing,Bottom;25237.035,Rifle,Basic,Bottom;25464.236,Rifle,RapidFire,Top;28438.662,Rifle,Heavy,Bottom;34915.85,Rifle,Burst,Bottom;36496.535,Rifle,Frangible,Top;38176.523,Rifle,Heavy,Top;41618.23,Rifle,Scatter,Bottom;43694.152,Rifle,Frangible,Bottom" },
            new object[] { "12345|15|1|1|71000|131130|11", "" },
            new object[] { "12345|15|1.5|0.6|71000|131130|17", "" },
            new object[] { "-7|1|1|1|15000|7912|1", "" },
            new object[] { "-7|1|1.5|0.6|15000|7912|1", "" },
            new object[] { "-7|2|1|1|19000|15831|1", "" },
            new object[] { "-7|2|1.5|0.6|19000|15831|2", "" },
            new object[] { "-7|3|1|1|23000|23750|2", "8221.65,Rifle,Heavy,Bottom;17727.488,Rifle,Scatter,Bottom" },
            new object[] { "-7|3|1.5|0.6|23000|23750|3", "6799.3013,Rifle,Basic,Top;8221.65,Rifle,Heavy,Bottom;17727.488,Rifle,Scatter,Bottom" },
            new object[] { "-7|5|1|1|31000|39588|4", "" },
            new object[] { "-7|5|1.5|0.6|31000|39588|5", "" },
            new object[] { "-7|9|1|1|47000|71264|6", "" },
            new object[] { "-7|9|1.5|0.6|47000|71264|9", "" },
            new object[] { "-7|15|1|1|71000|118778|12", "4916.562,Rifle,Burst,Bottom;14358.365,Rifle,Scatter,Bottom;26134.205,Rifle,Scatter,Top;27016.926,Rifle,Basic,Top;27521.682,Rifle,Heavy,Bottom;32171.006,Rifle,Basic,Bottom;34284.43,Rifle,Frangible,Top;43712.668,Rifle,Scatter,Top;48533.344,Rifle,Heavy,Top;55035.875,Rifle,Frangible,Top;57413.195,Rifle,Basic,Top;61132.207,Rifle,Piercing,Top" },
            new object[] { "-7|15|1.5|0.6|71000|118778|18", "2684.7886,Rifle,Basic,Bottom;3157.1453,Rifle,Basic,Bottom;4916.562,Rifle,Burst,Bottom;14358.365,Rifle,Scatter,Bottom;26134.205,Rifle,Scatter,Top;27016.926,Rifle,Basic,Top;27521.682,Rifle,Heavy,Bottom;31822.633,Rifle,Basic,Top;32171.006,Rifle,Basic,Bottom;33592.15,Rifle,RapidFire,Top;34284.43,Rifle,Frangible,Top;39488.375,Rifle,Frangible,Bottom;43712.668,Rifle,Scatter,Top;48533.344,Rifle,Heavy,Top;49186.918,Rifle,Scatter,Top;55035.875,Rifle,Frangible,Top;57413.195,Rifle,Heavy,Top;61132.207,Rifle,Piercing,Top" }
        };

        [TestCaseSource(nameof(Cases))]
        public void MatchesSrcGenerator(string head, string events)
        {
            string[] h = head.Split('|');
            var cfg = new LevelGenerationConfig { RandomSeed = int.Parse(h[0], CultureInfo.InvariantCulture) };
            float ts = float.Parse(h[2], CultureInfo.InvariantCulture);
            float ss = float.Parse(h[3], CultureInfo.InvariantCulture);

            LevelDefinition def = new ProceduralLevelGenerator(cfg, ts, ss).Generate(int.Parse(h[1], CultureInfo.InvariantCulture));

            Assert.AreEqual(float.Parse(h[4], CultureInfo.InvariantCulture), def.LevelDistance);
            Assert.AreEqual(int.Parse(h[5], CultureInfo.InvariantCulture), def.LevelSeed);
            Assert.AreEqual(int.Parse(h[6], CultureInfo.InvariantCulture), def.SpawnEvents.Count);
            if (events.Length == 0) return;

            string[] expected = events.Split(';');
            for (int i = 0; i < expected.Length; i++)
            {
                string[] e = expected[i].Split(',');
                SpawnEvent actual = def.SpawnEvents[i];
                Assert.AreEqual(float.Parse(e[0], CultureInfo.InvariantCulture), actual.Distance, 0.01f, "distance " + i); // Mono differs from .NET by 1 float ulp (~0.002 above 16384) in a few events
                Assert.AreEqual(e[1], actual.Type, "type " + i);
                Assert.AreEqual(e[2], actual.AmmoId, "ammo " + i);
                Assert.AreEqual(e[3], actual.Side, "side " + i);
            }
        }
    }
}
