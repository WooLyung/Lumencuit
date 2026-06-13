using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Lumencuit
{
    /// <summary>
    /// 게임 로그를 출력 및 관리합니다.
    /// </summary>
    public static class Logger
    {
        private const string LogDirectoryName = "Logs";
        private const string LatestLogFileName = "latest.log";

        private static readonly object fileLock = new();

        private static bool isWritable;
        private static string latestLogPath;
        private static string sessionLogPath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            try
            {
                string logDirectoryPath = Path.Combine(Application.persistentDataPath, LogDirectoryName);
                Directory.CreateDirectory(logDirectoryPath);

                string sessionFileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".log";
                sessionLogPath = Path.Combine(logDirectoryPath, sessionFileName);
                latestLogPath = Path.Combine(logDirectoryPath, LatestLogFileName);

                string header =
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Info] [Logger] Log started\n" +
                    $"Unity Version: {Application.unityVersion}\n" +
                    $"Application Version: {Application.version}\n" +
                    $"Platform: {Application.platform}\n" +
                    $"Persistent Data Path: {Application.persistentDataPath}\n\n";

                File.WriteAllText(latestLogPath, header, Encoding.UTF8);
                File.WriteAllText(sessionLogPath, header, Encoding.UTF8);

                isWritable = true;
                Application.logMessageReceived += OnUnityLogReceived;
            }
            catch (Exception)
            {
                isWritable = false;
            }
        }

        public static void Info(string message, string category = "")
        {
            Log("Info", message, category, null);
        }

        public static void Warning(string message, string category = "")
        {
            Log("Warning", message, category, null);
        }

        public static void Error(string message, string category = "", Exception exception = null)
        {
            Log("Error", message, category, exception);
        }

        private static void OnUnityLogReceived(string condition, string stackTrace, LogType type)
        {
            string level = type switch
            {
                LogType.Warning => "Warning",
                LogType.Error => "Error",
                LogType.Exception => "Error",
                LogType.Assert => "Error",
                _ => "Info"
            };

            string message = condition;
            if (!string.IsNullOrEmpty(stackTrace) && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
                message += $"\n{stackTrace}";

            Log(level, message, "Unity", null);
        }

        private static void Log(string level, string message, string category, Exception exception)
        {
            if (!isWritable)
                return;

            string line = Line(level, message, category, exception);

            try
            {
                lock (fileLock)
                {
                    File.AppendAllText(latestLogPath, line + "\n", Encoding.UTF8);
                    File.AppendAllText(sessionLogPath, line + "\n", Encoding.UTF8);
                }
            }
            catch
            {
            }
        }

        private static string Line(string level, string message, string category, Exception exception)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string categoryText = string.IsNullOrEmpty(category) ? "" : $"[{category}] ";
            string line = $"[{time}] [{level}] {categoryText}{message}";

            if (exception != null)
                line += $"\n{exception}";

            return line;
        }
    }
}