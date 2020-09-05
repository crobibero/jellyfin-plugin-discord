namespace Jellyfin.Plugin.Discord.Configuration
{
    /// <summary>
    /// Discord mention types.
    /// </summary>
    public enum MentionType
    {
        /// <summary>
        /// Mention @everyone.
        /// </summary>
        Everyone = 2,

        /// <summary>
        /// Mention @here.
        /// </summary>
        Here = 1,

        /// <summary>
        /// Mention none.
        /// </summary>
        None = 0
    }
}