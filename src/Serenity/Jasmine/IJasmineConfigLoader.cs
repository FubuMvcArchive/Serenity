using System;
using System.IO;
using System.Linq;
using FubuCore;

namespace Serenity.Jasmine
{
	public interface IJasmineConfigLoader
	{
		JasmineConfiguration LoadFrom(string path);
	}

	public class JasmineConfigLoader : IJasmineConfigLoader
	{
		private readonly IFileSystem _fileSystem;

		public JasmineConfigLoader(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public JasmineConfiguration LoadFrom(string path)
		{
			var file = path.ToFullPath();
			var contents = _fileSystem.ReadStringFromFile(file);
			var config = new JasmineConfiguration(file);

			using(var reader = new StringReader(contents))
			{
				string line;
				while((line = reader.ReadLine()) != null)
				{
					if (line.IsEmpty()) continue;
					
					if (line.StartsWith("include:")) 
						include(line, config);
					
					else if (line.StartsWith("exclude:")) 
						exclude(line, config);
				}
			}

			return config;
		}

		private static void include(string text, JasmineConfiguration config)
		{
			var folder = text.Split(':').Last();
			config.AddContentFolder(folder);
		}

		private static void exclude(string text, JasmineConfiguration config)
		{
			var filter = text.Split(':').Last();
			Console.WriteLine("Adding exclude filter " + filter);
			config.AddExclude(filter);
		}
	}
}