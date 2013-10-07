using OpenQA.Selenium;
using Serenity.Fixtures;
using StoryTeller.Engine;

namespace StorytellerProject.Fixtures
{
    public class HomeScreenFixture : ScreenFixture
    {
        public HomeScreenFixture()
        {
            Title = "Home Screen";
        }

        public void GoToHome()
        {
            Navigation.NavigateToHome();
        }

        [FormatAs("The textbox value is {returnValue}")]
        public string TheTextboxValueIs()
        {
            waitForElement(By.Name("foo"));
            return Driver.FindElement(By.Name("foo")).GetAttribute("value");
        }
    }
}