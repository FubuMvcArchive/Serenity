using System.Collections.Generic;
using FubuTestingSupport;
using NUnit.Framework;
using Serenity.WebDriver;
using Serenity.WebDriver.JavaScriptBuilders;

namespace Serenity.Testing.WebDriver.JavaScriptBuilders
{
    public class JavaScriptBuilderTester : InteractionContext<JavaScriptBuilder>
    {
        [TestCaseSource("MatchTestCases")]
        public bool Matches(object obj)
        {
            return ClassUnderTest.Matches(obj);
        }

        public IEnumerable<TestCaseData> MatchTestCases()
        {
            yield return new TestCaseData(null).Returns(false);
            yield return new TestCaseData((JavaScript) null).Returns(false);
            yield return new TestCaseData(new object()).Returns(false);
            yield return new TestCaseData(new JavaScript("some javascript")).Returns(true);
            yield return new TestCaseData(By.jQuery("some-selector")).Returns(true);
            yield return new TestCaseData(JQuery.HasTextFilterFunction("blah")).Returns(true);
            yield return new TestCaseData(JQuery.DoesNotHaveTextFilterFunction("blah")).Returns(true);
        }

        [Test]
        public void ReturnsRawJavaScript()
        {
            const string expectedScript = "$('.test').find(\".something\")";

            var script = JavaScript.Create("$('.test')").Find(".something");

            string actualScript = ClassUnderTest.Build(script);
            actualScript.ShouldEqual(expectedScript);
        }
    }
}