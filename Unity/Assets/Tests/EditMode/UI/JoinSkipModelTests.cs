using Gamelab.UI;
using NUnit.Framework;

namespace Gamelab.Tests.UI
{
    // Skip goldens come from evaluating the Src UpdateSkipProgress expression in float32 with a script.
    public class JoinSkipModelTests
    {
        private const float Tol = 1e-4f;
        private const float Dt = 1f / 60f;

        private static SkipTutorialModel Run(SkipTutorialModel m, int frames, bool held)
        {
            for (int i = 0; i < frames; i++) m.Update(Dt, held);
            return m;
        }

        [Test]
        public void HoldFillsAtTwoPerSecond()
        {
            Assert.AreEqual(0.0333333f, Run(new SkipTutorialModel(), 1, true).Progress, Tol);
            Assert.AreEqual(0.3333333f, Run(new SkipTutorialModel(), 10, true).Progress, Tol);
            Assert.AreEqual(1.0f, Run(new SkipTutorialModel(), 30, true).Progress, Tol);
            Assert.AreEqual(1.4666666f, Run(new SkipTutorialModel(), 44, true).Progress, Tol);
        }

        [Test]
        public void FullHoldTakesThreeQuartersOfASecond()
        {
            var m = new SkipTutorialModel();
            m.Update(0.75f, true);
            Assert.AreEqual(1.5f, m.Progress, 0f);
            Assert.AreEqual(1f, m.ProgressRatio, 0f);
            Assert.IsTrue(m.SkipRequested);
        }

        [Test]
        public void ReleaseDrainsAtFivePerSecondAndFloorsAtZero()
        {
            var m = Run(new SkipTutorialModel(), 30, true);
            Run(m, 1, false);
            Assert.AreEqual(0.9166670f, m.Progress, Tol);
            Run(m, 5, false);
            Assert.AreEqual(0.5f, m.Progress, Tol);
            Run(m, 60, false);
            Assert.AreEqual(0f, m.Progress, 0f);
        }

        [Test]
        public void CapAt1Point5AndRequestFirstRaisedOnFrame46()
        {
            var m = Run(new SkipTutorialModel(), 45, true);
            Assert.AreEqual(1.4999999f, m.Progress, Tol);
            Assert.IsFalse(m.SkipRequested);
            Run(m, 1, true);
            Assert.AreEqual(1.5f, m.Progress, 0f);
            Assert.IsTrue(m.SkipRequested);
            Run(m, 44, true);
            Assert.AreEqual(1.5f, m.Progress, 0f);
        }

        [Test]
        public void RatioIsProgressOverCap()
        {
            var m = Run(new SkipTutorialModel(), 30, true);
            Assert.AreEqual(m.Progress / 1.5f, m.ProgressRatio, 1e-6f);
            Assert.AreEqual(0.6666667f, m.ProgressRatio, Tol);
        }

        [Test]
        public void RequestStaysUntilConsumedEvenWhenProgressDrains()
        {
            var m = Run(new SkipTutorialModel(), 60, true);
            Run(m, 20, false);
            Assert.AreEqual(0f, m.Progress, 0f);
            Assert.IsTrue(m.SkipRequested);
            Assert.IsTrue(m.ConsumeRequest());
            Assert.IsFalse(m.SkipRequested);
            Assert.IsFalse(m.ConsumeRequest());
        }

        [Test]
        public void NoRequestWhenNeverReachingTheCap()
        {
            var m = Run(new SkipTutorialModel(), 30, true);
            Run(m, 10, false);
            Assert.IsFalse(m.SkipRequested);
        }

        [Test]
        public void SkipTextMatchesSrc() => Assert.AreEqual("Hold to skip tutorial", SkipTutorialModel.Text);

        private static readonly string[] Figures = { "IdleA0", "IdleA3", "IdleA1", "IdleA2" };

        [Test]
        public void JoinModelStatesForZeroToFourJoined()
        {
            int count = 0;
            var m = new JoinScreenModel(() => count);
            for (count = 0; count <= 4; count++)
            {
                m.Refresh();
                Assert.AreEqual(count == 0 ? "Enter the train" : "Press start to advance", m.Title);
                for (int s = 0; s < 4; s++)
                {
                    bool joined = s < count;
                    Assert.AreEqual(joined, m.IsJoined(s));
                    Assert.AreEqual(joined ? Figures[s] : "Silhouette", m.FigureId(s));
                    Assert.AreEqual(joined ? "Joined" : "Join", m.ButtonText(s));
                    Assert.AreEqual(joined ? XboxButtonAtlas.Face.Start : XboxButtonAtlas.Face.A, m.ButtonFace(s));
                }
            }
        }

        [Test]
        public void CountAboveFourClampsToFourSlots()
        {
            var m = new JoinScreenModel(() => 7);
            m.Refresh();
            for (int s = 0; s < 4; s++) Assert.IsTrue(m.IsJoined(s));
            Assert.AreEqual("Press start to advance", m.Title);
        }

        [Test]
        public void ChangedRaisedOncePerChangeAndNotOnUnchangedRefresh()
        {
            int count = 0;
            var m = new JoinScreenModel(() => count);
            int raised = 0;
            m.Changed += () => raised++;
            m.Refresh();
            Assert.AreEqual(0, raised);
            count = 1;
            m.Refresh();
            m.Refresh();
            Assert.AreEqual(1, raised);
            count = 3;
            m.Refresh();
            Assert.AreEqual(2, raised);
            count = 7;
            m.Refresh();
            Assert.AreEqual(3, raised);
            count = 4;
            m.Refresh();
            Assert.AreEqual(3, raised);
            count = 0;
            m.Refresh();
            Assert.AreEqual(4, raised);
        }
    }
}
