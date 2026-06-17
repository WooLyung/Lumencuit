using System;
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

            try
            {
                return string.Format(value, args);
            }
            catch (FormatException e)
            {
                Logger.Error($"Invalid language format. Key={key}, Value={value}", "Lang", e);
                return key;
            }
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
                Logger.Error($"Language directory not found: {path}", "Translator");
                return;
            }
            LoadDirectory(path);
        }

        private static void LoadDirectory(string path)
        {
            try
            {
                foreach (string filePath in Directory.GetFiles(path, "*.xml"))
                    LoadXmlFile(filePath);

                foreach (string directoryPath in Directory.GetDirectories(path))
                    LoadDirectory(directoryPath);
            }
            catch (IOException e)
            {
                Logger.Error($"Failed to load language directory. Path={path}", "Translator", e);
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Error($"No permission to load language directory. Path={path}", "Translator", e);
            }
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
                Logger.Error($"Invalid language file: {path}", "Lang", e);
            }
        }
    }
}