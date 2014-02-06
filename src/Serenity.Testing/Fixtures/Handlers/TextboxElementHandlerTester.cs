using FubuTestingSupport;
using HtmlTags;
using HtmlTags.Extended.Attributes;
using NUnit.Framework;
using OpenQA.Selenium;
using Serenity.Fixtures.Handlers;

namespace Serenity.Testing.Fixtures.Handlers
{
    public class TextboxElementHandlerTester : ScreenManipulationTester
    {
        private readonly TextboxElementHandler _handler = new TextboxElementHandler();

        private const string TheText = "(100)444-9898";
        private const string Id = "textbox1";
        private readonly By _byId = By.Id(Id);

        protected override void configureDocument(HtmlDocument document)
        {
            document.Add(new HtmlTag("input", tag =>
            {
                tag.Value(TheText);
                tag.Id(Id);
            }));
        }

        [Test]
        public void should_be_able_to_clear_original_text()
        {
            var textbox1 = theDriver.FindElement(_byId);
            _handler.EraseData(null, textbox1);
            _handler.GetData(null, textbox1).ShouldBeEmpty();
        }

        [Test]
        public void should_be_able_to_get_text_from_field()
        {
            var textbox1 = theDriver.FindElement(_byId);
            _handler.GetData(null, textbox1).ShouldEqual(TheText);
        }

        [Test]
        public void should_be_able_to_write_to_a_clean_field()
        {
            const string input = "Hello There";
            var textbox1 = theDriver.FindElement(_byId);
            _handler.EnterData(null, textbox1, input);
            _handler.GetData(null, textbox1).ShouldEqual(input);
        }
    }
}
