﻿using OSProj.TaskProcessor;
using System.Windows;

namespace OSProj.View
{
  public partial class MainWindow : Window
  {
    private readonly OSTaskProcessor _processor = new(ProcessorInfo.logger);
    private readonly ProcessorInfo _info;

    public MainWindow()
    {
      InitializeComponent();

      _info = new(_processor, PauseTask_btn, TaskProgressBar, Dispatcher);
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
          Stop_btn.IsEnabled = true;
          Start_btn.IsEnabled = false;
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
      Stop_btn.IsEnabled = false;
      Start_btn.IsEnabled = true;
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

    private void Activate_Task_Click(object sender, RoutedEventArgs e)
    {
      _processor.Activate();
    }

    private void Release_Task_Click(object sender, RoutedEventArgs e)
    {
      _processor.Release();
    }
  }
}