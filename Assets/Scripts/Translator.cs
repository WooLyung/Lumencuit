using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 게임 내 텍스트를 번역합니다.
    /// </summary>
    public static class Translator
    {
        private static string lang = "";
        private static Dictionary<string, string> textMap = null;

        public static string LangPath => Path.Combine(Application.dataPath, "../Lang");
        public static string Lang => lang;

        /// <summary>
        /// 주어진 key를 설정된 언어에 맞춰 번역합니다.
        /// </summary>
        public static string Translate(this string key)
        {
            if (textMap == null)
                return key;
            if (textMap.TryGetValue(key, out string value))
                return value;
            return key;
        }

        /// <summary>
        /// 주어진 key를 설정된 언어에 맞춰 번역하고 포맷팅합니다.
        /// </summary>
        public static string Translate(this string key, params object[] args)
        {
            if (textMap == null)
                return key;
            if (textMap.TryGetValue(key, out string value))
                return string.Format(value, args);
            return key;
        }

        /// <summary>
        /// 언어 파일을 불러옵니다.
        /// </summary>
        public static void Load(string lang)
        {
            Translator.lang = lang;
            textMap = new();

            string path = Path.Combine(LangPath, lang);
            if (!Directory.Exists(path))
            {
                textMap = null;
                Debug.LogError($"Language directory not found: {path}");
                return;
            }
            LoadDirectory(path);
        }

        private static void LoadDirectory(string path)
        {
            foreach (string filePath in Directory.GetFiles(path, "*.xml"))
                LoadXmlFile(filePath);

            foreach (string directoryPath in Directory.GetDirectories(path))
                LoadDirectory(directoryPath);
        }

        private static void LoadXmlFile(string path)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(path);

                XmlNodeList nodes = document.SelectNodes("//Text");
                foreach (XmlNode node in nodes)
                {
                    XmlAttribute keyAttribute = node.Attributes?["key"];
                    if (keyAttribute == null)
                        continue;

                    textMap[keyAttribute.Value] = node.InnerText;
                }
            }
            catch (XmlException e)
            {
                Debug.LogError($"Invalid language file: {path}\n{e}");
            }
        }
    }
}