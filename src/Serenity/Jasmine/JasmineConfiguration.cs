using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;

namespace Serenity.Jasmine
{
	public class JasmineConfiguration
	{
		private readonly IList<string> _contentFolders = new List<string>();
		private readonly IList<string> _excludeFilters = new List<string>();

		private string _file;

		public JasmineConfiguration(string file)
		{
			_file = file;
		}

		public IEnumerable<string> ContentFolders { get { return _contentFolders; } }
		public IEnumerable<string> ExcludeFilters { get { return _excludeFilters; } }

		public void AddContentFolder(string folder)
		{
			if (!Path.IsPathRooted(folder))
			{
				folder = _file.ParentDirectory().AppendPath(folder);
			}
			_contentFolders.Fill(folder);
		}

		public void AddExclude(string filter)
		{
			if (!Path.IsPathRooted(filter) && !filter.StartsWith("."))
			{
				filter = _file.ParentDirectory().AppendPath(filter);
			}

			_excludeFilters.Fill(filter);
		}

		public bool ShouldExclude(string file)
		{
			file = file.ToFullPath();
			return ExcludeFilters.Any(x => file.Contains(x) || file.EndsWith(x));
		}
	}
}