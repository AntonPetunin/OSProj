using OSProj.TaskProcessor;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OSProj
{

  public partial class MainWindow : Window
  {
    private OSTaskProcessor _processor = new();
    private ProcessorInfo _info;
    private delegate void ProcessorThreadRun();

    public MainWindow()
    {
      _info = new(_processor, Dispatcher);
      InitializeComponent();
    }

    private void Generate_Click(object sender, RoutedEventArgs e)
    {
      _processor.Generate();

    }

    private void Start_Click(object sender, RoutedEventArgs e)
    {
      if (!_processor.Running)
      {
        _processor.Start();
        Stop_btn.Visibility = Visibility.Visible;
      }
      else
        MessageBox.Show("Процесс уже запущен.");
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      TaskListView.ItemsSource = _info.Tasks;
    }

    private void Stop_btn_Click(object sender, RoutedEventArgs e)
    {
      _processor.Stop();
      Stop_btn.Visibility = Visibility.Hidden;
    }
  }
}