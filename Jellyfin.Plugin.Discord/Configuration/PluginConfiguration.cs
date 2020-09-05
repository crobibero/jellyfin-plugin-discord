using System;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.Discord.Configuration
{
    /// <inheritdoc />
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
        /// </summary>
        public PluginConfiguration()
        {
            Options = Array.Empty<DiscordOptions>();
        }

        /// <summary>
        /// Gets or sets the configured options.
        /// </summary>
        public DiscordOptions[] Options { get; set; }
    }
}
