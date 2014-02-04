using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using FubuCore;
using OpenQA.Selenium;
using Serenity.WebDriver.JavaScriptBuilders;

namespace Serenity.WebDriver
{
    public class JavaScript : DynamicObject
    {
        public string Statement { get; private set; }

        private IList<IWebElement> Arguments { get; set; }

        protected static IList<IJavaScriptBuilder> JavaScriptBuilders { get; private set; }

        static JavaScript()
        {
            JavaScriptBuilders = new ReadOnlyCollection<IJavaScriptBuilder>(new IJavaScriptBuilder[]
            {
                new NullObjectJavaScriptBuilder(),
                new StringJavaScriptBuilder(),
                new WebElementJavaScriptBuilder(),
                new DefaultJavaScriptBuilder()
            });
        }

        public JavaScript(string statement) : this(statement, new IWebElement[0]) { }

        public JavaScript(string statement, params IWebElement[] arguments)
        {
            Statement = statement;
            Arguments = new List<IWebElement>(arguments);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var javascriptFriendlyName = JavaScriptFriendlyName(binder.Name);
            result = new JavaScript(AppendFunction(javascriptFriendlyName, args),
                Arguments.Union(args.Where(x => x is IWebElement).Cast<IWebElement>()).ToArray());
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var javascriptFriendlyName = JavaScriptFriendlyName(binder.Name);
            result = new JavaScript("{0}.{1}".ToFormat(Statement, javascriptFriendlyName));
            return true;
        }

        public object ExecuteAndGet(IWebDriver driver)
        {
            return ExecuteAndGet((IJavaScriptExecutor) driver);
        }

        public object ExecuteAndGet(IJavaScriptExecutor executor)
        {
            return executor.ExecuteScript("return {0};".ToFormat(StatementWithArguments(true)), Arguments.Cast<object>().ToArray());
        }

        public T ExecuteAndGet<T>(IWebDriver driver) where T : class
        {
            return ExecuteAndGet<T>((IJavaScriptExecutor) driver);
        }

        public T ExecuteAndGet<T>(IJavaScriptExecutor executor) where T : class
        {
            return ExecuteAndGet(executor) as T;
        }

        public void Execute(IWebDriver driver)
        {
            Execute((IJavaScriptExecutor) driver);
        }

        public void Execute(IJavaScriptExecutor executor)
        {
            executor.ExecuteScript(StatementWithArguments(false), Arguments.Cast<object>().ToArray());
        }

        private string StatementWithArguments(bool returnValue)
        {
            if (Arguments == null || Arguments.Count == 0)
            {
                return Statement;
            }

            var argumentVariables = Arguments.Select((element, index) => new
            {
                ParameterName = "__element__argument__{0}".ToFormat(index),
                ParameterRetrieval = "arguments[{0}]".ToFormat(index)
            }).ToList();

            var correctedStatement = argumentVariables
                .Select(x => x.ParameterName)
                .Aggregate(Statement, (statement, arg) => WebElementJavaScriptBuilder.MarkerRgx.Replace(statement, arg, 1));

            return "(function({0}) {{ {1}{2} }})({3})".ToFormat(
                argumentVariables.Select(x => x.ParameterName).Join(", "),
                returnValue ? "return " : "",
                correctedStatement,
                argumentVariables.Select(x => x.ParameterRetrieval).Join(", "));
        }

        public dynamic ModifyStatement(string format)
        {
            return new JavaScript(format.ToFormat(Statement));
        }

        public override string ToString()
        {
            return Statement;
        }

        private string JavaScriptFriendlyName(string name)
        {
            return char.ToLowerInvariant(name[0]) + name.Substring(1);
        }

        private string AppendFunction(string func, params object[] args)
        {
            var argsString = args == null
                ? ""
                : args
                    .Reverse()
                    .SkipWhile(arg => arg == null)
                    .Reverse()
                    .Select(arg => JavaScriptBuilders.First(x => x.Matches(arg)).Build(arg))
                    .Join(", ");

            return "{0}.{1}({2})".ToFormat(Statement, func, argsString);
        }

        public static dynamic Create(string javaScript)
        {
            return new JavaScript(javaScript);
        }

        public static dynamic CreateJQuery(string selector)
        {
            return new JavaScript("$(\"" + selector + "\")");
        }

        public static dynamic JQueryFrom(IWebElement element)
        {
            return new JavaScript("$({0})".ToFormat(WebElementJavaScriptBuilder.Marker), element);
        }

        public static dynamic Function(JavaScript body)
        {
            return Function(Enumerable.Empty<string>(), body);
        }

        public static dynamic Function(IEnumerable<string> args, JavaScript body)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            return new JavaScript("function({0}) {{ {1} }}".ToFormat(args.Join(", "), body.Statement));
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
}