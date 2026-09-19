// Unity/Assets/Tests/EditMode/Stations/StationIdsTests.cs
using NUnit.Framework;
using Gamelab.PhysicalEntities.Stations;

namespace Gamelab.Tests.Stations
{
    public class StationIdsTests
    {
        [Test]
        public void GetComponentResourceId_PrefixesWithResourceAndComponent()
        {
            // Mirrors Src/: a component station id is Resource + Component + componentId.
            string id = StationIds.GetResourceStationId(StationIds.GetComponentResourceId("BasicCasing"));

            Assert.AreEqual("ResourceComponentBasicCasing", id);
            Assert.IsTrue(StationIds.IsComponentStationId(id));
        }

        [Test]
        public void GetResourceStationId_PrefixesWithResource()
        {
            string id = StationIds.GetResourceStationId("Coal");

            Assert.AreEqual("ResourceCoal", id);
            Assert.IsTrue(StationIds.IsResourceStationId(id));
            Assert.AreEqual("Coal", StationIds.GetStationResourceId(id));
        }
    }
}
