using System.Net;
using System.Text;
using System.Text.Json;

namespace Jellyfin.Plugin.Discord.Models
{
    /// <summary>
    /// Memester service helper.
    /// </summary>
    public static class MemesterServiceHelper
    {
        private static string UploadEndpoint => "https://i.memester.xyz/upload?format=json";

        /// <summary>
        /// Upload image.
        /// </summary>
        /// <param name="path">image path.</param>
        /// <returns>The Image service response.</returns>
        public static ImageServiceResponse UploadImage(string path)
        {
            using var client = new WebClient();
            var response = client.UploadFile(UploadEndpoint, path);
            return JsonSerializer.Deserialize<ImageServiceResponse>(Encoding.Default.GetString(response));
        }
    }
}