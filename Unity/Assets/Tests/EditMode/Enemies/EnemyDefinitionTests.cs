using NUnit.Framework;
using Gamelab.Enemies.Core;

namespace Gamelab.Tests.Enemies
{
    public class EnemyDefinitionTests
    {
        [Test]
        public void Id_ForDummy_ReturnsDummyId()
        {
            var definition = new EnemyDefinition(EnemyType.Dummy);

            Assert.AreEqual(EnemyIds.Dummy, definition.Id);
        }

        [Test]
        public void Parse_RoundTripsWithId()
        {
            EnemyDefinition parsed = EnemyDefinition.Parse(EnemyIds.Rifle);

            Assert.AreEqual(EnemyType.Rifle, parsed.Type);
        }
    }
}
