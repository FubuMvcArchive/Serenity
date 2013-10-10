using System.Linq;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using NUnit.Framework;
using StructureMap;

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


        [Test]
        public void modify_the_underlying_container()
        {
            using (var system = new FubuMvcSystem<TargetApplication>())
            {
                system.ModifyContainer(x => x.For<IColor>().Use<Green>());

                system.CreateContext().Services.GetInstance<IColor>()
                    .ShouldBeOfType<Green>();
            }
        }
    }

    public class TargetApplication : IApplicationSource
    {
        public FubuApplication BuildApplication()
        {
            var container = new Container(x => {
                x.For<IColor>().Use<Red>();
            });

            return FubuApplication.DefaultPolicies().StructureMap(container);
        }
    }

    public interface IColor{}
    public class Red : IColor{}
    public class Green : IColor{}
}