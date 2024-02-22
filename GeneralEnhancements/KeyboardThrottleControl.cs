using UnityEngine;
using UnityEngine.InputSystem;

namespace GeneralEnhancements
{
    public sealed class KeyboardThrottleControl : Feature
    {
        static float shipThrottle;
        static float playerThrottle;
        public static float ShipThrottle => shipThrottle;
        public static float PlayerThrottle => playerThrottle;
        JetpackThrusterModel jetpack;

        public static void ResetThrottle()
        {
            ResetPlayer();
            ResetShip();
        }
        public static void ResetPlayer() => playerThrottle = 1f;
        public static void ResetShip() => shipThrottle = 1f;
        public KeyboardThrottleControl()
        {
            ResetThrottle();
        }
        public override void LateInitialize()
        {
            jetpack = Locator.GetPlayerController().GetComponent<JetpackThrusterModel>();
        }
        public override void Update()
        {
            if (OWInput.UsingGamepad() || !Settings.AllowThrottleScroll)
            {
                ResetThrottle();
                return; //Only Keyboard.
            }
            if (jetpack.IsBoosterFiring()) ResetPlayer();

            if (OWInput.IsInputMode(InputMode.ShipCockpit))
            {
                HandleThrottle(ref shipThrottle);
            }
            else if (OWInput.IsInputMode(InputMode.ModelShip))
            {
                HandleThrottle(ref shipThrottle);
            }
            else
            {
                HandleThrottle(ref playerThrottle);
            }
        }

        void HandleThrottle(ref float throttle)
        {
            float scroll = Mouse.current.scroll.y.ReadValue();

            if (scroll > 0.1f)
            {
                throttle += 0.2f;
            }
            if (scroll < -0.1f)
            {
                throttle -= 0.2f;
            }

            throttle = Mathf.Clamp(throttle, 0.2f, 1f);
        }
    }
}