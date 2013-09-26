using System;
using System.ComponentModel;
using FubuCore;
using FubuCore.CommandLine;
using StoryTeller;
using StoryTeller.Execution;
using StoryTeller.Workspace;

namespace Serenity.StoryTeller
{
	public class StoryTellerInput
	{
		public StoryTellerInput()
		{
			BrowserFlag = BrowserType.Firefox;
		}

		[Description("Path to the StoryTeller project file")]
		public string ProjectFile { get; set; }

		[Description("Path to write out the results")]
		public string ResultsFile { get; set; }

		[Description("Optional.  Runs only one workspace")]
		public string WorkspaceFlag { get; set; }

		[Description("Choose which browser to use for the testing")]
		public BrowserType BrowserFlag { get; set; }
	}

	[CommandDescription("Run a suite of StoryTeller tests")]
	public class StoryTellerCommand : FubuCommand<StoryTellerInput>
	{
		public override bool Execute(StoryTellerInput input)
		{
			StoryTellerEnvironment.Set(new SerenityEnvironment
			{
				Browser = input.BrowserFlag,
				WorkingDir = AppDomain.CurrentDomain.BaseDirectory
			});

			Console.WriteLine("Using browser " + input.BrowserFlag);

            var runner = new ProjectRunner(new[] { Project.LoadFromFile(input.ProjectFile) }, input.ResultsFile);
			if (input.WorkspaceFlag.IsNotEmpty())
			{
				Console.WriteLine("Using workspace " + input.WorkspaceFlag);
				runner.Workspace = input.WorkspaceFlag;
			}

			return runner.Execute() == 0;
		}
	}
}