namespace Jellyfin.Plugin.Discord.Models
{
    /// <summary>
    /// Field.
    /// </summary>
    public class Field
    {
        /// <summary>
        /// Gets or sets name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether inline.
        /// </summary>
        public bool Inline { get; set; }
    }
}