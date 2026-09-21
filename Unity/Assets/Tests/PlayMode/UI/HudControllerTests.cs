using System;
using System.Collections;
using System.Collections.Generic;
using Gamelab.Config;
using Gamelab.Levels;
using Gamelab.Map.Train.State;
using Gamelab.UI;
using Gamelab.UI.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Gamelab.Tests.UI
{
    public class HudControllerTests
    {
        private readonly List<UnityEngine.Object> cleanup = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in cleanup) if (o != null) UnityEngine.Object.DestroyImmediate(o);
            cleanup.Clear();
        }

        private static LevelDefinition Level(params float[] distances)
        {
            var def = new LevelDefinition { LevelDistance = 1000f };
            foreach (float d in distances) def.SpawnEvents.Add(new SpawnEvent { Distance = d });
            return def;
        }

        private static TrainState NewState() =>
            new TrainState(TrainSpeedSetting.CreateSet(TrainSpeedTuning.Default), TrainStateTuning.Default);

        private HudController Make(TrainState state, LevelDefinition level)
        {
            var go = new GameObject("Hud");
            cleanup.Add(go);
            var c = go.AddComponent<HudController>();
            c.Bind(state, level);
            return c;
        }

        private static float MarkerLeft(HudController c) => c.View.TrainMarker.resolvedStyle.left;

        [UnityTest]
        public IEnumerator NeedleConvergesAndMarkerMoves()
        {
            var state = NewState();
            var c = Make(state, Level());
            yield return null;
            for (int i = 0; i < 80; i++) c.Tick();
            Assert.AreEqual(358.3276f, c.Model.NeedleDegrees, 0.05f);
            float before = c.Model.DistanceRatio;
            state.Update(1f);
            c.Tick();
            Assert.Greater(c.Model.DistanceRatio, before);
            yield return null;
            float x1 = MarkerLeft(c);
            state.Update(1f);
            c.Tick();
            yield return null;
            Assert.Greater(MarkerLeft(c), x1);
        }

        [UnityTest]
        public IEnumerator DistanceBeyondLevelClampsAtEnd()
        {
            var state = NewState();
            var c = Make(state, Level());
            state.Update(100f);
            c.Tick();
            Assert.AreEqual(1f, c.Model.DistanceRatio, 1e-5f);
            yield return null;
            Assert.AreEqual(455.5f - 16f, MarkerLeft(c), 3f);
        }

        [UnityTest]
        public IEnumerator FrostFollowsTemperature()
        {
            var state = NewState();
            var c = Make(state, Level());
            c.Tick();
            Assert.AreEqual(0f, c.Model.FrostOpacity1, 1e-5f);
            state.DecreaseTemperature(state.MaxTemperature * 0.5f);
            c.Tick();
            yield return null;
            Assert.Greater(c.View.Frost1.resolvedStyle.opacity, 0.1f);
            state.IncreaseTemperature(state.MaxTemperature);
            c.Tick();
            yield return null;
            Assert.AreEqual(0f, c.View.Frost1.resolvedStyle.opacity, 0.01f);
        }

        [UnityTest]
        public IEnumerator SetLevelRebuildsOnlyOnNewReference()
        {
            var first = Level(100f);
            var c = Make(NewState(), first);
            yield return null;
            int version = c.Model.LevelVersion;
            Assert.AreEqual(1, c.View.Dots.Count);
            c.SetLevel(first);
            Assert.AreEqual(version, c.Model.LevelVersion);
            c.SetLevel(Level(100f, 200f, 300f));
            c.Tick();
            Assert.AreEqual(version + 1, c.Model.LevelVersion);
            Assert.AreEqual(3, c.View.Dots.Count);
        }

        [UnityTest]
        public IEnumerator WorksWhilePaused()
        {
            float old = Time.timeScale;
            Time.timeScale = 0f;
            try
            {
                var state = NewState();
                var c = Make(state, Level());
                yield return null;
                state.Update(1f);
                yield return null;
                yield return null;
                Assert.Greater(c.Model.DistanceRatio, 0f);
                Assert.AreEqual(state.DistanceTraveled / 1000f, c.Model.DistanceRatio, 1e-4f);
            }
            finally { Time.timeScale = old; }
        }

        [Test]
        public void BindTwiceThrows()
        {
            var c = Make(NewState(), Level());
            Assert.Throws<InvalidOperationException>(() => c.Bind(NewState(), Level()));
        }

        [UnityTest]
        public IEnumerator RuntimeOverloadShowsLevelRatio()
        {
            var go = new GameObject("Run");
            cleanup.Add(go);
            var train = go.AddComponent<TrainStateRuntime>();
            train.SelfTick = false;
            var level = go.AddComponent<LevelRuntime>();
            var def = Level();
            level.Initialize(def, train, () => false);
            var c = go.AddComponent<HudController>();
            c.Bind(train, level);
            train.Tick(1f);
            yield return null;
            Assert.AreEqual(train.State.DistanceTraveled / def.LevelDistance, c.Model.DistanceRatio, 1e-4f);
            Assert.Greater(c.Model.DistanceRatio, 0f);
        }

        [UnityTest]
        public IEnumerator SortingOrderBelowMenus()
        {
            var c = Make(NewState(), Level());
            yield return null;
            Assert.Less(c.View.GetComponent<UIDocument>().sortingOrder, 0f);
        }

        [UnityTest]
        public IEnumerator DestroyDestroysPanel()
        {
            var c = Make(NewState(), Level());
            yield return null;
            var panel = c.View.GetComponent<UIDocument>().panelSettings;
            UnityEngine.Object.Destroy(c.gameObject);
            yield return null;
            yield return null;
            Assert.IsTrue(panel == null);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
