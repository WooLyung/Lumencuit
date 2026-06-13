using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 세이브 관리용 클래스입니다.
    /// </summary>
    public static class SaveManagement
    {
        // 경로 및 파일명
        private const string SaveDirectoryName = "Saves";
        private const string SaveFileName = "save.xml";
        private static readonly string saveDirectoryPath = Path.Combine(Application.persistentDataPath, SaveDirectoryName);
        private static readonly string saveFilePath = Path.Combine(saveDirectoryPath, SaveFileName);

        // 버전 및 데이터
        private const int CurrentVersion = 1;
        private static GlobalSaveData globalData = new();
        public static GlobalSaveData GlobalData => globalData;

        // 처리용
        private static readonly object fileLock = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            try
            {
                Directory.CreateDirectory(saveDirectoryPath);
                Load();
            }
            catch (Exception e)
            {
                Logger.Error("Failed to initialize SaveManagement. Application will quit.", "SaveManagement", e);
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                return;
            }
        }

        /// <summary>
        /// 저장된 세이브 파일을 불러옵니다.
        /// </summary>
        public static void Load()
        {
            // 저장된 세이브 파일이 없다면 생성
            if (!File.Exists(saveFilePath))
            {
                Logger.Info("Save file does not exist. Creating new save file.", "SaveManagement");

                globalData = new GlobalSaveData();
                Save();
                return;
            }

            // 있다면 세이브 파일의 버전을 읽고 버전에 맞춰 파일 로드
            try
            {
                int version = ReadSaveVersion(saveFilePath);

                switch (version)
                {
                    case 1:
                        SaveFileDataV1 fileData = LoadSaveFile<SaveFileDataV1>();
                        globalData = new GlobalSaveData();
                        globalData.LoadFromFileData(fileData.Global);
                        break;

                    default:
                        Logger.Warning($"Unsupported save version: {version}. Creating new save data.", "SaveManagement");
                        BackupSave();
                        globalData = new GlobalSaveData();
                        Save();
                        return;
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to load save file. Creating new save data.", "SaveManagement", e);
                BackupSave();
                globalData = new GlobalSaveData();
                Save();
            }
        }

        /// <summary>
        /// 세이브 파일을 저장합니다.
        /// </summary>
        public static void Save()
        {
            SaveFileDataV1 fileData = new SaveFileDataV1
            {
                Version = CurrentVersion,
                Global = globalData.ToFileData()
            };

            string tempPath = saveFilePath + ".tmp";

            try
            {
                lock (fileLock)
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(SaveFileDataV1));
                    using (FileStream stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                        serializer.Serialize(stream, fileData);
                    if (File.Exists(saveFilePath))
                        File.Delete(saveFilePath);
                    File.Move(tempPath, saveFilePath);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to write save file.", "SaveManagement", e);
                try
                {
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                }
                catch (Exception e2)
                {
                    Logger.Error("Failed to delete temporary save file.", "SaveManagement", e2);
                }
                throw;
            }
        }

        /// <summary>
        /// 버전에 맞게 세이브 파일을 불러옵니다.
        /// </summary>
        /// <typeparam name="T">버전</typeparam>
        private static T LoadSaveFile<T>() where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using FileStream stream = new FileStream(saveFilePath, FileMode.Open, FileAccess.Read);
            T fileData = serializer.Deserialize(stream) as T;

            if (fileData == null)
                throw new InvalidDataException($"{typeof(T).Name} is null.");

            return fileData;
        }

        /// <summary>
        /// 세이브 파일로부터 버전을 가져옵니다.
        /// </summary>
        private static int ReadSaveVersion(string path)
        {
            XDocument document = XDocument.Load(path);
            XElement root = document.Root;

            if (root == null)
                throw new InvalidDataException("Save file has no root element.");

            XElement versionElement = root.Element("Version");

            if (versionElement == null)
                throw new InvalidDataException("Save file has no Version element.");

            if (!int.TryParse(versionElement.Value, out int version))
                throw new InvalidDataException($"Invalid save version: {versionElement.Value}");

            return version;
        }

        /// <summary>
        /// 세이브 파일 불러오기에 실패한 경우 백업합니다.
        /// </summary>
        private static void BackupSave()
        {
            try
            {
                if (!File.Exists(saveFilePath))
                    return;

                string backupFileName = "backup_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".xml";
                string backupPath = Path.Combine(saveDirectoryPath, backupFileName);
                File.Copy(saveFilePath, backupPath, true);
                Logger.Warning($"Invalid save file backed up: {backupFileName}", "SaveManagement");
            }
            catch (Exception e)
            {
                Logger.Error("Failed to backup invalid save file.", "SaveManagement", e);
            }
        }
    }
}