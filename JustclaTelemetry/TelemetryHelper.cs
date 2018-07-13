using Microsoft.Internal.VisualStudio.Shell;
using Microsoft.Internal.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

namespace Justcla
{
    /// <summary>
    /// Code originally borrowed from TelemetryForPPT
    /// </summary>
    public class VSTelemetryHelper
    {
        public static void PostEvent(string key, params object[] namesAndProperties)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            ThreadHelper.ThrowIfNotOnUIThread();

            IVsTelemetryEvent telemetryEvent = TelemetryHelper.TelemetryService.CreateEvent(key);

            //telemetryEvent.SetStringProperty("Vs.ReportingAssembly", callingAssemblyName);
            for (int i = 0; i < namesAndProperties.Length; i += 2)
            {
                string propertyName = namesAndProperties[i] as string;
                if (!string.IsNullOrEmpty(propertyName))
                {
                    telemetryEvent.SetProperty(propertyName, namesAndProperties[i + 1]);
                }
            }

            TelemetryHelper.DefaultTelemetrySession.PostEvent(telemetryEvent);
        }
    }
}
