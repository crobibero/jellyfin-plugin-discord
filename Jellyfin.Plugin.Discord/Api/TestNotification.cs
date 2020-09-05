using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Services;

namespace Jellyfin.Plugin.Discord.Api
{
    /// <summary>
    /// Test notification.
    /// </summary>
    [Route("/Notifications/Discord/Test/{UserID}", "POST", Summary = "Tests Discord")]
    [Authenticated(Roles = "Admin")]
    public class TestNotification : IReturnVoid
    {
        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        [ApiMember(Name = "UserID", Description = "User Id", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "GET")]
        public string UserId { get; set; }
    }
}