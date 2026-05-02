using System.Windows;

namespace DeskFlowAI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void RunCodeButton_Click(object sender, RoutedEventArgs e)
    {
        StatusTextBlock.Text = "Butona basildi. XAML, C# kodu ile konustu.";
    }
}
