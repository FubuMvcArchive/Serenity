using FubuCore;

namespace Serenity.WebDriver
{
    public static class JQuery
    {
        public static dynamic HasTextFilterFunction(string text)
        {
            return JavaScript.Function(JavaScript.Create("$(this)").Text().Trim().ModifyStatement("return {{0}} === '{0}';".ToFormat(text)));
        }

        public static dynamic DoesNotHaveTextFilterFunction(string text)
        {
            return JavaScript.Function(JavaScript.Create("$(this)").Text().Trim().ModifyStatement("return {{0}} !== '{0}';".ToFormat(text)));
        }

    }
}