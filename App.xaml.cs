using System.Windows;
using System.Windows.Threading;

namespace DeskFlowAI;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        base.OnStartup(e);
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Exception rootException = e.Exception;

        while (rootException.InnerException is not null)
        {
            rootException = rootException.InnerException;
        }

        MessageBox.Show(
            $"Unexpected UI error:\n{rootException.Message}",
            "DeskFlow AI",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        e.Handled = true;
    }
}
