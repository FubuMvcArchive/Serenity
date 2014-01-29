using HtmlTags;
using Serenity.Fixtures;
using Serenity.WebDriver;
using StoryTeller.Engine;

namespace Serenity.Testing.WebDriver
{
    // SAMPLE: simpleByJQuery
    public class ByJQueryExample : ScreenFixture
    {
        [FormatAs("Some grammar that clicks an element")]
        public void ClickButtonSimple()
        {
            // Generates JavaScript: return $(".button-class");
            Driver.FindElement((By) By.jQuery(".button-class")).Click();
        }

        [FormatAs("Some grammar that walks DOM to find element and then clicks it")]
        public void ClickButtonAdvanced()
        {
            // Generates JavaScript: return $(".some-label").filter(function() { return $(this).text().trim() === "Label Text"; }).parents(".parent-class").find(".button-class");
            Driver.FindElement((By) By.jQuery(".some-label")
                    .Filter(JQuery.HasTextFilterFunction("Label Text"))
                    .Parents(".parent-class")
                    .Find(".button-class"))
                .Click();
        }
    }
    // ENDSAMPLE
}