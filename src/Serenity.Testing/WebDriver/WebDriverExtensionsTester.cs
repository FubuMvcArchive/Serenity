using FubuTestingSupport;
using HtmlTags;
using NUnit.Framework;
using OpenQA.Selenium;
using Serenity.Fixtures;
using Serenity.Testing.Fixtures;

namespace Serenity.Testing.WebDriver
{
    public class WebDriverExtensionsTester : ScreenManipulationTester
    {
        private const string EmptyTextboxId = "emptytextboxid";
        private const string EmptyTextboxName = "emptytextboxname";

        protected override void configureDocument(HtmlDocument document)
        {
            var textbox = new TextboxTag(EmptyTextboxName, string.Empty)
                .Id(EmptyTextboxId)
                .Data("value", "some value");

            document.Add(textbox);
        }

        [Test]
        public void HasAttributeReturnsTrue()
        {
            theDriver.FindElement(By.Id(EmptyTextboxId)).HasAttribute("data-value").ShouldBeTrue();
        }

        [Test]
        public void HasAttributeReturnsFalse()
        {
            theDriver.FindElement(By.Id(EmptyTextboxId)).HasAttribute("data-is-not-there").ShouldBeFalse();
        }
    }
}