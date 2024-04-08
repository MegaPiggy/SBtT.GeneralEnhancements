using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GeneralEnhancements
{
    public sealed class TitleScreenVariation : Feature
    {
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

        public static GameObject optionsObj { get; private set; }
        //CamAngles defaultAngle;
        Transform camera;
        CamAngles angle;
        Vector3 startingLocalPos;
        Quaternion startingRot;
        TitleScreenAnimation titleAnimator;
        GameObject riebeck;
        public TitleScreenVariation()
        {
            //NH can mess with title screen, so check Riebeck is still there
            riebeck = GameObject.Find("Traveller_HEA_Riebeck (1)");

            optionsObj = GameObject.Find("TitleMenu/OptionsCanvas/OptionsMenu-Panel");

            var cam = GameObject.Find("Scene/Background/CameraSocket");
            if (cam == null) return;

            camera = cam.transform;
            var angles = new List<CamAngles>();
            //angles.Add(new CamAngles(-42.2091f, 94.6458f, 38.4818f, 349.4323f, 116.6129f, 7.6f));
            //angles.Add(new CamAngles(-14.7362f, 97.3187f, 10.2636f, 10.6552f, 142.3454f, 3.5272f));

            //defaultAngle = new CamAngles(-67.1f, 104.5f, 52.9f, 342.012f, 116.613f, 325.4727f); //Wrong?
            //angles.Add(defaultAngle); //Default

            angles.Add(new CamAngles(-12.3362f, 97.6999f, 4.7181f, 8.3024f, 79.304f, 355.6121f)); //Above Fire
            angles.Add(new CamAngles(-11.9726f, 112.0998f, 4.7181f, 85.8154f, 81.8863f, 351.9395f)); //Above Fire Look down

            angles.Add(new CamAngles(-26.4671f, 131.8737f, 53.6042f, 17.1353f, 178.5038f, 220.5991f)); //Upside-down

            angles.Add(new CamAngles(-53.6937f, 103.4794f, 11.9965f, 353.4547f, 78.6908f, 350.7633f)); //Corner Closer
            angles.Add(new CamAngles(-53.6937f, 87.3521f, 11.9965f, 331.9981f, 79.255f, 350.7633f)); //Corner On the Horizon
            
            angles.Add(new CamAngles(-45.9543f, 96.7013f, 12.4273f, 359.1464f, 82.1276f, 3.5272f)); //Flattish

            angles.Add(new CamAngles(-14.9544f, 94.1735f, 10.5f, 322.9446f, 153.1817f, 3.5272f)); //POV
            angles.Add(new CamAngles(-14.9544f, 104.2826f, 10.5f, 42.6182f, 154.1637f, 3.5272f)); //Look down
            angles.Add(new CamAngles(-14.9908f, 126.0283f, 12.3909f, 60.0731f, 153.0911f, 3.5272f)); //Look down high

            int rand = Random.Range(0, angles.Count);
            angle = angles[rand];

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
            camera.localPosition = Vector3.Lerp(startingLocalPos, angle.localPos, t);
            camera.rotation = Quaternion.Lerp(startingRot, angle.rot, t);
        }
    }
}