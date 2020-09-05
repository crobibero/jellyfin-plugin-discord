using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Jellyfin.Plugin.Discord.Configuration;
using Jellyfin.Plugin.Discord.Models;
using MediaBrowser.Common.Json;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Services;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Discord.Api
{
    /// <summary>
    /// Notifications service.
    /// </summary>
    public class NotificationsService : IService
    {
        private readonly ILogger<NotificationsService> _logger;
        private readonly IServerConfigurationManager _serverConfiguration;
        private readonly IHttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationsService"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{NoticiationsService}"/> interface.</param>
        /// <param name="serverConfiguration">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
        /// <param name="httpClient">Instance of the <see cref="IHttpClient"/> interface.</param>
        public NotificationsService(
            ILogger<NotificationsService> logger,
            IServerConfigurationManager serverConfiguration,
            IHttpClient httpClient)
        {
            _logger = logger;
            _serverConfiguration = serverConfiguration;
            _httpClient = httpClient;
            _jsonSerializerOptions = JsonDefaults.GetOptions();
            _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        }

        private DiscordOptions GetOptions(string userId)
        {
            return DiscordPlugin.Instance.Configuration.Options
                .FirstOrDefault(i => string.Equals(i.UserId, userId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Post test notification.
        /// </summary>
        /// <param name="request">The test notification request.</param>
        public void Post(TestNotification request)
        {
            var task = PostAsync(request);
            Task.WaitAll(task);
        }

        /// <summary>
        /// Post test notification async.
        /// </summary>
        /// <param name="request">The test notification request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task PostAsync(TestNotification request)
        {
            var options = GetOptions(request.UserId);

            var footerText = options.ServerNameOverride ? $"From {_serverConfiguration.Configuration.ServerName}" : "From Jellyfin Server";

            var discordMessage = new DiscordMessage();
            discordMessage.AvatarUrl = options.AvatarUrl;
            discordMessage.Username = options.Username;
            discordMessage.Embeds.Add(
                new DiscordEmbed
                {
                    Color = int.Parse(options.EmbedColor.Substring(1, 6), NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                    Description = "This is a test notification from Jellyfin",
                    Title = "It worked!",
                    Footer = new Footer
                    {
                        IconUrl = options.AvatarUrl,
                        Text = footerText
                    },
                    Timestamp = DateTime.Now
                });

            discordMessage.Content = options.MentionType switch
            {
                MentionType.Everyone => "@everyone",
                MentionType.Here => "@here",
                _ => string.Empty
            };

            try
            {
                await DiscordWebhookHelper.ExecuteWebhook(_httpClient, _logger, discordMessage, options.DiscordWebhookUri, _jsonSerializerOptions)
                    .ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to execute webhook", e);
                throw;
            }
        }
    }
}
