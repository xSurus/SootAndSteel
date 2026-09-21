using System;
using System.Collections;
using FMOD.Studio;
using Gamelab.Services.Sound;
using Gamelab.UI.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using ParameterBinding = Gamelab.Services.Sound.ParameterBinding;

namespace Gamelab.Tests.UI
{
    public class UiPlumbingTests
    {
        class StubSound : ISoundService
        {
            public SoundSettings Settings { get; } = new SoundSettings { MasterVolume = 0.3f, MusicVolume = 0.4f, SfxVolume = 0.5f };
            public float LastMaster = -1, LastMusic = -1, LastSfx = -1;
            public void LoadSound(string id) { }
            public void UnloadSound(string id) { }
            public void PlayOnce(string id) { }
            public EventInstance GetSoundInstance(string id) => default;
            public ParameterBinding RegisterParameter(EventInstance e, string n, Func<float> g) => null;
            public ParameterBinding RegisterGlobalParameter(string n, Func<float> g) => null;
            public void SetGlobalParameter(string n, float v) { }
            public void ResetGlobalParameters() { }
            public void SetMasterVolume(float v) => LastMaster = v;
            public void SetMusicVolume(float v) => LastMusic = v;
            public void SetSfxVolume(float v) => LastSfx = v;
        }

        [Test]
        public void PanelSettingsValues()
        {
            var ps = UiPanel.Create(7f);
            Assert.AreEqual(PanelScaleMode.ScaleWithScreenSize, ps.scaleMode);
            Assert.AreEqual(new Vector2Int(1920, 1080), ps.referenceResolution);
            Assert.AreEqual(PanelScreenMatchMode.Shrink, ps.screenMatchMode);
            Assert.AreEqual(7f, ps.sortingOrder);
            Assert.NotNull(ps.themeStyleSheet);
            UnityEngine.Object.Destroy(ps);
        }

        [Test]
        public void CommonStyleLoads()
        {
            Assert.NotNull(UiResources.LoadStyle("Common"));
            Assert.IsNull(UiResources.LoadTree("DoesNotExist"));
        }

        [UnityTest]
        public IEnumerator DocumentWithCommonStyleLogsNothing()
        {
            var go = new GameObject("doc");
            var ps = UiPanel.Create();
            var doc = go.AddComponent<UIDocument>();
            doc.panelSettings = ps;
            yield return null;
            var root = doc.rootVisualElement;
            root.styleSheets.Add(UiResources.LoadStyle("Common"));
            var labels = new Label[3];
            var classes = new[] { "font-mono", "font-typewriter", "font-serif" };
            for (int i = 0; i < 3; i++)
            {
                labels[i] = new Label("Hello 123");
                labels[i].AddToClassList(classes[i]);
                labels[i].AddToClassList("paper");
                root.Add(labels[i]);
            }
            yield return null;
            yield return null;
            foreach (var l in labels) Assert.Greater(l.resolvedStyle.width, 0f);
            LogAssert.NoUnexpectedReceived();
            UnityEngine.Object.Destroy(go);
            UnityEngine.Object.Destroy(ps);
        }

        [Test]
        public void VolumeAdapterReadsAndWrites()
        {
            var stub = new StubSound();
            var v = new SoundVolumeSettings(stub);
            Assert.AreEqual(0.3f, v.MasterVolume);
            Assert.AreEqual(0.4f, v.MusicVolume);
            Assert.AreEqual(0.5f, v.SfxVolume);
            v.SetMasterVolume(0.1f); v.SetMusicVolume(0.2f); v.SetSfxVolume(0.6f);
            Assert.AreEqual(0.1f, stub.LastMaster);
            Assert.AreEqual(0.2f, stub.LastMusic);
            Assert.AreEqual(0.6f, stub.LastSfx);
        }
    }
}
