using System;
using OpenQA.Selenium;

namespace Serenity
{
    public interface IBrowserLifecycle : IDisposable
    {
        void UseInitializer(IBrowserSessionInitializer initializer);

        IWebDriver Driver { get; }
        void Recycle();
        bool HasBeenStarted();
    }
}