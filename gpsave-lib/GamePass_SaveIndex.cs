namespace gpsave
{
    /// <summary>
    /// Save index from a Game Pass save
    /// </summary>
    public class GamePass_SaveIndex
    {
        /// <summary>
        /// Package name associated to the index
        /// </summary>
        public string AppName { get; set; } = string.Empty;

        /// <summary>
        /// Index file name, should be containers.index
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Array of save containers listed in index save file
        /// </summary>
        public GamePass_SaveContainer[]? Containers { get; set; }
    }
}
