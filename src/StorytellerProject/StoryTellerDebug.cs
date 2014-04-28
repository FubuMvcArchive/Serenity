using System.Diagnostics;
using FubuCore;
using StoryTeller.Execution;
using StoryTeller.ProjectUtils.Loaders;
using StoryTeller.Workspace;
using FileSystem = FubuCore.FileSystem;

namespace StoryTellerTestHarness
{
    public class Template
    {

        public void Passing_Test()
        {
            var directory = ".".ToFullPath().ParentDirectory().ParentDirectory();
            Debug.WriteLine(directory);

            var project =
                new ProjectDirectoryLoader(new FileSystem()).Load(directory);
            using (var runner = new ProjectTestRunner(project))
            {
                runner.RunAndAssertTest("Screens/Passing Test");
            }
            
        }


    }
}