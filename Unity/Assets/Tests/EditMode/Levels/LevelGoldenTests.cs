using System.Globalization;
using Gamelab.Levels;
using NUnit.Framework;

namespace Gamelab.Tests.Levels
{
    /// <summary>
    /// Golden values printed by the real Src ProceduralLevelGenerator (net10.0, default LevelGenerationConfig with
    /// RandomSeed set). Row: seed|level|threatScale|spacingScale|distance|levelSeed|eventCount, then events
    /// "distance,type,ammo,side" (round-trip floats). Every row pins all events.
    /// </summary>
    public class LevelGoldenTests
    {
        private static readonly object[] Cases =
        {
            new object[] { "1|1|1|1|15000|7920|1", "5682.048,Rifle,Basic,Bottom" },
            new object[] { "1|1|1.5|0.6|15000|7920|1", "5682.048,Rifle,Basic,Bottom" },
            new object[] { "1|2|1|1|19000|15839|1", "8311.921,Rifle,Basic,Top" },
            new object[] { "1|2|1.5|0.6|19000|15839|2", "6211.081,Rifle,Basic,Top;8311.921,Rifle,Basic,Top" },
            new object[] { "1|3|1|1|23000|23758|2", "11630.297,Rifle,Scatter,Top;12170.814,Rifle,Heavy,Bottom" },
            new object[] { "1|3|1.5|0.6|23000|23758|3", "11630.297,Rifle,Scatter,Top;12170.814,Rifle,Heavy,Bottom;17903.918,Rifle,Basic,Top" },
            new object[] { "1|5|1|1|31000|39596|4", "3199.8975,Rifle,Basic,Top;6293.669,Rifle,Basic,Bottom;11238.501,Rifle,Burst,Bottom;20332.566,Rifle,Basic,Bottom" },
            new object[] { "1|5|1.5|0.6|31000|39596|6", "3199.8975,Rifle,Basic,Top;6293.669,Rifle,Basic,Bottom;11238.501,Rifle,Burst,Bottom;17113.969,Rifle,Basic,Top;20332.566,Rifle,Basic,Bottom;27509.207,Rifle,Scatter,Top" },
            new object[] { "1|9|1|1|47000|71272|6", "2999.1548,Rifle,Heavy,Top;4696.5244,Rifle,Scatter,Top;29714.604,Rifle,Basic,Top;36528.035,Rifle,Scatter,Bottom;39998.566,Rifle,Burst,Bottom;40783.23,Rifle,Scatter,Bottom" },
            new object[] { "1|9|1.5|0.6|47000|71272|10", "2999.1548,Rifle,Heavy,Top;3623.5483,Rifle,Basic,Bottom;4696.5244,Rifle,Scatter,Top;18603.324,Rifle,Scatter,Bottom;22861.865,Rifle,Heavy,Bottom;29714.604,Rifle,Basic,Top;34815.57,Rifle,Scatter,Top;36528.035,Rifle,Scatter,Bottom;39998.566,Rifle,Burst,Bottom;40783.23,Rifle,Scatter,Bottom" },
            new object[] { "1|15|1|1|71000|118786|11", "15007.112,Rifle,Scatter,Bottom;19426.29,Rifle,Piercing,Bottom;24400.951,Rifle,Heavy,Bottom;26760.125,Rifle,Frangible,Top;29243.729,Rifle,Frangible,Bottom;29671.475,Rifle,Scatter,Top;31394.295,Rifle,Piercing,Bottom;38154.176,Rifle,Piercing,Bottom;41537.617,Rifle,Heavy,Top;51686.465,Rifle,Frangible,Top;53516.758,Rifle,Scatter,Bottom" },
            new object[] { "1|15|1.5|0.6|71000|118786|16", "2934.5967,Rifle,Basic,Top;3851.0496,Rifle,Basic,Bottom;11159.006,Rifle,Frangible,Top;15007.112,Rifle,Scatter,Bottom;19426.29,Rifle,Piercing,Bottom;24182.146,Rifle,RapidFire,Bottom;24400.951,Rifle,Scatter,Bottom;26760.125,Rifle,Frangible,Top;29243.729,Rifle,Frangible,Bottom;29671.475,Rifle,Scatter,Top;31394.295,Rifle,Piercing,Bottom;34845.64,Rifle,RapidFire,Top;38154.176,Rifle,Piercing,Bottom;41537.617,Rifle,Heavy,Top;51686.465,Rifle,Frangible,Top;53516.758,Rifle,Scatter,Bottom" },
            new object[] { "42|1|1|1|15000|7961|1", "10295.865,Rifle,Basic,Top" },
            new object[] { "42|1|1.5|0.6|15000|7961|1", "10295.865,Rifle,Basic,Top" },
            new object[] { "42|2|1|1|19000|15880|1", "14603.489,Rifle,Basic,Bottom" },
            new object[] { "42|2|1.5|0.6|19000|15880|2", "2478.4834,Rifle,Basic,Top;14603.489,Rifle,Basic,Bottom" },
            new object[] { "42|3|1|1|23000|23799|2", "7442.8574,Rifle,Basic,Top;19599.617,Rifle,Scatter,Bottom" },
            new object[] { "42|3|1.5|0.6|23000|23799|3", "5940.087,Rifle,Basic,Top;7442.8574,Rifle,Basic,Top;19599.617,Rifle,Scatter,Bottom" },
            new object[] { "42|5|1|1|31000|39637|4", "4657.388,Rifle,Basic,Bottom;14445.966,Rifle,Basic,Bottom;16292.434,Rifle,Frangible,Bottom;23481.223,Rifle,Frangible,Top" },
            new object[] { "42|5|1.5|0.6|31000|39637|5", "4657.388,Rifle,Basic,Bottom;5517.529,Rifle,Burst,Bottom;14445.966,Rifle,Basic,Bottom;16292.434,Rifle,Frangible,Bottom;23481.223,Rifle,Frangible,Top" },
            new object[] { "42|9|1|1|47000|71313|6", "2891.4165,Rifle,Burst,Bottom;9451.994,Rifle,Basic,Bottom;21034.984,Rifle,Heavy,Bottom;23028.908,Rifle,Homing,Top;36996.41,Rifle,Basic,Bottom;37690.82,Rifle,Scatter,Top" },
            new object[] { "42|9|1.5|0.6|47000|71313|10", "2891.4165,Rifle,Burst,Bottom;8348.639,Rifle,Basic,Top;9451.994,Rifle,Basic,Bottom;21034.984,Rifle,Heavy,Bottom;23028.908,Rifle,Homing,Top;25390.463,Rifle,Basic,Bottom;35599.64,Rifle,Piercing,Top;35972.156,Rifle,Basic,Bottom;36996.41,Rifle,Basic,Bottom;37690.82,Rifle,Scatter,Top" },
            new object[] { "42|15|1|1|71000|118827|10", "11328.505,Rifle,Basic,Bottom;24865.35,Rifle,RapidFire,Top;26788.64,Rifle,Burst,Top;27435.17,Rifle,Piercing,Bottom;30465.424,Rifle,Basic,Top;33611.383,Rifle,Heavy,Bottom;58457.363,Rifle,Basic,Bottom;59645.727,Rifle,Piercing,Top;66256.516,Rifle,RapidFire,Top;67319.03,Rifle,RapidFire,Bottom" },
            new object[] { "42|15|1.5|0.6|71000|118827|15", "5720.0522,Rifle,Scatter,Top;11328.505,Rifle,Basic,Bottom;16215.148,Rifle,Piercing,Bottom;21657.69,Rifle,Piercing,Bottom;24865.35,Rifle,RapidFire,Top;26788.64,Rifle,Burst,Top;27435.17,Rifle,Piercing,Bottom;30465.424,Rifle,Basic,Top;33532.207,Rifle,Heavy,Top;33611.383,Rifle,Heavy,Bottom;36254.055,Rifle,Burst,Bottom;58457.363,Rifle,Basic,Bottom;59645.727,Rifle,Piercing,Top;66256.516,Rifle,RapidFire,Top;67319.03,Rifle,RapidFire,Bottom" },
            new object[] { "12345|1|1|1|15000|20264|1", "3680.91,Rifle,Basic,Top" },
            new object[] { "12345|1|1.5|0.6|15000|20264|1", "3680.91,Rifle,Basic,Top" },
            new object[] { "12345|2|1|1|19000|28183|1", "5583.0957,Rifle,Basic,Top" },
            new object[] { "12345|2|1.5|0.6|19000|28183|2", "2304.1082,Rifle,Basic,Bottom;5583.0957,Rifle,Basic,Top" },
            new object[] { "12345|3|1|1|23000|36102|2", "7221.9824,Rifle,Heavy,Top;8173.786,Rifle,Scatter,Bottom" },
            new object[] { "12345|3|1.5|0.6|23000|36102|3", "7221.9824,Rifle,Heavy,Top;8173.786,Rifle,Scatter,Bottom;14329.545,Rifle,Basic,Top" },
            new object[] { "12345|5|1|1|31000|51940|3", "15420.682,Rifle,Piercing,Top;23167.346,Rifle,Heavy,Bottom;28214.295,Rifle,Piercing,Bottom" },
            new object[] { "12345|5|1.5|0.6|31000|51940|5", "7554.332,Rifle,Basic,Bottom;15420.682,Rifle,Piercing,Top;23167.346,Rifle,Heavy,Bottom;24697.709,Rifle,Heavy,Bottom;28214.295,Rifle,Piercing,Bottom" },
            new object[] { "12345|9|1|1|47000|83616|6", "25237.035,Rifle,Basic,Bottom;28438.662,Rifle,Heavy,Bottom;34915.85,Rifle,Burst,Bottom;36496.535,Rifle,Frangible,Top;38176.523,Rifle,Heavy,Top;43694.152,Rifle,Frangible,Bottom" },
            new object[] { "12345|9|1.5|0.6|47000|83616|9", "10368.059,Rifle,Piercing,Bottom;25237.035,Rifle,Basic,Bottom;25464.236,Rifle,RapidFire,Top;28438.662,Rifle,Heavy,Bottom;34915.85,Rifle,Burst,Bottom;36496.535,Rifle,Frangible,Top;38176.523,Rifle,Heavy,Top;41618.23,Rifle,Scatter,Bottom;43694.152,Rifle,Frangible,Bottom" },
            new object[] { "12345|15|1|1|71000|131130|11", "8030.4346,Rifle,Scatter,Top;14673.046,Rifle,Basic,Bottom;24086.473,Rifle,Scatter,Bottom;25965.424,Rifle,Piercing,Top;29935.178,Rifle,Scatter,Top;32518.363,Rifle,Scatter,Bottom;33113.816,Rifle,Homing,Bottom;40912.383,Rifle,Heavy,Bottom;42544.266,Rifle,Burst,Bottom;58896.51,Rifle,Frangible,Bottom;60918.15,Rifle,Heavy,Bottom" },
            new object[] { "12345|15|1.5|0.6|71000|131130|17", "8030.4346,Rifle,Scatter,Top;9343.639,Rifle,Frangible,Bottom;14673.046,Rifle,Basic,Bottom;24086.473,Rifle,Scatter,Bottom;25965.424,Rifle,Piercing,Top;29935.178,Rifle,Scatter,Top;32518.363,Rifle,Scatter,Bottom;33113.816,Rifle,Matryoshka,Bottom;33819.113,Rifle,Heavy,Bottom;35460.08,Rifle,Scatter,Bottom;40912.383,Rifle,Heavy,Bottom;42544.266,Rifle,Burst,Bottom;49941.82,Rifle,Scatter,Top;50435.453,Rifle,Basic,Top;58896.51,Rifle,Frangible,Bottom;60918.15,Rifle,Heavy,Bottom;67335.14,Rifle,Scatter,Bottom" },
            new object[] { "-7|1|1|1|15000|7912|1", "3708.6206,Rifle,Basic,Top" },
            new object[] { "-7|1|1.5|0.6|15000|7912|1", "3708.6206,Rifle,Basic,Top" },
            new object[] { "-7|2|1|1|19000|15831|1", "5620.883,Rifle,Basic,Top" },
            new object[] { "-7|2|1.5|0.6|19000|15831|2", "5620.883,Rifle,Basic,Top;10597.929,Rifle,Basic,Top" },
            new object[] { "-7|3|1|1|23000|23750|2", "8221.65,Rifle,Heavy,Bottom;17727.488,Rifle,Scatter,Bottom" },
            new object[] { "-7|3|1.5|0.6|23000|23750|3", "6799.3013,Rifle,Basic,Top;8221.65,Rifle,Heavy,Bottom;17727.488,Rifle,Scatter,Bottom" },
            new object[] { "-7|5|1|1|31000|39588|4", "11096.225,Rifle,Scatter,Top;12588.264,Rifle,Basic,Top;15488.697,Rifle,Frangible,Top;17513.422,Rifle,Basic,Bottom" },
            new object[] { "-7|5|1.5|0.6|31000|39588|5", "11096.225,Rifle,Scatter,Top;12588.264,Rifle,Piercing,Top;14019.778,Rifle,Heavy,Top;15488.697,Rifle,Frangible,Top;17513.422,Rifle,Basic,Bottom" },
            new object[] { "-7|9|1|1|47000|71264|6", "8231.439,Rifle,Homing,Bottom;11396.532,Rifle,Scatter,Bottom;17272.156,Rifle,RapidFire,Top;38284.848,Rifle,Basic,Top;40163.38,Rifle,Basic,Bottom;42932.855,Rifle,Frangible,Top" },
            new object[] { "-7|9|1.5|0.6|47000|71264|9", "8231.439,Rifle,Homing,Bottom;11396.532,Rifle,Scatter,Bottom;17272.156,Rifle,RapidFire,Top;24466.041,Rifle,Homing,Bottom;37311.336,Rifle,Basic,Bottom;38284.848,Rifle,Basic,Top;40163.38,Rifle,Heavy,Bottom;42932.855,Rifle,Frangible,Top;44101.605,Rifle,Basic,Top" },
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

            string[] expected = events.Split(';');
            for (int i = 0; i < expected.Length; i++)
            {
                string[] e = expected[i].Split(',');
                SpawnEvent actual = def.SpawnEvents[i];
                Assert.That(actual.Distance, Is.EqualTo(float.Parse(e[0], CultureInfo.InvariantCulture)).Within(1).Ulps, "distance " + i); // Bit-identical to Src on .NET; 1 ulp slack for Mono float widening (exact failed on 13 rows, e.g. "1|5|1|1|31000|39596|4" event 0, all off by exactly 1 ulp)
                Assert.AreEqual(e[1], actual.Type, "type " + i);
                Assert.AreEqual(e[2], actual.AmmoId, "ammo " + i);
                Assert.AreEqual(e[3], actual.Side, "side " + i);
            }
        }
    }
}
