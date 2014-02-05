using System;
using System.Diagnostics;
using System.Threading;
using FubuCore;
using FubuCore.Descriptions;
using FubuTestingSupport;
using HtmlTags;
using HtmlTags.Extended.Attributes;
using NUnit.Framework;
using OpenQA.Selenium;
using Serenity.Fixtures;
using Serenity.Fixtures.Handlers;
using StoryTeller;
using StoryTeller.Domain;
using StoryTeller.Engine;
using TestContext = StoryTeller.Engine.TestContext;

namespace Serenity.Testing.Fixtures.Handlers
{
    [TestFixture]
    public class TextboxElementHandlerTester
    {
        private IWebDriver theDriver;
        private IWebElement textbox1;
        private TextboxElementHandler theHandler = new TextboxElementHandler();

        private string file = "textboxfixture.htm";
        private string theText = "(100)444-9898";
        private string id = "textbox1";

        [SetUp]
        [TestFixtureSetUp]
        public void Setup()
        {
            var document = new HtmlDocument();

            document.Add(new HtmlTag("input", tag =>
            {
                tag.Value(theText);
                tag.Id(id);
            }));

            document.WriteToFile(file);

            try
            {
                StartDriver();
            }
            catch (Exception)
            {
                Thread.Sleep(2000);
                StartDriver();
            }

            textbox1 = theDriver.FindElement(By.Id(id));
        }

        public void StartDriver()
        {
            theDriver = BrowserForTesting.Driver;

            BrowserForTesting.Driver.Navigate().GoToUrl("file:///" + file.ToFullPath());
        }


        [Test]
        public void should_be_able_to_clear_original_text()
        {
            theHandler.EraseData(null, textbox1);
            theHandler.GetData(null, textbox1).ShouldBeEmpty();
        }

        [Test]
        public void should_be_able_to_get_text_from_field()
        {
            theHandler.GetData(null, textbox1).ShouldEqual(theText);
        }

        [Test]
        public void should_be_able_to_write_to_a_clean_field()
        {
            var input = "Hello There";
            theHandler.EnterData(null, textbox1, input);
            theHandler.GetData(null, textbox1).ShouldEqual(input);
        }
    }
}
