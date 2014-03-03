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

        [FormatAs("Can find elements on screen")]
        public bool CanFindElements()
        {
            try
            {
                Driver.FindElement(By.Name("foo"));
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        [FormatAs("Only succeed on/after test attempt {attempt}")]
        public bool OnlyPassOnAttempt(int attempt)
        {
            return Retrieve<ITestContext>().RetryAttemptNumber >= attempt;
        }
    }
}