using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GeneralEnhancements
{
    public sealed class GiantsDeepDarkSide : Feature
    {
        Light ambientLight;
        Color originalAmbientLightColor;
        float originalAmbientLightIntensity;
        GameObject clouds;
        Material[] cloudMats;
        int propID_Color;
        public GiantsDeepDarkSide()
        {

        }
        public override void LateInitialize()
        {
            if (Locator._giantsDeep == null) return;

            clouds = GameObject.Find("GiantsDeep_Body/Sector_GD/Clouds_GD");
            cloudMats = clouds.transform.Find("CloudsBottomLayer_GD").GetComponent<TessellatedSphereRenderer>()._materials;

            ambientLight = GameObject.Find("GiantsDeep_Body/AmbientLight_GD").GetComponent<Light>();
            originalAmbientLightColor = ambientLight.color;
            originalAmbientLightIntensity = ambientLight.intensity;

            propID_Color = Shader.PropertyToID("_Color");

            //Perhaps fade out on night side, and fade back in as raise altitude.
            //Use dithering somehow?

            //Also is AmbientLight_GD_Interior but doesn't seem to do anything??
        }
        public override void Update()
        {
            if (Locator._giantsDeep == null) return;
            if (Locator.GetSunTransform() == null) return;

            if (!PlayerState.InGiantsDeep() || !Settings.GiantsDeepDarkSide)
            {
                ambientLight.intensity = originalAmbientLightIntensity;
                foreach (var m in cloudMats) {
                    m.SetColor(propID_Color, Color.white);
                }
                return;
            }

            var gdPos = Locator._giantsDeep.transform.position;
            var dayDir = (Locator.GetSunTransform().position - gdPos).normalized;
            var playerVec = Locator.GetPlayerTransform().position - gdPos;
            var playerDir = playerVec.normalized;
            var dayDot = Vector3.Dot(dayDir, playerDir);

            float fullDay = 0.25f;
            float fullNight = -0.25f;
            var nightDay01 = Mathf.Clamp01(Mathf.InverseLerp(fullNight, fullDay, dayDot));

            //0.45
            ambientLight.intensity = Mathf.Lerp(0.5f, originalAmbientLightIntensity, nightDay01);
            foreach (var m in cloudMats) {
                m.SetColor(propID_Color, Color.Lerp(Color.black, Color.white, nightDay01));
            }
        }
    }
}