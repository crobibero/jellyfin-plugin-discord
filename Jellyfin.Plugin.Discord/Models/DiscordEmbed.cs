using System;
using System.Collections.Generic;

namespace Jellyfin.Plugin.Discord.Models
{
    /// <summary>
    /// Discord embed.
    /// </summary>
    public class DiscordEmbed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordEmbed"/> class.
        /// </summary>
        public DiscordEmbed()
        {
            Fields = new List<Field>();
        }

        /// <summary>
        /// Gets or sets color.
        /// </summary>
        public int Color { get; set; }

        /// <summary>
        /// Gets or sets title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets url.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets thumbnail.
        /// </summary>
        public Thumbnail Thumbnail { get; set; }

        /// <summary>
        /// Gets fields.
        /// </summary>
        public List<Field> Fields { get; }

        /// <summary>
        /// Gets or sets footer.
        /// </summary>
        public Footer Footer { get; set; }

        /// <summary>
        /// Gets or sets timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }
    }
}