using System.Text;

namespace gpsave
{
    /// <summary>
    /// Class containing methods to find, read and export the Game Pass save file
    /// </summary>
    /// <see href="https://github.com/goatfungus/NMSSaveEditor/issues/306" />
    /// <seealso href="https://github.com/Z1ni/XGP-save-extractor"/>
    public class GamePass_SaveReader
    {
        /// <summary>
        /// Folder where games and applications from MS are installed
        /// </summary>
        public static readonly string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Packages";


        /// <summary>
        /// Game or application folder name in rootPath
        /// </summary>
        public string GameFolder { get; internal set; }


        /// <summary>
        /// Create an instance of GamePass_SaveReader
        /// </summary>
        /// <param name="gameFolder">Game or application folder in rootPath</param>
        /// <exception cref="Exception">Raise when <paramref name="gameFolder"/> doesn't exists in rootPath</exception>
        public GamePass_SaveReader(string gameFolder)
        {
            if (!Directory.Exists(Path.Combine(rootPath, gameFolder)))
            {
                throw new Exception(string.Format("Directory {0} don't exists in {1}", gameFolder, rootPath));
            }

            GameFolder = gameFolder;
        }


        /// <summary>
        /// List all installed games or applications in rootPath
        /// </summary>
        /// <returns cref="string[]">String array of folder with path in rootPath</returns>
        public static string[] ListAppFolder()
        {
            return Directory.GetDirectories(rootPath).ToArray();
        }


        /// <summary>
        /// Locate containers.index file in game or application folder
        /// </summary>
        /// <returns cref="string">Full path of containers.index</returns>
        /// <exception cref="Exception">Raise an exception if containers.index has not been found in game or application folder</exception>
        public string GetIndexFilePath()
        {
            string saveParentDir = Path.Combine(rootPath, GameFolder, "SystemAppData\\wgs");

            string indexFilePath = "";
            foreach (string directory in Directory.GetDirectories(saveParentDir))
            {
                string tmpPath = Path.Combine(directory, "containers.index");
                if (File.Exists(tmpPath))
                {
                    indexFilePath = tmpPath;
                    break;
                }
            }

            if (indexFilePath.Length == 0)
            {
                throw new Exception(string.Format("Can't locate containers.index file for {0}", GameFolder));
            }

            return indexFilePath;
        }


        /// <summary>
        /// Read containers.index from game or application folder and return a GamePass_SaveIndex object
        /// </summary>
        /// <returns cref="GamePass_SaveIndex">GamePass_SaveIndex containing information from containers.index reading</returns>
        public GamePass_SaveIndex ReadIndex()
        {
            string indexFile = GetIndexFilePath();

            GamePass_SaveIndex saveIndex = new GamePass_SaveIndex();
            saveIndex.FileName = Path.GetFileName(indexFile);

            using (BinaryReader reader = new BinaryReader(File.Open(indexFile, FileMode.Open)))
            {
                reader.ReadBytes(4); // offset 4

                saveIndex.Containers = new GamePass_SaveContainer[reader.ReadInt32()]; // number of containers

                reader.ReadBytes(4); // offset 4

                byte[] rawAppName = reader.ReadBytes(reader.ReadInt32() * 2);
                saveIndex.AppName = Encoding.Unicode.GetString(rawAppName); // package name

                reader.ReadBytes(12); // offset 12

                reader.ReadBytes(reader.ReadInt32() * 2); // string, but don't known what's for
                reader.ReadBytes(8); // offset 8

                // Read containers
                for (int i = 0; i < saveIndex.Containers.Length; ++i)
                {
                    saveIndex.Containers[i] = new GamePass_SaveContainer();

                    byte[] rawSaveName = reader.ReadBytes(reader.ReadInt32() * 2); // save name
                    saveIndex.Containers[i].SaveName = Encoding.Unicode.GetString(rawSaveName);

                    reader.ReadBytes(4 + rawSaveName.Length); // save name seem to be written two times

                    reader.ReadBytes(reader.ReadInt32() * 2); ; // some string again, don't known what's for

                    saveIndex.Containers[i].Id = reader.ReadByte(); // container file extension

                    reader.ReadBytes(4); // offset 4

                    saveIndex.Containers[i].FileName = GetUUIDFromBytes(reader.ReadBytes(16));
                    saveIndex.Containers[i].FilePath = Path.Combine(Path.GetDirectoryName(indexFile), saveIndex.Containers[i].FileName); // container folder name
                    saveIndex.Containers[i].CreatedAt = DateTimeOffset.FromFileTime(reader.ReadInt64()).LocalDateTime; // container date
                    saveIndex.Containers[i].CreatedAtPrecision = reader.ReadInt64(); // container date with precision under a second

                    reader.ReadBytes(8); // offset 8, can be read to have more precise save date
                }
            }

            return saveIndex;
        }

        
        /// <summary>
        /// Read container.x file from a save for game or application folder and return a GamePass_SaveContainerFile[]
        /// </summary>
        /// <param name="container" cref="GamePass_SaveContainer">Container to read</param>
        /// <returns cref="GamePass_SaveContainerFile[]">Array of GamePass_SaveContainerFile</returns>
        public GamePass_SaveContainerFile[] ReadContainer(GamePass_SaveContainer container)
        {
            string containerFile = Path.Combine(container.FilePath, string.Format("container.{0}", container.Id));

            List<GamePass_SaveContainerFile> containerFiles = new List<GamePass_SaveContainerFile>();
            using (BinaryReader reader = new BinaryReader(File.Open(containerFile, FileMode.Open)))
            {
                reader.ReadBytes(4); // offset 4

                int nbFile = reader.ReadInt32();
                for (int i = 0; i < nbFile; ++i)
                {
                    GamePass_SaveContainerFile item = new GamePass_SaveContainerFile();

                    byte[] rawFileName = reader.ReadBytes(128);
                    item.FileName = Encoding.Unicode.GetString(rawFileName).Split('\0')[0];
                    item.FileId = GetUUIDFromBytes(reader.ReadBytes(16));
                    item.FileId_2 = GetUUIDFromBytes(reader.ReadBytes(16));
                    item.RootPath = container.FilePath;

                    containerFiles.Add(item);
                }
            }

            return containerFiles.ToArray();
        }


        /// <summary>
        /// Export all save file from a save container to an output folder
        /// </summary>
        /// <param name="container">Container</param>
        /// <param name="outputFolder">Destination folder</param>
        /// <returns cref="int">Number of exported save file</returns>
        public int ExportSave(GamePass_SaveContainer container, string outputFolder)
        {
            string saveDirectory = Path.Combine(
                outputFolder,
                string.Format("{0}_{1}", container.CreatedAt.ToString("yyyyMMdd_hhmmss"), container.SaveName)
            );

            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }

            int fileSaved = 0;

            GamePass_SaveContainerFile[] files = ReadContainer(container);
            for (int i = 0; i < files.Length; ++i)
            {
                string file_1 = Path.Combine(files[i].RootPath, files[i].FileId);
                string file_2 = Path.Combine(files[i].RootPath, files[i].FileId_2);

                if (File.Exists(file_1))
                {
                    File.Copy(file_1, Path.Combine(saveDirectory, files[i].FileName), true);
                    fileSaved++;
                }
                else if (File.Exists(file_2))
                {
                    File.Copy(file_2, Path.Combine(saveDirectory, files[i].FileName), true);
                    fileSaved++;
                }
            }

            return fileSaved;
        }


        /// <summary>
        /// Export all save from game or application to an output folder
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="outputFolder">Destination folder</param>
        /// <returns cref="int">Number of exported save file</returns>
        public int ExportSaveAll(GamePass_SaveIndex index, string outputFolder)
        {
            int fileSaved = 0;

            for (int i = 0; i < index.Containers.Length; ++i)
            {
                fileSaved += ExportSave(index.Containers[i], outputFolder);
            }

            return fileSaved;
        }


        string GetUUIDFromBytes(byte[] rawUUID)
        {
            byte[] byteUUID = new byte[16] {
                rawUUID[3], rawUUID[2], rawUUID[1], rawUUID[0],
                rawUUID[5], rawUUID[4], rawUUID[7], rawUUID[6],
                rawUUID[8], rawUUID[9], rawUUID[10], rawUUID[11],
                rawUUID[12], rawUUID[13], rawUUID[14], rawUUID[15],
            };

            return Convert.ToHexString(byteUUID);
        }
    }
}
