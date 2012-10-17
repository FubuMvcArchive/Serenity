using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles;
using Bottles.Diagnostics;
using FubuCore;
using FubuMVC.Coffee;
using FubuMVC.Core;
using FubuMVC.Core.Packaging;
using FubuMVC.StructureMap;
using StructureMap;

namespace Serenity.Jasmine
{
    public interface ISerenityJasmineApplication
    {
        void AddContentFolder(string contentFolder);
    }

    public class SerenityJasmineApplication : IApplicationSource, IPackageLoader, ISerenityJasmineApplication
    {
        private readonly IList<string> _contentFolders = new List<string>();

        public FubuApplication BuildApplication()
        {
            return FubuApplication
                .For<SerenityJasmineRegistry>()
                .StructureMap(new Container())
                .Packages(x => {
                    x.Loader(this);
                    x.Assembly(GetType().Assembly);

                });
        }

        public string Name
        {
            get { return "Serenity Jasmine Runner"; }
        }

        public IEnumerable<IPackageInfo> Load(IPackageLog log)
        {
            var packages = new List<IPackageInfo>();

            foreach (var folder in _contentFolders)
            {
                log.Trace("Loading content package from " + folder);
                var pak = new ContentOnlyPackageInfo(folder, Path.GetFileName(folder));
                packages.Add(pak);

                packages.AddRange(ContentOnlyPackageInfo.FromAssemblies(folder));
            }

            return packages;
        }

        public void AddContentFolder(string contentFolder)
        {
            _contentFolders.Add(contentFolder);
        }
    }
}