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
    public class TextAreaElementHandlerTester
    {
        private IWebDriver theDriver;
        private IWebElement textarea1;
        private TextAreaElementHandler theHandler = new TextAreaElementHandler();
        private string theText = "Test data within the textarea";

        [TestFixtureSetUp]
        public void SetUp()
        {
            var document = new HtmlDocument();
            document.Add(new HtmlTag("textarea", tag =>
                                                     {
                                                         tag.Id("textarea1");
                                                         tag.Text(theText);
                                                     })
                );

    
            

            document.WriteToFile("textarea.htm");

            try
            {
                startDriver();
            }
            catch (Exception)
            {
                Thread.Sleep(2000);
                startDriver();
            }

            textarea1 = theDriver.FindElement(By.Id("textarea1"));
        }

        private void startDriver()
        {
            theDriver = BrowserForTesting.Driver;
            theDriver.Navigate().GoToUrl("file:///" + "textarea.htm".ToFullPath());
        }

        [Test]
        public void should_be_able_to_read_write()
        {
            theHandler.EnterData(null, textarea1, "New Data");
            theHandler.GetData(null, textarea1).ShouldEqual("New Data");
        }

        [Test]
        public void clearing_data_should_return_clear_and_not_original_text()
        {
            theHandler.EnterData(null, textarea1, "");
            theHandler.GetData(null, textarea1).ShouldEqual("");
        }
    }
}
