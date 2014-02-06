using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FubuCore;
using FubuTestingSupport;
using HtmlTags;
using HtmlTags.Extended.Attributes;
using NUnit.Framework;
using OpenQA.Selenium;
using Serenity.Fixtures.Handlers;

namespace Serenity.Testing.Fixtures.Handlers
{
    [TestFixture]
    public class TextAreaElementHandlerTester : ScreenManipulationTester
    {
        private readonly TextAreaElementHandler _handler = new TextAreaElementHandler();

        private const string TheText = "Test data within the textarea";
        private const string Id = "textbox1";
        private readonly By _byId = By.Id(Id);

        protected override void configureDocument(HtmlDocument document)
        {
            document.Add(new HtmlTag("textarea", tag =>
            {
                tag.Id(Id);
                tag.Text(TheText);
            }));
        }

        [Test]
        public void should_be_able_to_read_write()
        {
            var textarea1 = theDriver.FindElement(_byId);
            _handler.EnterData(null, textarea1, "New Data");
            _handler.GetData(null, textarea1).ShouldEqual("New Data");
        }

        [Test]
        public void clearing_data_should_return_clear_and_not_original_text()
        {
            var textarea1 = theDriver.FindElement(_byId);
            _handler.EnterData(null, textarea1, "");
            _handler.GetData(null, textarea1).ShouldEqual("");
        }
    }
}
