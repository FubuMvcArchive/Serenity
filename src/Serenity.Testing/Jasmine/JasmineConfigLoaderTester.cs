using System.Collections.Generic;
using System.IO;
using System.Text;
using FubuCore;
using FubuTestingSupport;
using NUnit.Framework;
using Rhino.Mocks;
using Serenity.Jasmine;

namespace Serenity.Testing.Jasmine
{
	public static class TestFileHelper
	{
		public static string RelativePath(params string[] paths)
		{
			var targets = new List<string> {"code", "something"};
			targets.AddRange(paths);

			return FileSystem.Combine(targets.ToArray()).ToFullPath();
		}
	}

	[TestFixture]
	public class loading_a_file_with_a_single_include
	{
		private IFileSystem theFileSystem;
		private string theFile;
		private string theFileContents;
		private JasmineConfigLoader theLoader;
		private JasmineConfiguration theConfiguration;

		[SetUp]
		public void SetUp()
		{
			theFileSystem = MockRepository.GenerateStub<IFileSystem>();
			theFile = TestFileHelper.RelativePath("serenity.txt");
			theFileContents = "include:MyProject.Web";

			theFileSystem.Stub(x => x.ReadStringFromFile(theFile.ToFullPath())).Return(theFileContents);

			theLoader = new JasmineConfigLoader(theFileSystem);
			theConfiguration = theLoader.LoadFrom(theFile);
		}

		[Test]
		public void contains_the_single_content_folder()
		{
			theConfiguration.ContentFolders.ShouldHaveTheSameElementsAs(TestFileHelper.RelativePath("MyProject.Web"));
		}
	}

	[TestFixture]
	public class loading_a_file_with_multiple_includes
	{
		private IFileSystem theFileSystem;
		private string theFile;
		private string theFileContents;
		private JasmineConfigLoader theLoader;
		private JasmineConfiguration theConfiguration;

		[SetUp]
		public void SetUp()
		{
			theFileSystem = MockRepository.GenerateStub<IFileSystem>();
			theFile = TestFileHelper.RelativePath("serenity.txt");

			theFileContents = new StringBuilder()
				.AppendLine("include:MyProject.Web")
				.AppendLine("include:MyProject.Web2")
				.ToString();

			theFileSystem.Stub(x => x.ReadStringFromFile(theFile.ToFullPath())).Return(theFileContents);

			theLoader = new JasmineConfigLoader(theFileSystem);
			theConfiguration = theLoader.LoadFrom(theFile);
		}

		[Test]
		public void contains_the_content_folders()
		{
			theConfiguration
				.ContentFolders
				.ShouldHaveTheSameElementsAs(TestFileHelper.RelativePath("MyProject.Web"), TestFileHelper.RelativePath("MyProject.Web2"));
		}
	}

	[TestFixture]
	public class loading_a_file_with_a_single_exclude
	{
		private IFileSystem theFileSystem;
		private string theFile;
		private string theFileContents;
		private JasmineConfigLoader theLoader;
		private JasmineConfiguration theConfiguration;

		[SetUp]
		public void SetUp()
		{
			theFileSystem = MockRepository.GenerateStub<IFileSystem>();
			theFile = TestFileHelper.RelativePath("serenity.txt");

			theFileContents = "exclude:MyProject.Web{0}.idea".ToFormat(Path.DirectorySeparatorChar);

			theFileSystem.Stub(x => x.ReadStringFromFile(theFile.ToFullPath())).Return(theFileContents);

			theLoader = new JasmineConfigLoader(theFileSystem);
			theConfiguration = theLoader.LoadFrom(theFile);
		}

		[Test]
		public void contains_the_exclude()
		{
			theConfiguration.ExcludeFilters.ShouldHaveTheSameElementsAs(TestFileHelper.RelativePath("MyProject.Web", ".idea"));
		}
	}

	[TestFixture]
	public class loading_a_file_with_multiple_excludes
	{
		private IFileSystem theFileSystem;
		private string theFile;
		private string theFileContents;
		private JasmineConfigLoader theLoader;
		private JasmineConfiguration theConfiguration;

		[SetUp]
		public void SetUp()
		{
			theFileSystem = MockRepository.GenerateStub<IFileSystem>();
			theFile = TestFileHelper.RelativePath("serenity.txt");

			theFileContents = new StringBuilder()
				.AppendLine("exclude:MyProject.Web{0}.idea".ToFormat(Path.DirectorySeparatorChar))
				.AppendLine("exclude:MyProject.Web{0}Web.config".ToFormat(Path.DirectorySeparatorChar))
				.ToString();

			theFileSystem.Stub(x => x.ReadStringFromFile(theFile.ToFullPath())).Return(theFileContents);

			theLoader = new JasmineConfigLoader(theFileSystem);
			theConfiguration = theLoader.LoadFrom(theFile);
		}

		[Test]
		public void contains_the_excludes()
		{
			theConfiguration
				.ExcludeFilters
				.ShouldHaveTheSameElementsAs(TestFileHelper.RelativePath("MyProject.Web", ".idea"), TestFileHelper.RelativePath("MyProject.Web", "Web.config"));
		}
	}

	[TestFixture]
	public class ExcludeFilterTests
	{
		private JasmineConfiguration theConfiguration;

		[SetUp]
		public void SetUp()
		{
			theConfiguration = new JasmineConfiguration(TestFileHelper.RelativePath("serenity.txt"));
		}

		[Test]
		public void basic_negative()
		{
			// starting with nothing
			theConfiguration.ShouldExclude(TestFileHelper.RelativePath("MyProject.Web", ".idea")).ShouldBeFalse();
		}

		[Test]
		public void excluding_a_directory()
		{
			theConfiguration.AddExclude("MyProject.Web{0}.idea".ToFormat(Path.DirectorySeparatorChar));
			theConfiguration.ShouldExclude(TestFileHelper.RelativePath("MyProject.Web", ".idea")).ShouldBeTrue();
		}

		[Test]
		public void excluding_a_file()
		{
			theConfiguration.AddExclude("MyProject.Web{0}Web.config".ToFormat(Path.DirectorySeparatorChar));
			theConfiguration.ShouldExclude(TestFileHelper.RelativePath("MyProject.Web", "Web.config")).ShouldBeTrue();
		}

		[Test]
		public void excluding_a_file_within_a_directory_that_is_ignored()
		{
			theConfiguration.AddExclude("MyProject.Web{0}.idea".ToFormat(Path.DirectorySeparatorChar));
			theConfiguration.ShouldExclude(TestFileHelper.RelativePath("MyProject.Web", ".idea", "something.txt")).ShouldBeTrue();
		}

		[Test]
		public void excluding_a_file_extension_pattern()
		{
			theConfiguration.AddExclude(".js___jb_bak___");
			theConfiguration.ShouldExclude(TestFileHelper.RelativePath("MyProject.Web", ".idea", "something.js___jb_bak___")).ShouldBeTrue();
		}
	}
}