using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Serenity
{
    public static class Wait
    {
        public static bool Until(Func<bool> condition, int millisecondPolling = 500, int timeoutInMilliseconds = 5000)
        {
            if (condition()) return true;

            var clock = new Stopwatch();
            clock.Start();

            return Until(condition, clock, millisecondPolling, timeoutInMilliseconds);
        }

        public static bool Until(IEnumerable<Func<bool>> conditions, int millisecondPolling = 500, int timeoutInMilliseconds = 5000)
        {
            if (conditions == null)
                throw new ArgumentNullException("conditions");

            var clock = new Stopwatch();
            clock.Start();

            return conditions
                .Where(condition => !condition())
                .All(condition => Until(condition, clock, millisecondPolling, timeoutInMilliseconds));
        }

        private static bool Until(Func<bool> condition, Stopwatch clock, int millisecondPolling, int timeoutInMilliseconds)
        {
            while (clock.ElapsedMilliseconds < timeoutInMilliseconds)
            {
                Thread.Yield();
                Thread.Sleep(millisecondPolling);

                if (condition()) return true;
            }

            return false;
        }
    }
}