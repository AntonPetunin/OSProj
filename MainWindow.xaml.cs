using OSProj.TaskProcessor;
using System.Windows;
using NLog;
using NLog.Config;

namespace OSProj
{
  public partial class MainWindow : Window
  {
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private OSTaskProcessor _processor = new(logger);
    private ProcessorInfo _info;

    public MainWindow()
    {
      _info = new(_processor, Dispatcher);
      InitializeComponent();
      ConfigureLogging();
      TextBoxTarget.LogAction = AppendLogToTextBox;
    }

    private void Generate_Click(object sender, RoutedEventArgs e)
    {
      _processor.Generate();

    }

    private void Start_Click(object sender, RoutedEventArgs e)
    {
      lock (_processor)
      {
        if (!_processor.Running)
        {
          _processor.Start();
          Stop_btn.Visibility = Visibility.Visible;
        }
        else
          MessageBox.Show("Процесс уже запущен.");
      }
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


    private void ConfigureLogging()
    {
      var config = new LoggingConfiguration();
      var textBoxTarget = new TextBoxTarget();
      textBoxTarget.Layout = "${longdate} ${level:uppercase=true} ${message} ${exception:format=toString}\n";
      config.AddRule(LogLevel.Info, LogLevel.Fatal, textBoxTarget);
      LogManager.Configuration = config;
    }

    private void AppendLogToTextBox(string text)
    {
      Dispatcher.Invoke(() =>
      {
        LogTextBox.AppendText(text);
        LogTextBox.ScrollToEnd();
      });
    }

    private void PauseTask_btn_Click(object sender, RoutedEventArgs e)
    {
      _processor.PauseActiveTask();
    }

    private void TerminateTask_btn_Click(object sender, RoutedEventArgs e)
    {
      _processor.TerminateActiveTask();
    }
  }
}