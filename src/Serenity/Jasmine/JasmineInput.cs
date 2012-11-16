using System;
using System.Collections.Generic;
using System.ComponentModel;
using FubuCore.CommandLine;
using OpenQA.Selenium;


namespace Serenity.Jasmine
{
    public enum JasmineMode
    {
        interactive,
        run,
        add_folders
        
    }

    public class JasmineInput
    {
        public JasmineInput()
        {
            PortFlag = 5500;
            BrowserFlag = BrowserType.Chrome;
        	TimeoutFlag = 5;
        }

        [Description("Chooses whether to open the browser application or just run all the specs in CLI mode")]
        public JasmineMode Mode { get; set; }

		[Description("Display verbose output of spec results for CI purposes")]
		[FlagAlias("verbose", 'v')]
		public bool VerboseFlag { get; set; }

        [Description("Name of the application folder with Jasmine specs or a file containing directives for where the specifications are located")]
        public string SerenityFile { get; set; }

        [Description("Optionally overrides which port number Kayak uses for the web application.  Default is 5500")]
        public int PortFlag { get; set; }

		[Description("Optionally overrides the amount of time (in seconds) to wait for the specs to run.")]
		public int TimeoutFlag { get; set; }

        [Description("Choose which browser to use for the testing")]
        public BrowserType BrowserFlag { get; set; }

        [Description("Adds folders to a Jasmine project in the add_folders.  \nFolders can be either absolute paths or relative to the jasmine text file")]
        public IEnumerable<string> Folders { get; set; }

        public IBrowserLifecycle GetBrowser()
        {
            return WebDriverSettings.GetBrowserLifecyle(BrowserFlag);
        }
    }
}