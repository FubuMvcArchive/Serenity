using System;
using System.Collections.Generic;
using System.IO;
using FubuCore;
using FubuCore.CommandLine;

namespace Serenity.Jasmine
{
    public class JasmineConfigurator
    {
    	private readonly IFileSystem _fileSystem;
    	private readonly IJasmineConfigLoader _configLoader;

    	public JasmineConfigurator(IFileSystem fileSystem, IJasmineConfigLoader configLoader)
        {
    		_fileSystem = fileSystem;
    		_configLoader = configLoader;
        }

        public JasmineConfiguration Configure(string file, ISerenityJasmineApplication application)
        {
        	file = file.ToFullPath();

        	JasmineConfiguration config;
			if(_fileSystem.IsFile(file))
			{
				Console.WriteLine("Reading directives from " + file);
				if (!_fileSystem.FileExists(file))
				{
					throw new CommandFailureException("Designated serenity/jasmine file at {0} does not exist".ToFormat(file));
				}

				config = _configLoader.LoadFrom(file);
			}
			else
			{
				config = new JasmineConfiguration(file);
				config.AddContentFolder(file);
			}

			ProcessConfiguration(config, application);

        	return config;
        }

        public void ProcessConfiguration(JasmineConfiguration config, ISerenityJasmineApplication application)
        {
        	config
				.ContentFolders
				.Each(folder =>
				{
					Console.WriteLine("Adding content from folder " + folder);
					application.AddContentFolder(folder);
        	    });
        }
    }
}