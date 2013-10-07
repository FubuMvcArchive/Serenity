using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FubuMVC.Core;
using FubuMVC.StructureMap;
using HtmlTags;
using Serenity;

namespace StorytellerProject
{
    public class StorytellerApplication : IApplicationSource
    {
        public FubuApplication BuildApplication()
        {
            return FubuApplication.DefaultPolicies().StructureMap();
        }
    }

    public class SerenitySystem : FubuMvcSystem<StorytellerApplication>
    {
        
    }

    public class HomeEndpoint
    {
        public HtmlDocument Index()
        {
            var document = new HtmlDocument();
            document.Title = "Serenity/Storyteller Harness";
            document.Add("h1").Text("Serenity/Storyteller Harness");

            document.Add("input").Attr("name", "foo").Id("foo").Attr("value", "bar");

            return document;
        }
    }
}
