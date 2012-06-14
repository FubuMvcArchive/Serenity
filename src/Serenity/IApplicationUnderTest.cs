using System;
using System.Collections.Generic;
using FubuMVC.Core.Endpoints;
using FubuMVC.Core.Urls;
using OpenQA.Selenium;

namespace Serenity
{
    public interface IApplicationUnderTest
    {
        string Name { get; }
        IUrlRegistry Urls { get; }
        IWebDriver Driver { get; }

        string RootUrl { get; }

        T GetInstance<T>();
        object GetInstance(Type type);
        IEnumerable<T> GetAll<T>();

        IBrowserLifecycle Browser { get; }

        void Ping();

        void Teardown();

        NavigationDriver Navigation { get;}
        EndpointDriver Endpoints();

    }
}