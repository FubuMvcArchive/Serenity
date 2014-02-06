using HtmlTags;
using NUnit.Framework;
using OpenQA.Selenium;
using Serenity.Fixtures.Handlers;
using FubuTestingSupport;

namespace Serenity.Testing.Fixtures.Handlers
{
    public class CheckboxHandlerTester : ScreenManipulationTester
    {
        private readonly CheckboxHandler _handler = new CheckboxHandler();

        private const string Disabled = "disabled";
        private const string CheckedAttr = "checked";
        private const string Enabled = "enabled";
        private const string NotChecked = "not-checked";

        private const string Target1 = "tartget1";
        private const string Target2 = "tartget2";
        private const string Target3 = "tartget3";
        private const string Target4 = "tartget4";
        private const string Div1 = "div1";
        private const string Text1 = "text1";

        private readonly By _disabledById = By.Id(Disabled);
        private readonly By _checkedById = By.Id(CheckedAttr);
        private readonly By _enabledById = By.Id(Enabled);
        private readonly By _notCheckedById = By.Id(NotChecked);
        private readonly By _div1ById = By.Id(Div1);
        private readonly By _text1ById = By.Id(Text1);
        
        private readonly By _target1ById = By.Id(Target1);
        private readonly By _target2ById = By.Id(Target2);
        private readonly By _target3ById = By.Id(Target3);
        private readonly By _target4ById = By.Id(Target4);

        protected override void configureDocument(HtmlDocument document)
        {
            document.Add(new CheckboxTag(true).Id(CheckedAttr));
            document.Add(new CheckboxTag(false).Id(NotChecked));
            document.Add(new CheckboxTag(false).Attr(Disabled, Disabled).Id(Disabled));
            document.Add(new CheckboxTag(true).Id(Enabled));
            document.Add(new CheckboxTag(false).Id(Target1));
            document.Add(new CheckboxTag(false).Id(Target2));
            document.Add(new CheckboxTag(true).Id(Target3));
            document.Add(new CheckboxTag(false).Id(Target4));
            document.Add(new DivTag(Div1));
            document.Add(new TextboxTag().Id(Text1));
        }

        [Test]
        public void matches_negative()
        {
            _handler.Matches(theDriver.FindElement(_text1ById)).ShouldBeFalse();
            _handler.Matches(theDriver.FindElement(_div1ById)).ShouldBeFalse();
        }

        [Test]
        public void matches_positive()
        {
            _handler.Matches(theDriver.FindElement(_checkedById)).ShouldBeTrue();
        }

        [Test]
        public void enabled_positive()
        {
            CheckboxHandler.IsEnabled(theDriver.FindElement(_enabledById))
                .ShouldBeTrue();
        }

        [Test]
        public void enabled_negative()
        {
            CheckboxHandler.IsEnabled(theDriver.FindElement(_disabledById))
                .ShouldBeFalse();
        }

        [Test]
        public void is_checked_negative()
        {
            CheckboxHandler.IsChecked(theDriver.FindElement(_notCheckedById))
                .ShouldBeFalse();
        }

        [Test]
        public void is_checked_positive()
        {
            CheckboxHandler.IsChecked(theDriver.FindElement(_checkedById))
                .ShouldBeTrue();
        }

        [Test]
        public void check_a_checkbox()
        {
            var target = theDriver.FindElement(_target1ById);
            CheckboxHandler.IsChecked(target).ShouldBeFalse();
            
            CheckboxHandler.Check(target);

            CheckboxHandler.IsChecked(target).ShouldBeTrue();
        }

        [Test]
        public void enter_data_true()
        {
            var target = theDriver.FindElement(_target2ById);

            CheckboxHandler.IsChecked(target).ShouldBeFalse();

            _handler.EnterData(null, target, true);
            CheckboxHandler.IsChecked(target).ShouldBeTrue();

            _handler.EnterData(null, target, true);
            CheckboxHandler.IsChecked(target).ShouldBeTrue();
        }

        [Test]
        public void enter_data_false()
        {
            var target = theDriver.FindElement(_target3ById);

            CheckboxHandler.IsChecked(target).ShouldBeTrue();

            _handler.EnterData(null, target, false);
            CheckboxHandler.IsChecked(target).ShouldBeFalse();

            _handler.EnterData(null, target, false);
            CheckboxHandler.IsChecked(target).ShouldBeFalse();
        }

        [Test]
        public void enter_data_true_string()
        {
            var target = theDriver.FindElement(_target2ById);

            CheckboxHandler.IsChecked(target).ShouldBeFalse();

            _handler.EnterData(null, target, "true");
            CheckboxHandler.IsChecked(target).ShouldBeTrue();

            _handler.EnterData(null, target, "True");
            CheckboxHandler.IsChecked(target).ShouldBeTrue();
        }

        [Test]
        public void enter_data_false_string()
        {
            var target = theDriver.FindElement(_target3ById);

            CheckboxHandler.IsChecked(target).ShouldBeTrue();

            _handler.EnterData(null, target, "false");
            CheckboxHandler.IsChecked(target).ShouldBeFalse();

            _handler.EnterData(null, target, "False");
            CheckboxHandler.IsChecked(target).ShouldBeFalse();
        }

        [Test]
        public void enter_data_empty_string()
        {
            var target = theDriver.FindElement(_target3ById);

            CheckboxHandler.IsChecked(target).ShouldBeTrue();

            _handler.EnterData(null, target, string.Empty);
            CheckboxHandler.IsChecked(target).ShouldBeFalse();
        }

        [Test]
        public void get_data()
        {
            _handler.GetData(theDriver, theDriver.FindElement(_checkedById)).ShouldEqual(true.ToString());
            _handler.GetData(theDriver, theDriver.FindElement(_notCheckedById)).ShouldEqual(false.ToString());
        }
    }
}