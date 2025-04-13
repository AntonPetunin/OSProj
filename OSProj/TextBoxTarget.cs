using NLog;
using NLog.Targets;

namespace OSProj
{
  [Target("TextBoxTarget")]
  public sealed class TextBoxTarget : TargetWithLayout
  {
    public static Action<string>? LogAction { get; set; }

    protected override void Write(LogEventInfo logEvent)
    {
      string logMessage = Layout.Render(logEvent);
      LogAction?.Invoke(logMessage);
    }
  }
}
