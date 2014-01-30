using FubuCore;

namespace Serenity.WebDriver.JavaScriptBuilders
{
    public class JavaScriptBuilder : IJavaScriptBuilder
    {
        public bool Matches(object obj)
        {
            return obj is JavaScript;
        }

        public string Build(object obj)
        {
            return obj.As<JavaScript>().Statement;
        }
    }
}