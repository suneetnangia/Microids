namespace Microsoft.OneWeek.Hack.Microids.Common
{
    using System;

    public interface ITelemetryClient
    {

        void TrackEvent(string message);
        void TrackException(Exception ex);
        void TrackDependency(string a, string b, string c, DateTime start, TimeSpan elapsed, bool success);

    }

}