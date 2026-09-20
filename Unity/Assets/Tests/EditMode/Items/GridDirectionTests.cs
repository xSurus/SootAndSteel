using System.Numerics;
using Gamelab.Utils;
using NUnit.Framework;

namespace Gamelab.Tests.Items
{
    public class GridDirectionTests
    {
        [TestCase(GridDirection.Up, GridDirection.Down)]
        [TestCase(GridDirection.Down, GridDirection.Up)]
        [TestCase(GridDirection.Left, GridDirection.Right)]
        [TestCase(GridDirection.Right, GridDirection.Left)]
        public void Opposite(GridDirection d, GridDirection expected) => Assert.AreEqual(expected, d.Opposite());

        [TestCase(GridDirection.Up, GridDirection.Right)]
        [TestCase(GridDirection.Right, GridDirection.Down)]
        [TestCase(GridDirection.Down, GridDirection.Left)]
        [TestCase(GridDirection.Left, GridDirection.Up)]
        public void Clockwise(GridDirection d, GridDirection expected) => Assert.AreEqual(expected, d.Clockwise());

        [TestCase(GridDirection.Up, 0f, -1f)]
        [TestCase(GridDirection.Down, 0f, 1f)]
        [TestCase(GridDirection.Left, -1f, 0f)]
        [TestCase(GridDirection.Right, 1f, 0f)]
        public void ToVector2(GridDirection d, float x, float y) => Assert.AreEqual(new Vector2(x, y), d.ToVector2());
    }
}
