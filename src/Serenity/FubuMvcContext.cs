using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuCore.Binding;
using HtmlTags;
using StoryTeller.Engine;

namespace Serenity
{
	public class FubuMvcContext : IExecutionContext, IResultsExtension
	{
		private readonly IApplicationUnderTest _application;
		private readonly BindingRegistry _binding;
	    private readonly IEnumerable<IContextualInfoProvider> _contextualProviders;

	    public FubuMvcContext(IApplicationUnderTest application, BindingRegistry binding, IEnumerable<IContextualInfoProvider> contextualProviders )
		{
			_application = application;
			_binding = binding;

		    _contextualProviders = contextualProviders ?? new IContextualInfoProvider[0];

	        _contextualProviders.Each(x => x.Reset());
		}

		public void Dispose()
		{

		}

		public IServiceLocator Services
		{
			get { return _application.Services; }
		}

		public BindingRegistry BindingRegistry
		{
			get { return _binding; }
		}

	    public IEnumerable<HtmlTag> Tags()
	    {
	        return _contextualProviders.SelectMany(x => x.GenerateReports());
	    }
	}
}