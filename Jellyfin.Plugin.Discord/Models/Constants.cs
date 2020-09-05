namespace Jellyfin.Plugin.Discord.Models
{
    /// <summary>
    /// Constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Recheck interval ms.
        /// </summary>
        public const int RecheckIntervalMs = 10000;

        /// <summary>
        /// Max retries before fallback.
        /// </summary>
        public const int MaxRetriesBeforeFallback = 10;

        /// <summary>
        /// Message queue send interval.
        /// </summary>
        public const int MessageQueueSendInterval = 1000;
    }
}