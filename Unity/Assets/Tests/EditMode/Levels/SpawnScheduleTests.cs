using System.Collections.Generic;
using Gamelab.Levels;
using NUnit.Framework;

namespace Gamelab.Tests.Levels
{
    public class SpawnScheduleTests
    {
        private static SpawnSchedule Make(params float[] distances)
        {
            var def = new LevelDefinition();
            foreach (float d in distances) def.SpawnEvents.Add(new SpawnEvent { Distance = d });
            return new SpawnSchedule(def);
        }

        [Test]
        public void PopDue_HandsOutEventsAtOrBeforeDistance_OnceEach_InOrder()
        {
            SpawnSchedule s = Make(100f, 200f, 200f, 500f);
            Assert.AreEqual(0, s.PopDue(99.9f).Count);
            Assert.AreEqual(100f, s.NextEvent.Distance);
            IReadOnlyList<SpawnEvent> due = s.PopDue(100f);
            Assert.AreEqual(1, due.Count);
            Assert.AreEqual(0, s.PopDue(100f).Count);
            Assert.AreEqual(2, s.PopDue(300f).Count);
            Assert.IsFalse(s.IsExhausted);
            Assert.AreEqual(1, s.PopDue(1e9f).Count);
            Assert.IsTrue(s.IsExhausted);
            Assert.IsNull(s.NextEvent);
            Assert.AreEqual(0, s.PopDue(1e9f).Count);
        }

        [Test]
        public void EmptyOrNullDefinition_IsExhausted()
        {
            Assert.IsTrue(Make().IsExhausted);
            Assert.IsTrue(new SpawnSchedule(null).IsExhausted);
        }
    }
}
