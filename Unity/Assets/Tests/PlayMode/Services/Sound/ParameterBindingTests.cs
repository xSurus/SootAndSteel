using NUnit.Framework;
using Gamelab.Services.Sound;

namespace Gamelab.Tests.Services
{
    public class ParameterBindingTests
    {
        [Test]
        public void Update_CallsSetterWithCurrentGetterValue()
        {
            float current = 1f;
            float captured = -1f;
            var binding = new ParameterBinding(v => captured = v, () => current);

            binding.Update();
            Assert.AreEqual(1f, captured, 0.0001f);

            current = 2.5f;
            binding.Update();
            Assert.AreEqual(2.5f, captured, 0.0001f);
        }

        [Test]
        public void Deactivate_SetsActiveFalse()
        {
            var binding = new ParameterBinding(v => { }, () => 0f);

            Assert.IsTrue(binding.active);
            binding.Deactivate();
            Assert.IsFalse(binding.active);
        }
    }
}
