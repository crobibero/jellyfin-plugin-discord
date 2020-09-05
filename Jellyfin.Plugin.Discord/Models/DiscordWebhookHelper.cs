using System.Globalization;
using System.Net.Mime;
using System.Text.Json;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Discord.Models
{
    /// <summary>
    /// Discord webhook helper.
    /// </summary>
    public static class DiscordWebhookHelper
    {
        /// <summary>
        /// Format color code.
        /// </summary>
        /// <param name="hexCode">Hex code.</param>
        /// <returns>Color integer.</returns>
        public static int FormatColorCode(string hexCode)
        {
            return int.Parse(hexCode.Substring(1, 6), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Execute webhook.
        /// </summary>
        /// <param name="httpClient">The http client.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="message">Message.</param>
        /// <param name="webhookUrl">Webhook url.</param>
        /// <param name="jsonSerializerOptions">Json serializer options.</param>
        /// <returns>A <see cref="Task"/>.</returns>
        public static async Task ExecuteWebhook(IHttpClient httpClient, ILogger logger, DiscordMessage message, string webhookUrl, JsonSerializerOptions jsonSerializerOptions)
        {
            var jsonString = JsonSerializer.Serialize(message, jsonSerializerOptions);
            logger.LogDebug("Execute Webhook: {0}", jsonString);
            var options = new HttpRequestOptions
            {
                Url = webhookUrl,
                RequestContent = jsonString,
                RequestContentType = MediaTypeNames.Application.Json,
                LogErrorResponseBody = true
            };

            using var response = await httpClient.Post(options).ConfigureAwait(false);
        }
    }
}
