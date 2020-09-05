using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Jellyfin.Data.Entities;
using Jellyfin.Plugin.Discord.Configuration;
using Jellyfin.Plugin.Discord.Models;
using MediaBrowser.Common;
using MediaBrowser.Common.Json;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Notifications;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Discord
{
    /// <summary>
    /// Notification service.
    /// </summary>
    public class Notifier : INotificationService, IDisposable
    {
        private readonly ILogger<Notifier> _logger;
        private readonly IServerConfigurationManager _serverConfiguration;
        private readonly ILibraryManager _libraryManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly IUserViewManager _userViewManager;
        private readonly IHttpClient _httpClient;
        private readonly IApplicationHost _applicationHost;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        private readonly System.Timers.Timer _queuedMessageHandler;
        private readonly System.Timers.Timer _queuedUpdateHandler;
        private readonly Dictionary<Guid, QueuedUpdateData> _queuedUpdateCheck = new Dictionary<Guid, QueuedUpdateData>();
        private readonly Dictionary<DiscordMessage, DiscordOptions> _pendingSendQueue = new Dictionary<DiscordMessage, DiscordOptions>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Notifier"/> class.
        /// </summary>
        /// <param name="logger">Instance of the <see cref="ILogger{Notifier}"/> interface.</param>
        /// <param name="serverConfiguration">Instance of the <see cref="IServerConfigurationManager"/> interface.</param>
        /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
        /// <param name="localizationManager">Instance of the <see cref="ILocalizationManager"/> interface.</param>
        /// <param name="userViewManager">Instance of the <see cref="IUserViewManager"/> interface.</param>
        /// <param name="httpClient">Instance of the <see cref="IHttpClient"/> interface.</param>
        /// <param name="applicationHost">Instance of the <see cref="IApplicationHost"/> interface.</param>
        public Notifier(
            ILogger<Notifier> logger,
            IServerConfigurationManager serverConfiguration,
            ILibraryManager libraryManager,
            ILocalizationManager localizationManager,
            IUserViewManager userViewManager,
            IHttpClient httpClient,
            IApplicationHost applicationHost)
        {
            _logger = logger;
            _serverConfiguration = serverConfiguration;
            _libraryManager = libraryManager;
            _localizationManager = localizationManager;
            _userViewManager = userViewManager;
            _httpClient = httpClient;
            _applicationHost = applicationHost;
            _jsonSerializerOptions = JsonDefaults.GetOptions();
            _jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            _queuedMessageHandler = new System.Timers.Timer(Constants.MessageQueueSendInterval);
            _queuedMessageHandler.AutoReset = true;
            _queuedMessageHandler.Elapsed += QueuedMessageSender;
            _queuedMessageHandler.Start();

            _queuedUpdateHandler = new System.Timers.Timer(Constants.RecheckIntervalMs);
            _queuedUpdateHandler.AutoReset = true;
            _queuedUpdateHandler.Elapsed += CheckForMetadata;
            _queuedUpdateHandler.Start();

            _libraryManager.ItemAdded += ItemAddHandler;
            _logger.LogDebug("Registered ItemAdd handler");
        }

        /// <inheritdoc />
        public string Name => DiscordPlugin.Instance.Name;

        /// <inheritdoc />
        public async Task SendNotification(UserNotification request, CancellationToken cancellationToken)
        {
            await Task.Run(
                () =>
                {
                    var options = GetOptions(request.User);

                    if ((options.MediaAddedOverride && !request.Name.Contains(_localizationManager.GetLocalizedString("ValueHasBeenAddedToLibrary").Replace("{0} ", string.Empty, StringComparison.OrdinalIgnoreCase).Replace(" {1}", string.Empty, StringComparison.OrdinalIgnoreCase), StringComparison.OrdinalIgnoreCase)) || !options.MediaAddedOverride)
                    {
                        var serverName = _serverConfiguration.Configuration.ServerName;

                        string footerText;
                        string requestName;

                        if (options.ServerNameOverride)
                        {
                            footerText = $"From {serverName}";
                            requestName = request.Name.Replace("Jellyfin Server", serverName, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            requestName = request.Name;
                            footerText = "From Jellyfin Server";
                        }

                        var discordMessage = new DiscordMessage();
                        discordMessage.AvatarUrl = options.AvatarUrl;
                        discordMessage.Username = options.Username;
                        discordMessage.Embeds.Add(
                            new DiscordEmbed
                            {
                                Color = DiscordWebhookHelper.FormatColorCode(options.EmbedColor),
                                Title = requestName,
                                Description = request.Description,
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

                        _pendingSendQueue.Add(discordMessage, options);
                    }
                }, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public bool IsEnabledForUser(User user)
        {
            var options = GetOptions(user);

            return options != null && options.Enabled;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose all resources.
        /// </summary>
        /// <param name="disposing">Dispose.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _libraryManager.ItemAdded -= ItemAddHandler;
                _queuedMessageHandler.Stop();
                _queuedMessageHandler.Dispose();
                _queuedUpdateHandler.Stop();
                _queuedUpdateHandler.Dispose();
            }
        }

        private async void QueuedMessageSender(object sender, ElapsedEventArgs eventArgs)
        {
            try
            {
                if (_pendingSendQueue.Count > 0)
                {
                    var message = _pendingSendQueue.First().Key;
                    var options = _pendingSendQueue.First().Value;

                    await DiscordWebhookHelper.ExecuteWebhook(_httpClient, _logger, message, options.DiscordWebhookUri, _jsonSerializerOptions).ConfigureAwait(false);

                    if (_pendingSendQueue.ContainsKey(message))
                    {
                        _pendingSendQueue.Remove(message);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to execute webhook");
            }
        }

        private void CheckForMetadata(object sender, ElapsedEventArgs eventArgs)
        {
            try
            {
                var queueCount = _queuedUpdateCheck.Count;
                if (queueCount > 0)
                {
                    _logger.LogDebug("Item in queue : {0}", queueCount);
                }

                _queuedUpdateCheck.ToList().ForEach(queuedItem =>
                {
                    // sometimes an update check might execute while another one is hanging and causes crash !
                    if (_queuedUpdateCheck.ContainsKey(queuedItem.Key))
                    {
                        var options = queuedItem.Value.Configuration;
                        var itemId = queuedItem.Value.ItemId;

                        _logger.LogDebug("{0} queued for recheck", itemId.ToString());

                        var item = _libraryManager.GetItemById(itemId);

                        var itemLibraryOptions = _libraryManager.GetLibraryOptions(item);
                        // var sysInfo = await _applicationHost.GetPublicSystemInfo(CancellationToken.None);
                        var serverConfig = _serverConfiguration.Configuration;

                        var libraryType = item.GetType().Name;
                        var serverName = options.ServerNameOverride ? serverConfig.ServerName : "Jellyfin Server";

                        // for whatever reason if you have extraction on during library scans then it waits for the extraction to finish before populating the metadata.... I don't get why the fuck it goes in that order
                        // its basically impossible to make a prediction on how long it could take as its dependent on the bitrate, duration, codec, and processing power of the system
                        var localMetadataFallback = _queuedUpdateCheck[queuedItem.Key].Retries >= (itemLibraryOptions.ExtractChapterImagesDuringLibraryScan ? Constants.MaxRetriesBeforeFallback * 5.5 : Constants.MaxRetriesBeforeFallback);

                        if (item.ProviderIds.Count > 0 || localMetadataFallback)
                        {
                            _logger.LogDebug("{0}[{1}] has metadata (Local fallback: {2}), adding to queue", item.Id, item.Name, localMetadataFallback, options.UserId);

                            if (_queuedUpdateCheck.ContainsKey(queuedItem.Key))
                            {
                                // remove it beforehand because if some operation takes any longer amount of time it might allow enough time for another notification to slip through
                                _queuedUpdateCheck.Remove(queuedItem.Key);
                            }

                            // build primary info
                            var mediaAddedEmbed = new DiscordMessage();
                            mediaAddedEmbed.Username = options.Username;
                            mediaAddedEmbed.AvatarUrl = options.AvatarUrl;
                            mediaAddedEmbed.Embeds.Add(
                                new DiscordEmbed
                                {
                                    Color = DiscordWebhookHelper.FormatColorCode(options.EmbedColor),
                                    Footer = new Footer
                                    {
                                        Text = $"From {serverName}",
                                        IconUrl = options.AvatarUrl
                                    },
                                    Timestamp = DateTime.Now
                                });

                            // populate title

                            string titleText;

                            if (libraryType == "Episode")
                            {
                                titleText = $"{item.Parent.Parent.Name}{(item.ParentIndexNumber.HasValue ? $" S{item.ParentIndexNumber:00}" : string.Empty)}{(item.IndexNumber.HasValue ? $"E{item.IndexNumber:00}" : string.Empty)} {item.Name}";
                            }
                            else if (libraryType == "Season")
                            {
                                titleText = $"{item.Parent.Name} {item.Name}";
                            }
                            else
                            {
                                titleText = $"{item.Name}{(item.ProductionYear.HasValue ? $" ({item.ProductionYear.ToString()})" : string.Empty)}";
                            }

                            mediaAddedEmbed.Embeds.First().Title = $"{titleText} has been added to {serverName.Trim()}";

                            // populate description
                            if (libraryType == "Audio")
                            {
                                var artists = _libraryManager.GetAllArtists(new InternalItemsQuery { ItemIds = new[] { itemId } });

                                var artistsFormat = artists.Items.Select(baseItem =>
                                {
                                    var artist = baseItem.Item1;
                                    var formattedArtist = artist.Name;

                                    if (artist.ProviderIds.Count != 0)
                                    {
                                        var (key, value) = artist.ProviderIds.FirstOrDefault();

                                        var providerUrl = key == "MusicBrainzArtist"
                                            ? $"https://musicbrainz.org/artist/{value}"
                                            : $"https://theaudiodb.com/artist/{value}";

                                        formattedArtist += $" [(Music Brainz)]({providerUrl})";
                                    }

                                    if (serverConfig.EnableRemoteAccess && !options.ExcludeExternalServerLinks)
                                    {
                                        formattedArtist += $" [(Jellyfin)]({options.ServerUrl}/web/index.html#!/item?id={itemId}&serverId={_applicationHost.SystemId})";
                                    }

                                    return formattedArtist;
                                });

                                if (artists.Items.Count != 0)
                                {
                                    mediaAddedEmbed.Embeds.First().Description = $"By {string.Join(", ", artistsFormat)}";
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(item.Overview))
                                {
                                    mediaAddedEmbed.Embeds.First().Description = item.Overview;
                                }
                            }

                            // populate title URL
                            if (serverConfig.EnableRemoteAccess && !options.ExcludeExternalServerLinks)
                            {
                                mediaAddedEmbed.Embeds.First().Url = $"{options.ServerUrl}/web/index.html#!/item?id={itemId}&serverId={_applicationHost.SystemId}";
                            }

                            // populate images
                            if (item.HasImage(ImageType.Primary))
                            {
                                var imageUrl = string.Empty;

                                if (!item.GetImageInfo(ImageType.Primary, 0).IsLocalFile)
                                {
                                    imageUrl = item.GetImagePath(ImageType.Primary);
                                }
                                else if (serverConfig.EnableRemoteAccess && !options.ExcludeExternalServerLinks) // in the future we can proxy images through memester server if people want to hide their server address
                                {
                                    imageUrl = $"{options.ServerUrl}/Items/{itemId}/Images/Primary";
                                }
                                else
                                {
                                    var localPath = item.GetImagePath(ImageType.Primary);

                                    try
                                    {
                                        var response = MemesterServiceHelper.UploadImage(localPath);
                                        imageUrl = response.FilePath;
                                    }
                                    catch (Exception e)
                                    {
                                        _logger.LogError(e, "Failed to proxy image");
                                    }
                                }

                                mediaAddedEmbed.Embeds.First().Thumbnail = new Thumbnail
                                {
                                    Url = imageUrl
                                };
                            }

                            mediaAddedEmbed.Content = options.MentionType switch
                            {
                                MentionType.Everyone => "@everyone",
                                MentionType.Here => "@here",
                                _ => string.Empty
                            };

                            // populate external URLs
                            var providerFields = new List<Field>();

                            if (!localMetadataFallback)
                            {
                                item.ProviderIds.ToList().ForEach(provider =>
                                {
                                    var field = new Field
                                    {
                                        Name = "External Links"
                                    };

                                    var didPopulate = true;

                                    switch (provider.Key.ToLower(CultureInfo.InvariantCulture))
                                    {
                                        case "imdb":
                                            field.Value = $"[IMDb](https://www.imdb.com/title/{provider.Value}/)";
                                            break;
                                        case "tmdb":
                                            field.Value = $"[TMDb](https://www.themoviedb.org/{(libraryType == "Movie" ? "movie" : "tv")}/{provider.Value})";
                                            break;
                                        case "musicbrainztrack":
                                            field.Value = $"[MusicBrainz Track](https://musicbrainz.org/track/{provider.Value})";
                                            break;
                                        case "musicbrainzalbum":
                                            field.Value = $"[MusicBrainz Album](https://musicbrainz.org/release/{provider.Value})";
                                            break;
                                        case "theaudiodbalbum":
                                            field.Value = $"[TADb Album](https://theaudiodb.com/album/{provider.Value})";
                                            break;
                                        default:
                                            didPopulate = false;
                                            break;
                                    }

                                    if (didPopulate)
                                    {
                                        providerFields.Add(field);
                                    }
                                });

                                if (providerFields.Count > 0)
                                {
                                    mediaAddedEmbed.Embeds.First().Fields.AddRange(providerFields);
                                }
                            }

                            _pendingSendQueue.Add(mediaAddedEmbed, options);
                        }
                        else
                        {
                            if (_queuedUpdateCheck.ContainsKey(queuedItem.Key))
                            {
                                _queuedUpdateCheck[queuedItem.Key].Retries++;
                            }
                        }
                    }
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something unexpected happened in the item update checker");
            }
        }

        private void ItemAddHandler(object sender, ItemChangeEventArgs changeEvent)
        {
            var item = changeEvent.Item;
            var libraryType = item.GetType().Name;

            DiscordPlugin.Instance.Configuration.Options.ToList().ForEach(options =>
            {
                var allowedItemTypes = new List<string>();
                if (options.EnableAlbums)
                {
                    allowedItemTypes.Add("MusicAlbum");
                }

                if (options.EnableMovies)
                {
                    allowedItemTypes.Add("Movie");
                }

                if (options.EnableEpisodes)
                {
                    allowedItemTypes.Add("Episode");
                }

                if (options.EnableSeries)
                {
                    allowedItemTypes.Add("Series");
                }

                if (options.EnableSeasons)
                {
                    allowedItemTypes.Add("Season");
                }

                if (options.EnableSongs)
                {
                    allowedItemTypes.Add("Audio");
                }

                if (
                    !item.IsVirtualItem
                    && Array.Exists(allowedItemTypes.ToArray(), t => t == libraryType)
                    && options.Enabled
                    && options.MediaAddedOverride
                    && IsInVisibleLibrary(options.UserId, item)
                )
                {
                    _queuedUpdateCheck.Add(Guid.NewGuid(), new QueuedUpdateData { Retries = 0, Configuration = options, ItemId = item.Id });
                }
            });
        }

        private bool IsInVisibleLibrary(string userId, BaseItem item)
        {
            var isIn = false;

            _userViewManager.GetUserViews(
                new UserViewQuery
                {
                    UserId = Guid.Parse(userId)
                }).ToList().ForEach(folder =>
            {
                var folders = folder.GetItemList(new InternalItemsQuery
                {
                    IncludeItemTypes = new[]
                    {
                        "MusicAlbum", "Movie", "Episode", "Series", "Season", "Audio"
                    }, Recursive = true
                });
                if (folders.Any(o => o.Id.Equals(item.Id)))
                {
                    isIn = true;
                }
            });

            return isIn;
        }

        private DiscordOptions GetOptions(User user)
        {
            return DiscordPlugin.Instance.Configuration.Options
                .FirstOrDefault(i => string.Equals(i.UserId, user.Id.ToString("N", CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase));
        }

        private class QueuedUpdateData
        {
            public int Retries { get; set; }

            public Guid ItemId { get; set; }

            public DiscordOptions Configuration { get; set; }
        }
    }
}