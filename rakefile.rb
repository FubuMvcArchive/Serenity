begin
  require 'bundler/setup'
  require 'fuburake'
rescue LoadError
  puts 'Bundler and all the gems need to be installed prior to running this rake script. Installing...'
  system("gem install bundler --source http://rubygems.org")
  sh 'bundle install'
  system("bundle exec rake", *ARGV)
  exit 0
end


@solution = FubuRake::Solution.new do |sln|
  sln.compile = {
    :solutionfile => 'src/Serenity.sln'
  }

  sln.assembly_info = {
    :product_name => "Serenity",
    :copyright => "Copyright 2011-#{Time.now.year} Jeremy D. Miller, Josh Arnold, et al. All rights reserved."
  }

  sln.ripple_enabled = true
  sln.fubudocs_enabled = true

  sln.defaults = %w{st:chrome:run st:phantom:run}
  
	sln.options[:nuget_publish_folder] = 'nupkgs'
	sln.options[:nuget_publish_url] = 'https://www.myget.org/F/fubumvc-edge/'
end

FubuRake::Storyteller.new({
  :path => 'src/StorytellerProject',
  :compilemode => @solution.compilemode
})

FubuRake::Storyteller.new({
  :path => 'src/StorytellerProject',
  :prefix => 'st:phantom',
  :profile => 'phantom',
  :compilemode => @solution.compilemode
})

FubuRake::Storyteller.new({
  :path => 'src/StorytellerProject',
  :prefix => 'st:chrome',
  :profile => 'chrome',
  :compilemode => @solution.compilemode
})

desc "Target used for the CI server (Mono)"
task :mono_ci => [:compile]



