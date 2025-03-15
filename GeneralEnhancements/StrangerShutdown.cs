using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GeneralEnhancements
{
    /// <summary>
    /// Code used as a starting point: Power Failure by LivingFray
    /// https://outerwildsmods.com/mods/powerfailure/
    /// </summary>
    public sealed class StrangerShutdown : Feature
    {
        RingWorldController ringWorldController;
        RingWorldScreenController ringWorldScreenController;

        OWEmissiveRenderer ringWorldSunRenderer;
        Light ringWorldSunLight;
        Light ringWorldAmbientLight;
        PlanetaryFogController fogController;
        float lightMaxIntensity;
        float ambientLightMaxIntensity;
        float maxFog;

        Color sunColor;
        Color ambientSunColor;
        Color ambientLightColor;

        Color sunlightColor;
        Color sunlightSetColor;

        CloakFieldController cloak;
        CloakingFieldProxy[] cloakProxies;

        bool finishedRevealSequence;

        public StrangerShutdown()
        {
            ringWorldController = GameObject.FindObjectOfType<RingWorldController>();
            if (ringWorldController == null) {
                Log.Print("Stranger not found. It's alright though.");
                return;
            }

            ringWorldScreenController = GameObject.FindObjectOfType<RingWorldScreenController>();
            ringWorldSunRenderer = SearchUtilities.Find("Sector_RingInterior/Geometry_RingInterior/Structure_IP_ArtificialSun/ArtificialSun_Bulb").GetComponent<OWEmissiveRenderer>();
            ringWorldSunLight = SearchUtilities.Find("Sector_RingInterior/Lights_RingInterior/IP_SunLight").GetComponent<Light>();
            ringWorldAmbientLight = SearchUtilities.Find("Sector_RingInterior/Lights_RingInterior/AmbientLight_IP_Surface").GetComponent<Light>();
            fogController = ringWorldController.GetComponentInChildren<PlanetaryFogController>();

            lightMaxIntensity = ringWorldSunLight.intensity;
            ambientLightMaxIntensity = ringWorldAmbientLight.intensity;
            maxFog = fogController.fogDensity;

            sunColor = ringWorldSunRenderer.GetOriginalEmissionColor();
            sunlightColor = ringWorldSunLight.color;
            ambientLightColor = ringWorldAmbientLight.color;

            ambientSunColor = new Color(0.01f, 0.03f, 0.06f);
            sunlightSetColor = new Color(1f, 0.4f, 0.2f);

            //var emberTwinAmbientLight = SearchUtilities.Find("").GetComponent<Light>();

            cloak = ringWorldController.GetComponentInChildren<CloakFieldController>();
            //GameObject.Instantiate(strangerLOD);
            cloakProxies = GameObject.FindObjectsOfType<CloakingFieldProxy>();
        }
        public override void OnSettingsUpdate()
        {
            if (ringWorldController == null) return;
            if (!Settings.StrangerShutdown)
            {
                ringWorldSunLight.enabled = true;
                UpdateStrangerState(0f);
                foreach (var screenRenderers in ringWorldScreenController._screenRenderers) {
                    screenRenderers.SetMaterialProperty(RingWorldScreenController.s_propID_screenFlicker, 1f);
                }
                if (!PlayerState.InCloakingField()) DoRevealSequence(ringWorldController.departTime + 20f);
            }
        }

        public override void Update()
        {
            if (ringWorldController == null) return;
            if (!Settings.StrangerShutdown) return;

            //(departTime = 7 min, damBreakTime = 13 min, _lighthouseCollapseTime = 20 min)
            float loopTime = TimeLoop.GetSecondsElapsed();

            if (!PlayerState.InCloakingField())  //Temporary disable of cloaking field when flicker happens
            {
                if (!finishedRevealSequence) DoRevealSequence(loopTime);

                return; //Don't bother if not even there.
            }

            if (loopTime < ringWorldController.damBreakTime) return;

            UpdateStrangerState(loopTime);
        }

        void UpdateStrangerState(float loopTime)
        {
            float screens01 = 1f;
            if (loopTime > 1240f)
            {
                screens01 = Mathf.Clamp01((loopTime - 1200f) / 190f); //Like is timer to end of loop instead of just awkward waiting after music.
                screens01 = 1f - screens01;
                screens01 = Mathf.Pow(screens01, 3f);

                foreach (var screenRenderers in ringWorldScreenController._screenRenderers)
                {
                    screenRenderers.SetMaterialProperty(RingWorldScreenController.s_propID_screenFlicker, screens01);
                }
            }

            float ambientLight01 = 1f - Mathf.Clamp01((loopTime - 840f) / 360f); //14-20 minutes
            float ambientLevel = Mathf.Pow(ambientLight01, 0.5f);
            float lightColor01 = 1f - Mathf.Clamp01((loopTime - ringWorldController.damBreakTime) / 60f);
            float light01 = 1f - Mathf.Clamp01((loopTime - (ringWorldController.damBreakTime + 60f)) / 60f);

            float lightLevel = Mathf.Pow(light01, 0.35f);

            fogController.fogDensity = maxFog * Mathf.Lerp(Mathf.Lerp(0.08f, 0.6f, screens01), 1f, ambientLevel);

            ringWorldSunRenderer.SetEmissionColor(Color.Lerp(ambientSunColor, Color.Lerp(sunlightSetColor, sunColor, lightColor01), lightLevel) * ambientLevel);

            float ambientDarkenBounce = Mathf.Pow(Mathf.Max(0.3f, Mathf.Abs(light01 - 0.5f) * 2f), 0.35f);
            ringWorldAmbientLight.intensity =
                ambientLightMaxIntensity * Mathf.Lerp(Mathf.Lerp(0f, 0.2f, screens01), 1f, ambientLevel) * ambientDarkenBounce;

            //Doesn't work -> cubemap -> just dimming ambient briefly instead.
            //ringWorldAmbientLight.color = Color.Lerp(sunlightSetColor, ambientLightColor, ambientSunsetBounce);

            if (lightLevel <= Mathf.Epsilon && !ringWorldSunLight.enabled)
            {
                //Sound?
                ringWorldSunLight.enabled = false;
            }
            else
            {
                ringWorldSunLight.color = Color.Lerp(sunlightSetColor, sunlightColor, lightColor01);
                ringWorldSunLight.intensity = lightLevel * lightMaxIntensity;
            }
        }

        void DoRevealSequence(float loopTime)
        {
            if (ModCompatibility.hasVisibleStranger)
            {
                finishedRevealSequence = true;
                return;
            }

            if (loopTime > ringWorldController.departTime + 10f)
            {
                foreach (var cloakProxy in cloakProxies)
                {
                    cloakProxy.OnPlayerExitCloakingField();
                }
                finishedRevealSequence = true;
            }
            else if (loopTime > ringWorldController.departTime)
            {
                float v = Mathf.Clamp01((loopTime - ringWorldController.departTime) / 10f);
                v = Mathf.Abs(v - 0.5f) * 2f;

                if (UnityEngine.Random.value > v)
                {
                    foreach (var cloakProxy in cloakProxies) cloakProxy.OnPlayerEnterCloakingField();
                }
                else
                {
                    foreach (var cloakProxy in cloakProxies) cloakProxy.OnPlayerExitCloakingField();
                }
            }
        }
    }
}