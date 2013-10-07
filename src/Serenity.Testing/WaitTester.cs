using FubuTestingSupport;
using NUnit.Framework;

namespace Serenity.Testing
{
    [TestFixture]
    public class WaitTester
    {
        [Test]
        public void immediately_true()
        {
            Wait.Until(() => true).
                ShouldBeTrue();
        }

        [Test]
        public void always_going_to_be_false()
        {
            Wait.Until(() => false, timeoutInMilliseconds:1000)
                .ShouldBeFalse();
        }

        [Test]
        public void eventually_try()
        {
            int i = 0;

            Wait.Until(() => {
                i++;
                return i > 4;
            })
            .ShouldBeTrue();
        }
    }
}