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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Flasher {

    /// <summary>
    /// Stops the flashing Coroutine anmitation of the provided Bar object. For this bar object all coroutines
    /// on its attached image objects will be stopped and in case they are currently partially faded out, brought back
    /// to full colour.
    /// </summary>
    /// <param name="barHolder">The bar whose flsahing should be stopped</param>
    public static void stopFlashingBar(Bar barHolder) {
        if (barHolder != null) {
            // if there was a previous bar, stop the blinking
            barHolder.StopAllCoroutines();
            // In case some images are currently partially faded out, fad them back in!
            List<Image> images = new List<Image>(barHolder.GetComponentsInChildren<Image>());
            foreach (Image image in images) {
                image.CrossFadeAlpha(1f, 0.5f, false);
            }
        }
    }


    /// <summary>
    /// Starts the flashing Coroutine animation on the provided list of images for a given duration.
    /// </summary>
    /// <param name="images">A list of images for which the flashing should start</param>
    /// <param name="duration">duration in seconds for one flashing animation</param>
    /// <returns></returns>
    public static IEnumerator Flash(List<Image> images, float duration) {
        bool fadeState = false;
        while (true) {
            if (fadeState) {
                foreach (Image image in images) {
                    // Flashing only goes back to 0.99 alpha value, in order to distinguish it with non-flasing
                    // bar that always has an alpha value of 1.00
                    image.CrossFadeAlpha(0.99f, duration, false);
                }
                yield return new WaitForSeconds(duration);
            }
            else {
                foreach (Image image in images) {
                    // Flashing only goes down to 0.51 alpha value, in order to distinguish it with non-flashing
                    // but highlighted bars that always have an alpha value of 0.50
                    image.CrossFadeAlpha(0.51f, duration, false);
                }
                yield return new WaitForSeconds(duration);
            }
            fadeState = !fadeState;
        }
    }
}