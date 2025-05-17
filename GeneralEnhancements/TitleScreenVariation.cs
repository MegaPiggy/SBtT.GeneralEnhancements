using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GeneralEnhancements
{
    public sealed class TitleScreenVariation : Feature
    {
        public enum TitleCamAngle
        {
            Default,
            /*CampfireTest,
            POVTest,*/
            AboveFire = 3,
            AboveFireLookDown,
            UpsideDown,
            CornerCloser,
            CornerHorizon,
            Flattish,
            POV,
            POVLookDown,
            POVLookDownHigh,
            
            CampfireSide,
            CampfireCloseUp,
            AirborneTilted,
            LowshotCenter,
            OffCenter,
            CircleCenter,
            TreetopPeek,
            PlanetSkyView,
        }

        public sealed class CamAngles
        {
            public Vector3 localPos;
            public Quaternion rot;
            public CamAngles(float localX, float localY, float localZ, float eulerX, float eulerY, float eulerZ)
            {
                this.localPos = new Vector3(localX, localY, localZ);
                this.rot = Quaternion.Euler(eulerX, eulerY, eulerZ);
            }
        }

        public static Dictionary<TitleCamAngle, CamAngles> angles = new Dictionary<TitleCamAngle, CamAngles>
            {
                { TitleCamAngle.Default, new CamAngles(-67.1f, 104.5f, 52.9f, 342.012f, 116.613f, 325.4727f) },
                /*{ TitleCamAngle.CampfireTest, new CamAngles(-42.2091f, 94.6458f, 38.4818f, 349.4323f, 116.6129f, 7.6f) },
                { TitleCamAngle.POVTest, new CamAngles(-14.7362f, 97.3187f, 10.2636f, 10.6552f, 142.3454f, 3.5272f) },*/

                { TitleCamAngle.AboveFire, new CamAngles(-12.3362f, 97.6999f, 4.7181f, 8.3024f, 79.304f, 355.6121f) }, // Above Fire
                { TitleCamAngle.AboveFireLookDown, new CamAngles(-11.9726f, 112.0998f, 4.7181f, 85.8154f, 81.8863f, 351.9395f) }, // Above Fire Look Down

                { TitleCamAngle.UpsideDown, new CamAngles(-26.4671f, 131.8737f, 53.6042f, 17.1353f, 178.5038f, 220.5991f) }, // Upside-Down View
                { TitleCamAngle.CornerCloser, new CamAngles(-53.6937f, 103.4794f, 11.9965f, 353.4547f, 78.6908f, 350.7633f) }, // Corner Closer
                { TitleCamAngle.CornerHorizon, new CamAngles(-53.6937f, 87.3521f, 11.9965f, 331.9981f, 79.255f, 350.7633f) }, // Corner On the Horizon

                { TitleCamAngle.Flattish, new CamAngles(-45.9543f, 96.7013f, 12.4273f, 359.1464f, 82.1276f, 3.5272f) }, // Flattish Perspective

                { TitleCamAngle.POV, new CamAngles(-14.9544f, 94.1735f, 10.5f, 322.9446f, 153.1817f, 3.5272f) }, // POV Sitting at Camp
                { TitleCamAngle.POVLookDown, new CamAngles(-14.9544f, 104.2826f, 10.5f, 42.6182f, 154.1637f, 3.5272f) }, // POV Looking Down Slightly
                { TitleCamAngle.POVLookDownHigh, new CamAngles(-14.9908f, 126.0283f, 12.3909f, 60.0731f, 153.0911f, 3.5272f) }, // POV Looking Down from Higher Angle

                { TitleCamAngle.CampfireSide, new CamAngles(-30.0f, 100.0f, 25.0f, 10.0f, 100.0f, 0f) }, // Campfire side look
                { TitleCamAngle.CampfireCloseUp, new CamAngles(-13.2f, 95.1f, 8.5f, 5.0f, 140.0f, 0f) }, // Very close to the fire
                { TitleCamAngle.AirborneTilted, new CamAngles(-30f, 130f, 30f, 60f, 120f, 45f) }, // High up, tilted like a drone shot
                { TitleCamAngle.LowshotCenter, new CamAngles(-44.5f, 97.8f, 10.2f, 10f, 100f, 0f) }, // Low view on center with a slight tilt
                { TitleCamAngle.OffCenter, new CamAngles(-60f, 105f, 0f, 5f, 70f, 0f) }, // Shows whole planet with campfire on horizon
                { TitleCamAngle.CircleCenter, new CamAngles(-28.5f, 89f, 20f, 355f, 180f, 0f) }, // On ground like you are walking in a circle around the centerx
                { TitleCamAngle.TreetopPeek, new CamAngles(10f, 115f, 20f, 20f, 240f, 0f) }, // Peeking from the treetops down at the fire
                { TitleCamAngle.PlanetSkyView, new CamAngles(-25, 150, 0, 90f, 0f, 0f) }, // Straight up, showing planet from far above
            };

        public static GameObject optionsObj { get; private set; }
        CamAngles defaultAngle = angles[TitleCamAngle.Default];
        Transform camera;
        TitleCamAngle angle;
        Vector3 startingLocalPos;
        Quaternion startingRot;
        TitleScreenAnimation titleAnimator;
        GameObject riebeck;
        public TitleScreenVariation()
        {
            //NH can mess with title screen, so check Riebeck is still there
            riebeck = SearchUtilities.Find("Traveller_HEA_Riebeck (1)");

            optionsObj = SearchUtilities.Find("TitleMenu/OptionsCanvas/OptionsMenu-Panel");

            var cam = SearchUtilities.Find("Scene/Background/CameraSocket");
            if (cam == null) return;

            camera = cam.transform;

            var keys = angles.Keys.ToList();
            int rand = Random.Range(1, keys.Count);
            angle = keys[rand];

            startingLocalPos = camera.localPosition;
            startingRot = camera.rotation;

            titleAnimator = Object.FindObjectOfType<TitleScreenAnimation>();

            Log.Print($"Title Perspective: {rand}");
        }

        public override void Update()
        {
            if (!Settings.TitleVariety)
            {
                return;
            }

            if (riebeck == null) return;
            if (!riebeck.activeInHierarchy) return; //Riebeck deactivated, assume NH changed planet.

            //if (finished) return;
            if (camera == null) return;
            if (titleAnimator._animator == null) return;
            if (!titleAnimator._animator.enabled) return;

            float t = titleAnimator._animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            t = Mathf.Clamp01(Mathf.InverseLerp(0.6f, 1f, t));
            t = ModMain.Smooth(t); //Smoothing function
            UpdateCamera(t);
        }

        public void SetCameraAngle(TitleCamAngle angle)
        {
            this.angle = angle;
            UpdateCamera(1);
        }

        public void UpdateCamera(float t)
        {
            var currentAngle = angles[angle];
            camera.localPosition = Vector3.Lerp(startingLocalPos, currentAngle.localPos, t);
            camera.rotation = Quaternion.Lerp(startingRot, currentAngle.rot, t);
        }
    }
}