using System;
using System.Collections.Generic;
using Jellyfin.Plugin.Discord.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.Discord
{
    /// <summary>
    /// Plugin entrypoint.
    /// </summary>
    public class DiscordPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        private readonly Guid _id = new Guid("71552A5A-5C5C-4350-A2AE-EBE451A30173");

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordPlugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
        /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
        public DiscordPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        /// <inheritdoc />
        public override string Name => "Discord";

        /// <inheritdoc />
        public override string Description => "Sends notifications to Discord via webhooks.";

        /// <inheritdoc />
        public override Guid Id => _id;

        /// <summary>
        /// Gets current plugin instance.
        /// </summary>
        public static DiscordPlugin Instance { get; private set; }

        /// <inheritdoc />
        public IEnumerable<PluginPageInfo> GetPages()
        {
            yield return new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.config.html"
            };

            yield return new PluginPageInfo
            {
                Name = $"{Name}JS",
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.config.js"
            };
        }

        /*public TranslationInfo[] GetTranslations()
        {
            string basePath = GetType().Namespace + ".strings.";

            return GetType()
                .Assembly
                .GetManifestResourceNames()
                .Where(i => i.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                .Select(i => new TranslationInfo
                {
                    Locale = Path.GetFileNameWithoutExtension(i.Substring(basePath.Length)),
                    EmbeddedResourcePath = i

                }).ToArray();
        }*/
    }
}