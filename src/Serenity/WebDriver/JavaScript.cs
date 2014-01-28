using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using FubuCore;
using OpenQA.Selenium;

namespace Serenity.WebDriver
{
    public class JavaScript : DynamicObject
    {
        public string Statement { get; private set; }

        public JavaScript(string statement)
        {
            Statement = statement;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var javascriptFriendlyName = char.ToLowerInvariant(binder.Name[0]) + binder.Name.Substring(1);
            result = new JavaScript(AppendFunction(javascriptFriendlyName, args));
            return true;
        }

        public object ExecuteAndGet(IJavaScriptExecutor executor)
        {
            return executor.ExecuteScript("return {0};".ToFormat(Statement));
        }

        public T ExecuteAndGet<T>(IJavaScriptExecutor executor) where T : class
        {
            return (T) ExecuteAndGet(executor);
        }

        public void Execute(IJavaScriptExecutor executor)
        {
            executor.ExecuteScript(Statement);
        }

        private string AppendFunction(string func, params object[] args)
        {
            var argsString = args == null
                ? ""
                : args
                    .Reverse()
                    .SkipWhile(p => p == null)
                    .Reverse()
                    .Select(p =>
                    {
                        if (p == null)
                        {
                            return "null";
                        }

                        if (p is string)
                        {
                            var pString = p as string;
                            return "\"{0}\"".ToFormat(pString);
                        }

                        return p.ToString();
                    })
                    .Join(", ");

            return "{0}.{1}({2})".ToFormat(Statement, func, argsString);
        }

        public static dynamic CreateJQuery(string selector)
        {
            return new JavaScript("$(\"" + selector + "\")");
        }

        public static implicit operator By(JavaScript source)
        {
            return (JavaScriptBy) source;
        }

        public static implicit operator OpenQA.Selenium.By(JavaScript source)
        {
            return (JavaScriptBy) source;
        }

        public static implicit operator JavaScriptBy(JavaScript source)
        {
            return new JavaScriptBy(source);
        }

        public static implicit operator JavaScript(JavaScriptBy source)
        {
            return (JavaScript) source.JavaScript;
        }
    }

    public static class JavaScriptExtensionMethods
    {
        public static void Execute(this IWebDriver driver, JavaScript script)
        {
            script.Execute((IJavaScriptExecutor) driver);
        }

        public static object ExecuteAndGet(this IWebDriver driver, JavaScript script)
        {
            return script.ExecuteAndGet((IJavaScriptExecutor) driver);
        }

        public static T ExecuteAndGet<T>(this IWebDriver driver, JavaScript script) where T : class
        {
            return script.ExecuteAndGet<T>((IJavaScriptExecutor) driver);
        }
    }
}