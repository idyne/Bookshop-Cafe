using GameAnalyticsSDK;
using GameAnalyticsSDK.Events;

namespace FateGames
{
    public static class AnalyticsManager
    {
        public static void Initialize()
        {
            GameAnalytics.Initialize();
        }

        public static void ReportStartProgress()
        {
            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "Level_1");
        }

        public static void ReportFinishProgress(bool success)
        {
            GameAnalytics.NewProgressionEvent(success ? GAProgressionStatus.Complete : GAProgressionStatus.Fail, "Level_1");
        }
    }
}

