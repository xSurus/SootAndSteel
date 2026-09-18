using NUnit.Framework;
using UnityEngine;

namespace Gamelab.Tests.PhysicalEntities
{
    /// <summary>
    /// Regression guard for the ProjectSettings retuning in this plan's Task 1: if these ever drift back
    /// to Unity's defaults, every dynamic PhysicalEntity's behavior silently stops matching the Aether
    /// reference (gravity would pull bodies down; the fixed step would run at 50Hz instead of 60Hz).
    /// </summary>
    public class PhysicsWorldSettingsTests
    {
        [Test]
        public void Physics2D_Gravity_IsZero_MatchingAetherWorld()
        {
            Assert.AreEqual(Vector2.zero, Physics2D.gravity);
        }

        [Test]
        public void FixedTimestep_Matches_OriginalGameplayConfig()
        {
            Assert.AreEqual(1f / 60f, Time.fixedDeltaTime, 0.0001f);
        }

        [Test]
        public void MaximumAllowedTimestep_Matches_OriginalMaxAccumulatedDelta()
        {
            Assert.AreEqual(0.25f, Time.maximumDeltaTime, 0.0001f);
        }
    }
}
