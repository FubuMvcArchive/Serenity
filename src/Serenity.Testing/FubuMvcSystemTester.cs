using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuCore.Util;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using FubuTestingSupport;
using HtmlTags;
using NUnit.Framework;
using StoryTeller.Engine;
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

        [Test]
        public void works_with_the_contextual_providers()
        {
            using (var system = new FubuMvcSystem<TargetApplication>())
            {
                system.ModifyContainer(x => {
                    x.For<IContextualInfoProvider>().Add(new FakeContextualProvider("red", "green"));
                    x.For<IContextualInfoProvider>().Add(new FakeContextualProvider("blue", "orange"));
                });

                system.CreateContext().As<IResultsExtension>()
                    .Tags().Select(x => x.Text())
                    .ShouldHaveTheSameElementsAs("red", "green", "blue", "orange");
            }
        }
    }

    public class FakeContextualProvider : IContextualInfoProvider
    {
        private readonly string[] _colors;

        public FakeContextualProvider(params string[] colors)
        {
            _colors = colors;
        }

        public void Reset()
        {
            
        }

        public IEnumerable<HtmlTag> GenerateReports()
        {
            return _colors.Select(x => new HtmlTag("span").Text(x));
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