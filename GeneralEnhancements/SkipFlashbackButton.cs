using System.Collections.Generic;
using UnityEngine;

namespace GeneralEnhancements
{
    /// <summary>
    /// Doesn't work very well.
    /// </summary>
    public sealed class SkipFlashbackButton : Feature
    {
        static bool enabled;
        bool skipping;
        Flashback flashback;
        LoadManager loadManager;
        public SkipFlashbackButton()
        {
            enabled = false;
            skipping = false;
            flashback = Object.FindObjectOfType<Flashback>();
            loadManager = Object.FindObjectOfType<LoadManager>();

            GlobalMessenger.RemoveListener("TriggerFlashback", OnTriggerFlashback);
            GlobalMessenger.AddListener("TriggerFlashback", OnTriggerFlashback);
        }
        static void OnTriggerFlashback()
        {
            AdvancedMinimap.IsReady = false;

            if (ModCompatibility.hasTimeSaver) return;

            enabled = true;
        }
        public override void Update()
        {
            if (!enabled) return;
            if (skipping)
            {
                if (flashback._flashbackTimer.SecondsNewlyElapsed(flashback._flashbackTimer.GetDuration() - 5f))
                {
                    flashback._updateFlashback = false;
                }
                return;
            }

            if (OWInput.IsNewlyPressed(InputLibrary.interact))
            {
                skipping = true;

                //flashback._updateFlashback = false;
                TimeLoop.RestartTimeLoop();
                if (LoadManager.IsAsyncLoadComplete())
                {
                    LoadManager.EnableAsyncLoadTransition();
                    return;
                }
                flashback._waitForLoading = true;
                SpinnerUI.Show();
            }
        }
    }
}