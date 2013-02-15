using System.Net;
using FubuTestingSupport;
using KayakTestApplication;
using NUnit.Framework;

namespace Serenity.Testing
{
    [TestFixture]
    public class Katana_System_Integrated_Tester
    {
        private FubuMvcSystem<KayakApplication> theSystem;

        [SetUp]
        public void SetUp()
        {
            theSystem = new FubuMvcSystem<KayakApplication>();
        }

        [TearDown]
        public void TearDown()
        {
            theSystem.Dispose();
        }

        [Test]
        public void start_a_test_and_try_to_hit_the_endpoint()
        {
            var context = theSystem.CreateContext();

            var application = context.Services.GetInstance<IApplicationUnderTest>();

            // The below is enough to prove we're making a round trip
            var response = application.Endpoints().PostJson(new NameModel {Name = "Jeremy"});
            response.StatusCode.ShouldEqual(HttpStatusCode.OK);
            response.ReadAsJson<NameModel>().Name.ShouldEqual("Jeremy");
        }
    }

   
}