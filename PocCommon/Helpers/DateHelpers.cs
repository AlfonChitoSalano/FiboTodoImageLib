namespace PocCommon.Helpers
{
    public static class DateHelpers
    {
        public static long GetCurrentEpochTime()
        {
            var currentTime = DateTimeOffset.UtcNow;
            var epochTime = currentTime.ToUnixTimeSeconds();
            return epochTime;
        }
    }
}