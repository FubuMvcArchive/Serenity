using System;
using System.Collections.Generic;
using System.IO;
using Bottles;
using FubuCore;
using FubuMVC.Core.Assets.Caching;

namespace Serenity.Jasmine
{
	public class AssetFileWatcher
	{
		private readonly IAssetContentCache _cache;
		private readonly IFileSystem _fileSystem = new FileSystem();
		private readonly IList<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();

		public AssetFileWatcher(IAssetContentCache cache)
		{
			_cache = cache;
		}

		public void StartWatching(ISpecFileListener listener, JasmineConfiguration configuration)
		{
			PackageRegistry.Packages.Each(pak => pak.ForFolder(BottleFiles.WebContentFolder, dir =>
			{
			    var contentFolder = dir.AppendPath("content");
			    if (_fileSystem.DirectoryExists(contentFolder))
			    {
			        addContentFolder(contentFolder, listener, configuration);
			    }

			    var watcher = new FileSystemWatcher(dir, "*.config");
			    watcher.Changed += (x, y) => listener.Recycle();
			    watcher.Deleted += (x, y) => listener.Recycle();
			    watcher.EnableRaisingEvents = true;
			    watcher.IncludeSubdirectories = true;

			    _watchers.Add(watcher);
			}));
		}

		private void addContentFolder(string dir, ISpecFileListener listener, JasmineConfiguration configuration)
		{
			var watcher = new FileSystemWatcher(dir);
			watcher.Changed += (x, file) =>
			{
				if (configuration.ShouldExclude(file.FullPath)) return;
			    Console.WriteLine("Detected a change to " + file.FullPath);

			    _cache.FlushAll();
			    listener.Changed();
			};

			watcher.Created += (x, y) =>
			{
				if (configuration.ShouldExclude(y.FullPath)) return;
			    Console.WriteLine("Detected a new file at " + y.FullPath);
			    listener.Added();
			};

			watcher.Deleted += (x, y) =>
			{
				if (configuration.ShouldExclude(y.FullPath)) return;
			    Console.WriteLine("Detected a file deletion at " + y.FullPath);
			    listener.Deleted();
			};

			watcher.EnableRaisingEvents = true;
			watcher.IncludeSubdirectories = true;
		}

		public void StopWatching()
		{
			_watchers.Each(x => x.SafeDispose());
		}
	}
}