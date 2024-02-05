namespace gpsave
{
    /// <summary>
    /// Container meta from a Game Pass save
    /// </summary>
    public class GamePass_SaveContainer
    {
        /// <summary>
        /// File extension of container.x file in container folder
        /// </summary>
        public byte Id { get; set; }

        /// <summary>
        /// Save creation date
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Under a second precision part of save creation date
        /// </summary>
        public long CreatedAtPrecision { get; set; } = 0;

        /// <summary>
        /// Full path where the container is located on disk
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Container folder name
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Real name of the save file
        /// </summary>
        public string SaveName { get; set; } = string.Empty;
    }
}
