using System.Collections.Generic;

namespace Jellyfin.Plugin.Discord.Models
{
    /// <summary>
    /// Discord message.
    /// </summary>
    public class DiscordMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordMessage"/> class.
        /// </summary>
        public DiscordMessage()
        {
            Embeds = new List<DiscordEmbed>();
        }

        /// <summary>
        /// Gets or sets avatar url.
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// Gets or sets username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets content.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets embeds.
        /// </summary>
        public List<DiscordEmbed> Embeds { get; }
    }
}