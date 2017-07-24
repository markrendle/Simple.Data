using System.Diagnostics;

namespace Shitty.Data
{
    public static class SimpleDataTraceSources
    {
        private const string TraceSourceName = "Simple.Data";
        private const SourceLevels DefaultLevel = SourceLevels.Warning;

        // Verbose=1xxx Information=2xxx Warning=3xxx Error=4xxx Critical=5xxx
        internal const int DebugMessageId = 1000;
        internal const int SqlMessageId = 2000;
        internal const int GenericWarningMessageId = 3000;
        internal const int PerformanceWarningMessageId = 3001;
        internal const int ObsoleteWarningMessageId = 3002;
        internal const int GenericErrorMessageId = 4000;

        private static TraceSource _traceSource;

        public static TraceSource TraceSource
        {
            get { return _traceSource ?? (_traceSource = new TraceSource(TraceSourceName, DefaultLevel)); }
        }
    }
}