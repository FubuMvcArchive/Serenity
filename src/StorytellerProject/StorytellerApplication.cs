using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FubuMVC.Core;
using FubuMVC.StructureMap;
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
}
