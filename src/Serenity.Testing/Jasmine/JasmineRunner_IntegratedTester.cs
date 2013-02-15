using System.Collections.Generic;
using FubuCore;
using FubuTestingSupport;
using FubuMVC.OwinHost;
using NUnit.Framework;
using Serenity.Jasmine;

namespace Serenity.Testing.Jasmine
{
	[TestFixture]
	public class JasmineRunner_IntegratedTester
	{
		private string theFolder;
		private JasmineInput theInput;
		private FileSystem theFileSystem;

		[SetUp]
		public void SetUp()
		{
			theFolder = "Jasmine/TestPackage1";

			theFileSystem = new FileSystem();

			var port = PortFinder.FindPort(5501);

			theInput = new JasmineInput
			{
				BrowserFlag = BrowserType.Firefox,
				SerenityFile = "jasmine-serenity.txt",
				Mode = JasmineMode.run,
				PortFlag = port
			};

			theFileSystem.AlterFlatFile(theInput.SerenityFile, list =>
			{
				list.Fill("include:" + theFolder);
			});

			theFileSystem.CreateDirectory(theFolder, "bin");
		}


		[Test]
		public void runs_the_specs()
		{
			var runner = new JasmineRunner(theInput);
			runner.RunAllSpecs().ShouldBeTrue();
		}
	}
}