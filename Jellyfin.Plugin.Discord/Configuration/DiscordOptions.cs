namespace Jellyfin.Plugin.Discord.Configuration
{
    /// <summary>
    /// Discord options.
    /// </summary>
    public class DiscordOptions
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether server name override.
        /// </summary>
        public bool ServerNameOverride { get; set; }

        /// <summary>
        /// Gets or sets the server url.
        /// </summary>
        public string ServerUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether media added override.
        /// </summary>
        public bool MediaAddedOverride { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether exclude external server links.
        /// </summary>
        public bool ExcludeExternalServerLinks { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enable movies.
        /// </summary>
        public bool EnableMovies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enable episodes.
        /// </summary>
        public bool EnableEpisodes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enable series.
        /// </summary>
        public bool EnableSeries { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enable seasons.
        /// </summary>
        public bool EnableSeasons { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enable albums.
        /// </summary>
        public bool EnableAlbums { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether enable songs.
        /// </summary>
        public bool EnableSongs { get; set; }

        /// <summary>
        /// Gets or sets embed color.
        /// </summary>
        public string EmbedColor { get; set; }

        /// <summary>
        /// Gets or sets avatar url.
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets discord webhook uri.
        /// </summary>
        public string DiscordWebhookUri { get; set; }

        /// <summary>
        /// Gets or sets mention type.
        /// </summary>
        public MentionType MentionType { get; set; }
    }
}