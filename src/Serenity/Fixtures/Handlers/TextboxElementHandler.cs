using FubuCore;
using OpenQA.Selenium;

namespace Serenity.Fixtures.Handlers
{
    public class TextboxElementHandler : IElementHandler
    {
        public virtual bool Matches(IWebElement element)
        {
            // TODO --- Change to psuedo CSS class?  Less repeated logic
            return element.TagName.ToLower() == "input" && element.GetAttribute("type").ToLower() == "text";
        }

        public virtual void EraseData(ISearchContext context, IWebElement element)
        {
            if (element.GetAttribute("value").IsNotEmpty())
                element.SendKeys(Keys.Home + Keys.Shift + Keys.End + Keys.Delete);
        }

        public virtual void EnterData(ISearchContext context, IWebElement element, object data)
        {
            if (element.GetAttribute("value").IsNotEmpty())
                EraseData(context, element);

            element.SendKeys(data as string ?? string.Empty);
        }

        public virtual string GetData(ISearchContext context, IWebElement element)
        {
            return element.GetAttribute("value");
        }
    }
}
