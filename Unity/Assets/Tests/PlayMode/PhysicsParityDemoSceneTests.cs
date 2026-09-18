using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Gamelab.PhysicalEntities;

namespace Gamelab.Tests.PhysicalEntities
{
    public class PhysicsParityDemoSceneTests
    {
        [UnityTest]
        public IEnumerator PhysicsParityDemoScene_BodyMovesTowardWallAndComesToRest()
        {
            yield return SceneManager.LoadSceneAsync("PhysicsParityDemo", LoadSceneMode.Additive);

            PhysicalEntity entity = Object.FindFirstObjectByType<PhysicalEntity>();
            Assert.IsNotNull(entity, "PhysicsParityDemo.unity should contain a PhysicalEntity");

            float startX = entity.Position.x;

            for (int i = 0; i < 90; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.Greater(entity.Position.x, startX, "the body should have moved toward the wall");
            Assert.Less(entity.Position.x, -0.05f, "the body should have stopped at the wall, not passed through it");

            yield return SceneManager.UnloadSceneAsync("PhysicsParityDemo");
        }
    }
}
