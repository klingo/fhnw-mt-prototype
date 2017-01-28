// The MIT License (MIT)

// Copyright(c) 2017 Fabian Schär

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using UnityEngine;

public static class Logger {

    /// <summary>
    /// Prints out a simple debug statement to the console, provided the debugLevel
    /// in the [SceneManager] is high enough
    /// </summary>
    /// <param name="debugLevel">0 = Important, will always be logged, 1 = Advanced , 2 = Info, only on highest level</param>
    /// <param name="loggingText"></param>
    public static void Log(int debugLevel, string loggingText) {
        if (SceneManager.DEBUG_LEVEL >= debugLevel) {
            Debug.Log(loggingText);
        }
    }


    /// <summary>
    /// Prints out a simple debug statement to the console, provided the debugLevel
    /// in the [SceneManager] is high enough
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


    /// <summary>
    /// Prints out a warning debug statement to the console. Always works, independent
    /// of the debugLevel in the [SceneManager]
    /// </summary>
    /// <param name="errorText"></param>
    public static void LogWarning(string errorText) {
        Debug.LogWarning(errorText);
    }


    /// <summary>
    /// /// Prints out an error debug statement to the console. Always works, independent
    /// of the debugLevel in the [SceneManager]
    /// </summary>
    /// <param name="errorText"></param>
    public static void LogError(string errorText) {
        Debug.LogError(errorText);
    }
}