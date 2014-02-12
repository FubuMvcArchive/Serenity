using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;

namespace Serenity.Fixtures.Handlers
{
    public abstract class ElementHandlerWrapper : IElementHandler
    {
        public static Func<IEnumerable<IElementHandler>> AllHandlers { get; set; }

        static ElementHandlerWrapper()
        {
            ResetAllHandlers();
        }

        public static void ResetAllHandlers()
        {
            AllHandlers = () => ElementHandlers.AllHandlers;
        }

        private IEnumerable<IElementHandler> FollowOnHandlers
        {
            get { return AllHandlers().SkipWhile(x => !ReferenceEquals(this, x)).Skip(1); }
        }

        public bool Matches(IWebElement element)
        {
            return WrapperMatches(element) && FollowOnHandlers.Any(x => x.Matches(element));
        }

        protected abstract bool WrapperMatches(IWebElement element);

        public void EnterData(ISearchContext context, IWebElement element, object data)
        {
            EnterDataBeforeNested(context, element, data);
            FollowOnHandlers.First(x => x.Matches(element)).EnterData(context, element, data);
            EnterDataAfterNested(context, element, data);
        }

        protected virtual void EnterDataBeforeNested(ISearchContext context, IWebElement element, object data) { }

        protected virtual void EnterDataAfterNested(ISearchContext context, IWebElement element, object data) { }

        public string GetData(ISearchContext context, IWebElement element)
        {
            GetDataBeforeNested(context, element);
            var value = FollowOnHandlers.First(x => x.Matches(element)).GetData(context, element);
            GetDataAfterNested(context, element);
            return value;
        }

        protected virtual void GetDataBeforeNested(ISearchContext context, IWebElement element) { }

        protected virtual void GetDataAfterNested(ISearchContext context, IWebElement element) { }
    }
}