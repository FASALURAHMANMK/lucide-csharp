using System;
using System.Threading;

namespace Lucide.NET.Wpf.Tests;

internal static class StaTestRunner
{
    public static void Run(Action action)
    {
        Exception? captured = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                captured = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (captured is not null)
        {
            throw new InvalidOperationException("STA test failed.", captured);
        }
    }
}
