using UnityEngine;

public static class Logger {

    /// <summary>
    /// Prints out a simple debug statement to the console, provided the debugLevel in the [SceneManager] is high enough
    /// </summary>
    /// <param name="debugLevel">0 = Important, will always be logged, 1 = Advanced , 2 = Info, only on highest level</param>
    /// <param name="loggingText"></param>
    public static void Log(int debugLevel, string loggingText) {
        if (SceneManager.DEBUG_LEVEL >= debugLevel) {
            Debug.Log(loggingText);
        }
    }


    /// <summary>
    /// Prints out a simple debug statement to the console, provided the debugLevel in the [SceneManager] is high enough
    /// </summary>
    /// <param name="debugLevel">0 = Important, will always be logged, 1 = Advanced , 2 = Info, only on highest level</param>
    /// <param name="loggingText"></param>
    public static void Log(int debugLevel, string[] loggingTexts) {
        if (SceneManager.DEBUG_LEVEL >= debugLevel) {
            foreach (string text in loggingTexts) {
                Debug.Log(text);
            }
        }
    }

    public static void LogWarning(string errorText) {
        Debug.LogWarning(errorText);
    }

    public static void LogError(string errorText) {
        Debug.LogError(errorText);
    }
}
