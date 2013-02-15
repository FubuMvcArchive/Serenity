using FubuCore;
using FubuCore.Binding;
using StoryTeller.Engine;

namespace Serenity
{
	public class FubuMvcContext : IExecutionContext
	{
		private readonly IApplicationUnderTest _application;
		private readonly BindingRegistry _binding;

		public FubuMvcContext(IApplicationUnderTest application, BindingRegistry binding)
		{
			_application = application;
			_binding = binding;
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
	}
}