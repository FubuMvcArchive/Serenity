﻿using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuTestingSupport;
using HtmlTags;
using NUnit.Framework;
using OpenQA.Selenium;
using Serenity.Testing.Fixtures;
using Serenity.WebDriver;
using By = Serenity.WebDriver.By;

namespace Serenity.Testing.WebDriver
{
    public class ByJqueryPageTester : ScreenManipulationTester
    {
        private const string RootText = "Welcome to WebDriver jQuery selectors.";

        protected override void configureDocument(HtmlDocument document)
        {
            document.Add("h1").AddClass("root-marker").Text(RootText);

            document.Add(BuildTestDiv(new Stack<int>(new[] {0}), 0));

            document.ReferenceJavaScriptFile("file:///" + "jquery-2.0.3.min.js".ToFullPath());
        }

        [TestCase(".root-marker", Result = RootText)]
        [TestCase(".depth-level-2 > span", Result = "--Div at depth 2 index 0")]
        [TestCase(".depth-level-2:nth-of-type(2) > span", Result = "--Div at depth 2 index 1")]
        public string CanRetrieveElementBySelectorWithJQuery(string selector)
        {
            return theDriver.FindElement((By) By.jQuery(selector)).Text;
        }

        [Test, ExpectedException(typeof (NoSuchElementException))]
        public void NoElementFoundWithJQuery()
        {
            theDriver.FindElement((By) By.jQuery(".no-such-element"));
        }

        [Test]
        public void CanRetrieveElementsComplex()
        {
            By selector = By.jQuery(".depth-level-3-0-0-0").Parents(".depth-level").Children("span");
            var textForParents = theDriver.FindElements(selector).Select(x => x.Text).ToList();

            textForParents.ShouldHaveTheSameElementsAs(
                "--Div at depth 2 index 0",
                "-Div at depth 1 index 0",
                "Div at depth 0 index 0");
        }

        [Test]
        public void JQueryFindByText()
        {
            By selector = By.jQuery(".depth-level-2 > span")
                .Filter(JQuery.HasTextFilterFunction("--Div at depth 2 index 1"))
                .Parent()
                .Find(".depth-level-3 > span")
                .Filter(JQuery.HasTextFilterFunction("---Div at depth 3 index 1"));

            var textFromFoundElements = theDriver.FindElements(selector).Select(x => x.Text).ToList();

            textFromFoundElements.ShouldHaveTheSameElementsAs(
                "---Div at depth 3 index 1",
                "---Div at depth 3 index 1",
                "---Div at depth 3 index 1");
        }

        [Test]
        public void JQueryFindByDoesNotHaveText()
        {
            By selector = By.jQuery(".depth-level-1-0 .depth-level-2 > span")
                .Filter(JQuery.DoesNotHaveTextFilterFunction("--Div at depth 2 index 1"))
                .Parent()
                .Find(".depth-level-3 > span")
                .Filter(JQuery.DoesNotHaveTextFilterFunction("---Div at depth 3 index 1"));

            var textFromFoundElements = theDriver.FindElements(selector).Select(x => x.Text).ToList();

            textFromFoundElements.ShouldHaveTheSameElementsAs(
                "---Div at depth 3 index 0",
                "---Div at depth 3 index 2",
                "---Div at depth 3 index 0",
                "---Div at depth 3 index 2");
        }

        private static DivTag BuildTestDiv(Stack<int> indexes, int depth)
        {
            var div = new DivTag();
            div.AddClass("depth-level");
            var classToAdd = "depth-level-{0}".ToFormat(depth);
            div.AddClass(classToAdd);

            indexes.Each(x =>
            {
                classToAdd += "-" + x;
                div.AddClass(classToAdd);
            });

            var span = new HtmlTag("span");
            span.Text("{0}Div at depth {1} index {2}".ToFormat(new String('-', depth), depth, indexes.Peek()));

            div.Children.Add(span);

            if (depth < 3)
            {
                for (var i = 0; i < 3; i++)
                {
                    indexes.Push(i);
                    var childDiv = BuildTestDiv(indexes, depth + 1);
                    indexes.Pop();
                    div.Children.Add(childDiv);
                }
            }

            return div;
        }
    }

    public class ByJqueryWithNoJQueryOnPageTester : ScreenManipulationTester
    {
        private const string RootText = "Welcome to WebDriver jQuery selectors. (No JQuery Here)";

        protected override void configureDocument(HtmlDocument document)
        {
            document.Add("h1").AddClass("root-marker").Text(RootText);
        }

        [Test, ExpectedException(typeof (InvalidOperationException))]
        public void NoJQueryOnPageThrows()
        {
            theDriver.FindElement((By) By.jQuery(".root-marker"));
        }
    }
}