using FubuCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using StoryTeller;

namespace Serenity
{
    public class ChromeBrowser : BrowserLifecycle
    {
        public const string ChromeProcess = "chrome";
        public const string DriverProcess = "chromedriver";
        public const string File = "chromedriver.exe";


        protected override void preCleanUp()
        {
            cleanUp(null);
            Kill.Processes(ChromeProcess);
        }

        protected override IWebDriver buildDriver()
        {
            var fileSystem = new FileSystem();
            var settings = StoryTellerEnvironment.Get<SerenityEnvironment>();

            if (fileSystem.FileExists(settings.WorkingDir, File))
            {
                return new ChromeDriver(settings.WorkingDir);
            }

            return new ChromeDriver();
        }

        protected override void cleanUp(IWebDriver value)
        {
            Kill.Processes(DriverProcess, File);
        }
    }
}