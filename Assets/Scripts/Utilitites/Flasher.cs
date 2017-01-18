using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Flasher {

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
