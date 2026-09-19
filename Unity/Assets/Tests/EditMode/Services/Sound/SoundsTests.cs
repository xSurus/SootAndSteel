// Unity/Assets/Tests/EditMode/Services/Sound/SoundsTests.cs
using NUnit.Framework;
using Gamelab.Services.Sound;

namespace Gamelab.Tests.Services
{
    public class SoundsTests
    {
        [Test]
        public void AllEventPaths_StartWithEventPrefix()
        {
            string[] paths =
            {
                Sounds.MenuSelect, Sounds.Stamp, Sounds.Train, Sounds.ShovelUp, Sounds.ShovelDown,
                Sounds.CannonLoad, Sounds.CannonFire, Sounds.Craft, Sounds.WallHit, Sounds.WallBreak,
                Sounds.WallFix, Sounds.WallFixed, Sounds.Walk, Sounds.PickupItem, Sounds.DropItem,
                Sounds.OpenDoor, Sounds.CloseDoor, Sounds.SpeedChange, Sounds.Purchase, Sounds.GrabStation,
                Sounds.DropStation, Sounds.Freeze, Sounds.Fall, Sounds.HorseRiding, Sounds.HorseFlee,
                Sounds.EnemyHit, Sounds.EnemyFire, Sounds.GameOver, Sounds.AmbientSong, Sounds.BattleTheme
            };

            foreach (var path in paths)
            {
                StringAssert.StartsWith("event:/", path);
            }
        }

        [Test]
        public void DefaultGlobalParameterValues_ContainsTemperature()
        {
            Assert.AreEqual(1, Sounds.DefaultGlobalParameterValues.Count);
            Assert.AreEqual("Temperature", Sounds.DefaultGlobalParameterValues[0].Key);
            Assert.AreEqual(1f, Sounds.DefaultGlobalParameterValues[0].Value);
        }
    }
}
