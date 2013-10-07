using System.Linq;
using FubuTestingSupport;
using NUnit.Framework;

namespace Serenity.Testing
{
    [TestFixture]
    public class FubuMvcSystemTester
    {
        [Test]
        public void sets_itself_as_current_when_it_is_created()
        {
            var system = new FubuMvcSystem(null, () => null);
            FubuMvcSystem.Current.ShouldBeTheSameAs(system);
        }

        [Test]
        public void register_a_remote_subsystem()
        {
            var system = new FubuMvcSystem(null, () => null);
            system.AddRemoteSubSystem("foo", x => { });

            system.RemoteSubSystemFor("foo").ShouldNotBeNull();
            system.SubSystems.Single().ShouldBeOfType<RemoteSubSystem>();
        }

    }
}