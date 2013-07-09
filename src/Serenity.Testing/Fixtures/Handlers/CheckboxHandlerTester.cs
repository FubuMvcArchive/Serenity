using System;
using System.Threading;
using FubuCore;
using HtmlTags;
using HtmlTags.Extended.Attributes;
using NUnit.Framework;
using OpenQA.Selenium;
using Serenity.Fixtures.Handlers;
using FubuTestingSupport;

namespace Serenity.Testing.Fixtures.Handlers
{
    [TestFixture]
    public class CheckboxHandlerTester
    {
        private IWebDriver theDriver;
        private CheckboxHandler theHandler = new CheckboxHandler();
        private IBrowserLifecycle _lifecycle;

        [SetUp]
        public void SetUp()
        {
            var document = new HtmlDocument();
            document.Add(new CheckboxTag(true).Id("checked"));
            document.Add(new CheckboxTag(false).Id("not-checked"));
            document.Add(new CheckboxTag(false).Attr("disabled", "disabled").Id("disabled"));
            document.Add(new CheckboxTag(true).Id("enabled"));
            document.Add(new CheckboxTag(false).Id("target1"));
            document.Add(new CheckboxTag(false).Id("target2"));
            document.Add(new CheckboxTag(true).Id("target3"));
            document.Add(new CheckboxTag(false).Id("target4"));
            document.Add(new DivTag("div1"));
            document.Add(new TextboxTag().Id("txt1"));

            document.WriteToFile("checkbox.htm");

            try
            {
                startDriver();
            }
            catch (Exception)
            {
                Thread.Sleep(2000);
                startDriver();
            }


        }

        private void startDriver()
        {
            _lifecycle = WebDriverSettings.GetBrowserLifecyle();
            theDriver = _lifecycle.Driver;
            theDriver.Navigate().GoToUrl("file:///" + "checkbox.htm".ToFullPath());
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            theDriver.Dispose();
        }

        [Test]
        public void matches_negative()
        {
            theHandler.Matches(theDriver.FindElement(By.Id("txt1"))).ShouldBeFalse();
            theHandler.Matches(theDriver.FindElement(By.Id("div1"))).ShouldBeFalse();
        }

        [Test]
        public void matches_positive()
        {
            theHandler.Matches(theDriver.FindElement(By.Id("checked"))).ShouldBeTrue();
        }

        [Test]
        public void enabled_positive()
        {
            CheckboxHandler.IsEnabled(theDriver.FindElement(By.Id("enabled")))
                .ShouldBeTrue();
        }

        [Test]
        public void enabled_negative()
        {
            CheckboxHandler.IsEnabled(theDriver.FindElement(By.Id("disabled")))
                .ShouldBeFalse();
        }

        [Test]
        public void is_checked_negative()
        {
            CheckboxHandler.IsChecked(theDriver.FindElement(By.Id("not-checked")))
                .ShouldBeFalse();
        }

        [Test]
        public void is_checked_positive()
        {
            CheckboxHandler.IsChecked(theDriver.FindElement(By.Id("checked")))
                .ShouldBeTrue();
        }

        [Test]
        public void check_a_checkbox()
        {
            var target = theDriver.FindElement(By.Id("target1"));
            CheckboxHandler.IsChecked(target).ShouldBeFalse();
            
            CheckboxHandler.Check(target);

            CheckboxHandler.IsChecked(target).ShouldBeTrue();
        }

        [Test]
        public void enter_data_true()
        {
            var target = theDriver.FindElement(By.Id("target2"));

            CheckboxHandler.IsChecked(target).ShouldBeFalse();

            theHandler.EnterData(null, target, true);
            CheckboxHandler.IsChecked(target).ShouldBeTrue();

            theHandler.EnterData(null, target, true);
            CheckboxHandler.IsChecked(target).ShouldBeTrue();
        }

        [Test]
        public void enter_data_false()
        {
            var target = theDriver.FindElement(By.Id("target3"));

            CheckboxHandler.IsChecked(target).ShouldBeTrue();

            theHandler.EnterData(null, target, false);
            CheckboxHandler.IsChecked(target).ShouldBeFalse();

            theHandler.EnterData(null, target, false);
            CheckboxHandler.IsChecked(target).ShouldBeFalse();
        }

        [Test]
        public void enter_data_true_string()
        {
            var target = theDriver.FindElement(By.Id("target2"));

            CheckboxHandler.IsChecked(target).ShouldBeFalse();

            theHandler.EnterData(null, target, "true");
            CheckboxHandler.IsChecked(target).ShouldBeTrue();

            theHandler.EnterData(null, target, "True");
            CheckboxHandler.IsChecked(target).ShouldBeTrue();
        }

        [Test]
        public void enter_data_false_string()
        {
            var target = theDriver.FindElement(By.Id("target3"));

            CheckboxHandler.IsChecked(target).ShouldBeTrue();

            theHandler.EnterData(null, target, "false");
            CheckboxHandler.IsChecked(target).ShouldBeFalse();

            theHandler.EnterData(null, target, "False");
            CheckboxHandler.IsChecked(target).ShouldBeFalse();
        }

        [Test]
        public void enter_data_empty_string()
        {
            var target = theDriver.FindElement(By.Id("target3"));

            CheckboxHandler.IsChecked(target).ShouldBeTrue();

            theHandler.EnterData(null, target, string.Empty);
            CheckboxHandler.IsChecked(target).ShouldBeFalse();
        }

        [Test]
        public void get_data()
        {
            theHandler.GetData(theDriver, theDriver.FindElement(By.Id("checked"))).ShouldEqual(true.ToString());
            theHandler.GetData(theDriver, theDriver.FindElement(By.Id("not-checked"))).ShouldEqual(false.ToString());
        }
    }
}