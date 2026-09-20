using System;
using Gamelab.Enemies.Core;
using Gamelab.Levels;
using NUnit.Framework;

namespace Gamelab.Tests.Levels
{
    public class LevelGeneratorTests
    {
        private static readonly int[] Seeds = { 1, 42, 12345, -7 };
        private static readonly int[] Levels = { 1, 2, 3, 5, 9, 15 };

        private static LevelDefinition Gen(int seed, int level, float ts = 1f, float ss = 1f, LevelGenerationConfig cfg = null)
        {
            cfg = cfg ?? new LevelGenerationConfig();
            cfg.RandomSeed = seed;
            return new ProceduralLevelGenerator(cfg, ts, ss).Generate(level);
        }

        [Test]
        public void SameSeedSameOutput()
        {
            LevelDefinition a = Gen(42, 9, 1.5f, 0.6f);
            LevelDefinition b = Gen(42, 9, 1.5f, 0.6f);
            Assert.AreEqual(a.SpawnEvents.Count, b.SpawnEvents.Count);
            for (int i = 0; i < a.SpawnEvents.Count; i++)
            {
                Assert.AreEqual(a.SpawnEvents[i].Distance, b.SpawnEvents[i].Distance);
                Assert.AreEqual(a.SpawnEvents[i].AmmoId, b.SpawnEvents[i].AmmoId);
                Assert.AreEqual(a.SpawnEvents[i].Side, b.SpawnEvents[i].Side);
            }
        }

        [Test]
        public void DifferentLevelDifferentSeed()
        {
            Assert.AreNotEqual(Gen(42, 1).LevelSeed, Gen(42, 2).LevelSeed);
        }

        [Test]
        public void EventsSatisfyInvariants()
        {
            var defaults = new LevelGenerationConfig();
            foreach (int seed in Seeds)
            foreach (int level in Levels)
            foreach (float[] scale in new[] { new[] { 1f, 1f }, new[] { 1.5f, 0.6f } })
            {
                LevelDefinition def = Gen(seed, level, scale[0], scale[1]);
                float minGap = defaults.MinSpawnSpacing * scale[1];
                int spent = 0;
                for (int i = 0; i < def.SpawnEvents.Count; i++)
                {
                    SpawnEvent e = def.SpawnEvents[i];
                    Assert.GreaterOrEqual(e.Distance, defaults.SafeZoneDistance);
                    Assert.That(e.Side, Is.EqualTo("Top").Or.EqualTo("Bottom"));
                    if (i > 0)
                    {
                        Assert.GreaterOrEqual(e.Distance, def.SpawnEvents[i - 1].Distance, "sorted");
                        Assert.GreaterOrEqual(e.Distance - def.SpawnEvents[i - 1].Distance, minGap, "spacing");
                    }

                    spent += EnemyAmmoCatalog.GetCost(defaults.BaseCost, EnemyType.Rifle, e.AmmoId);
                }

                float floatBudget = (defaults.BaseEnemyBudget * MathF.Pow(defaults.BudgetMultiplierPerLevel, level - 1) +
                                     (level - 1) * defaults.BudgetGrowthPerLevel) * scale[0];
                int budget = Math.Max(defaults.BaseCost, (int)MathF.Round(floatBudget));
                Assert.LessOrEqual(spent, budget);
            }
        }

        [Test]
        public void ZeroBudgetLevelOneTerminates()
        {
            LevelDefinition def = Gen(1, 1, cfg: new LevelGenerationConfig { BaseEnemyBudget = 0f });
            Assert.LessOrEqual(def.SpawnEvents.Count, 1);
        }

        [Test]
        public void ProviderMatchesGeneratorWithPlayerCountScales()
        {
            var provider = new ProceduralLevelProvider(3, 42);
            var cfg = new LevelGenerationConfig { RandomSeed = 42 };
            LevelDefinition expected = new ProceduralLevelGenerator(
                cfg, PlayerCountScaling.GetThreatScale(3), PlayerCountScaling.GetEnemySpawnSpacingScale(3)).Generate(5);
            LevelDefinition actual = provider.GetLevel(5);
            Assert.AreEqual(expected.LevelSeed, actual.LevelSeed);
            Assert.AreEqual(expected.SpawnEvents.Count, actual.SpawnEvents.Count);
            for (int i = 0; i < expected.SpawnEvents.Count; i++)
            {
                Assert.AreEqual(expected.SpawnEvents[i].Distance, actual.SpawnEvents[i].Distance);
                Assert.AreEqual(expected.SpawnEvents[i].AmmoId, actual.SpawnEvents[i].AmmoId);
                Assert.AreEqual(expected.SpawnEvents[i].Side, actual.SpawnEvents[i].Side);
            }
        }

        [Test]
        public void PlayerCountScalingValues()
        {
            Assert.AreEqual(1f, PlayerCountScaling.GetThreatScale(1));
            Assert.AreEqual(1f, PlayerCountScaling.GetEnemySpawnSpacingScale(1));
            Assert.AreEqual(1.45f, PlayerCountScaling.GetThreatScale(2), 1e-5f);
            Assert.AreEqual(PlayerCountScaling.GetThreatScale(4), PlayerCountScaling.GetThreatScale(9));
            Assert.Less(PlayerCountScaling.GetEnemySpawnSpacingScale(4), 1f);
            Assert.GreaterOrEqual(PlayerCountScaling.GetEnemySpawnSpacingScale(4), 0.4f);
        }

        [Test]
        public void WatcherNeedsDistanceAndNoThreatsAndFiresOnce()
        {
            var watcher = new LevelCompletionWatcher(new LevelDefinition { LevelDistance = 100f });
            int fired = 0;
            watcher.OnLevelCompleted += () => fired++;

            watcher.Update(50f, false);
            watcher.Update(150f, true);
            Assert.AreEqual(0, fired);
            watcher.Update(100f, false);
            watcher.Update(200f, false);
            Assert.AreEqual(1, fired);
        }

        [Test]
        public void WatcherWithNullDefinitionNeverFires()
        {
            var watcher = new LevelCompletionWatcher(null);
            watcher.OnLevelCompleted += () => Assert.Fail("fired");
            watcher.Update(1e9f, false);
        }
    }
}
