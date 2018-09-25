namespace Take.Blip.Builder.Diagnostics
{
    public enum TraceMode
    {
        Disabled,
        Error,
        Slow,
        ErrorSlow,
        All
    }

    public static class TraceModeExtensions
    {
        public static bool IsError(this TraceMode traceMode)
        {
            return
                traceMode == TraceMode.Error ||
                traceMode == TraceMode.ErrorSlow;
        }

        public static bool IsSlow(this TraceMode traceMode)
        {
            return
                traceMode == TraceMode.Slow ||
                traceMode == TraceMode.ErrorSlow;
        }
    }
}
