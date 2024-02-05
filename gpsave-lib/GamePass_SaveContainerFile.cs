namespace gpsave
{
    /// <summary>
    /// Save file meta from Game Pass save
    /// </summary>
    public class GamePass_SaveContainerFile
    {
        /// <summary>
        /// Full path on disk from container where the file came from
        /// </summary>
        public string RootPath { get; set; } = string.Empty;

        /// <summary>
        /// Real file name
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// File name in container folder
        /// </summary>
        public string FileId { get; set; } = string.Empty;

        /// <summary>
        /// Second possible file name in container folder
        /// </summary>
        public string FileId_2 { get; set; } = string.Empty;
    }
}
