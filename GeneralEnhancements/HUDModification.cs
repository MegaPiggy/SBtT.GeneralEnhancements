using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace GeneralEnhancements
{
    public sealed class HUDModification : Feature
    {
        Signalscope signalscope;
        PlayerResources resources;
        LineRenderer waveform;
        Image waveformLineStart, waveformLineEnd;
        Color waveformLineOriginalColor;
        //Transform throttle; Image throttleBg, throttleArrow;
        Material ticksMat;

        GameObject helmetFrame, reticule;
        Transform[] helmetEffects;

        Transform resourceGaugeRoot, healthSilhouette;
        Text vitalsText, healthNumber;
        Image fuelBar, o2Bar, boostBackground;
        Color originalFuelColor, originalO2Color, originalBoostBgColor;
        Transform gForce, gravityText, thruster, thrusterAxis;
        Vector3 originalGForcePos, originalThrusterPos;
        static Text gForceNumber, frequencyLabel;
        Color originalGForceNumberColor, originalFrequencyLabelColor;

        public static Text hudTextTemplate => gForceNumber;

        public HUDModification()
        {
            signalscope = Object.FindObjectOfType<Signalscope>();
            resources = Object.FindObjectOfType<PlayerResources>();

            var uiCanvas = SearchUtilities.Find("HelmetOnUI/UICanvas").transform;
            resourceGaugeRoot = uiCanvas.Find("GaugeGroup");
            healthNumber = resourceGaugeRoot.Find("Health").GetComponentInChildren<Text>(); //Unused but still there
            vitalsText = resourceGaugeRoot.Find("VitalsText").GetComponent<Text>();
            healthSilhouette = resourceGaugeRoot.Find("HealthSilhouette");

            var secondaryGroup = uiCanvas.Find("SecondaryGroup");
            gForce = secondaryGroup.Find("GForce");
            gForceNumber = gForce.Find("NumericalReadout").GetComponent<Text>();
            gravityText = gForceNumber.transform.Find("GravityText");

            thruster = secondaryGroup.Find("HUD_Thrusters");
            thrusterAxis = thruster.Find("Axis");
            ticksMat = thruster.Find("Ticks").GetComponentInChildren<Renderer>().sharedMaterial;

            var guages = resourceGaugeRoot.Find("Gauge");
            var oxygen = guages.Find("Oxygen");
            var fuel = guages.Find("Fuel");
            /*
            var boost = guages.Find("Boost");
            throttle = GameObject.Instantiate(boost).transform;
            throttle.parent = boost.parent;
            throttle.localPosition = boost.localPosition;
            throttle.localRotation = Quaternion.Euler(0f, 0f, 45f);
            throttle.localScale = new Vector3(-1f, -1f, 1f);
            throttleArrow = throttle.Find("BoostArrowIndicator").GetComponent<Image>();
            */

            fuelBar = fuel.Find("FuelBar").GetComponent<Image>();
            o2Bar = oxygen.Find("O2Bar").GetComponent<Image>();
            originalFuelColor = fuelBar.color;
            originalO2Color = o2Bar.color;
            //var boostBar = boost.Find("BoostBar");

            //var resourcesBackground = gaugeGroup.Find("BackgroundFuelO2"); //Image: 1, 0.5, 0, 0.5
            //1f 0f 0f 0.3f

            boostBackground = resourceGaugeRoot.Find("BackgroundBoost").GetComponent<Image>();
            originalBoostBgColor = boostBackground.color;

            /*
            var throttleBgObj = Object.Instantiate(boostBackground).transform;
            throttleBgObj.parent = boostBackground.parent;
            throttleBgObj.localPosition = boostBackground.localPosition;
            throttleBgObj.localRotation = Quaternion.Euler(0f, 0f, 255f);
            var s = boostBackground.localScale; s.y = -s.y;
            throttleBgObj.localScale = s;
            throttleBg = throttleBgObj.GetComponent<Image>();
            */

            //var gaugeTexts = gaugeGroup.GetComponentsInChildren<Text>();
            //var gaugeImages = gaugeGroup.GetComponentsInChildren<Image>();

            originalGForcePos = gForce.localPosition;
            originalThrusterPos = thruster.localPosition;

            originalGForceNumberColor = gForceNumber.color;

            var signalScopeHelmetUI = uiCanvas.Find("SigScopeDisplay");
            frequencyLabel = signalScopeHelmetUI.Find("FrequencyLabel").GetComponent<Text>();
            originalFrequencyLabelColor = frequencyLabel.color;

            var line = signalScopeHelmetUI.Find("SignalscopeLine");
            waveform = line.Find("WaveformRenderer").GetComponent<LineRenderer>();

            waveformLineStart = line.Find("BoundLine_Left").GetComponent<Image>();
            waveformLineEnd = line.Find("BoundLine_Right").GetComponent<Image>();
            waveformLineOriginalColor = waveformLineStart.color;

            var helmetRoot = SearchUtilities.Find("HelmetRoot/HelmetMesh").transform;
            helmetFrame = helmetRoot.Find("HUD_Helmet_v2").gameObject;

            helmetEffects = new Transform[] {
                helmetRoot.Find("HelmetRainDroplets"),
                helmetRoot.Find("HelmetRainStreaks"),
                //helmetRoot.Find("HelmetVisorEffects"),
                helmetRoot.Find("HUD_HelmetCracks")
            };

            reticule = SearchUtilities.Find("Reticule/Image");
            //0.6 0.2 0.1 1

            OnSettingsUpdate();
        }
        public override void OnSettingsUpdate()
        {
            //--------------------------------------------- Resource Gauge ---------------------------------------------//
            if (Settings.ResourceGauges == ResourceGauges.Disabled)
            {
                resourceGaugeRoot.gameObject.SetActive(false);
            }
            else
            {
                resourceGaugeRoot.gameObject.SetActive(true);

                bool useUnused = Settings.ResourceGauges == ResourceGauges.Unused;
                healthSilhouette.gameObject.SetActive(!useUnused);
                healthNumber.transform.parent.gameObject.SetActive(useUnused);
                vitalsText.gameObject.SetActive(useUnused);

                if (Settings.ResourceGauges == ResourceGauges.Vanilla)
                {

                    fuelBar.color = originalFuelColor;
                    o2Bar.color = originalO2Color;
                    boostBackground.color = originalBoostBgColor;
                }
                else
                {

                    fuelBar.color = new Color(1f, 0.825f, 1f);
                    o2Bar.color = new Color(0.5f, 0.6f, 1f);
                    boostBackground.color = new Color(1f, 0.5f, 0.5f, 0.498f);
                }
            }

            //--------------------------------------------- Gravity Thruster Gauge ---------------------------------------------//
            if (Settings.GravityThrusterGauge == GravityThrusterGauges.Disabled)
            {
                gForce.gameObject.SetActive(false);
                thruster.gameObject.SetActive(false);
            }
            else
            {
                gForce.gameObject.SetActive(true);
                thruster.gameObject.SetActive(true);

                if (Settings.GravityThrusterGauge == GravityThrusterGauges.Vanilla)
                {
                    gravityText.gameObject.SetActive(true);
                    thrusterAxis.gameObject.SetActive(true);

                    gForce.localPosition = originalGForcePos;
                    thruster.localPosition = originalThrusterPos;

                    gForceNumber.color = originalGForceNumberColor;
                    frequencyLabel.color = originalFrequencyLabelColor;
                }
                else
                {
                    gravityText.gameObject.SetActive(false);
                    thrusterAxis.gameObject.SetActive(false);

                    var combinedPos = new Vector3(217.5f, 221f, 0f);
                    gForce.localPosition = combinedPos;
                    thruster.localPosition = combinedPos;

                    gForceNumber.color = new Color(1f, 0.6f, 0.4f);
                    frequencyLabel.color = new Color(1f, 0.5f, 0f, 1f);
                }
            }

            reticule.SetActive(Settings.Reticule);
            helmetFrame.SetActive(Settings.Helmet);
            foreach (var effect in helmetEffects) effect.gameObject.SetActive(Settings.HelmetEffects);
        }
        public override void Update()
        {
            if (signalscope != null && signalscope.IsEquipped())
            {
                UpdateSignalscopeColors();
            }

            if (healthNumber != null && healthNumber.gameObject.activeSelf)
            {
                healthNumber.text = Mathf.Max(0f, resources.GetHealth()).ToString("F1");
                float t = (resources.GetHealthFraction() - 0.2f) * 1.25f;
                t = Mathf.Clamp01(t);
                var color = Color.Lerp(new Color(1f, 0.2f, 0.1f, 1f), new Color(1f, 1f, 1f, 1f), t);
                healthNumber.color = color;
                vitalsText.color = color;
            }

            if (Locator.GetPlayerSuit() != null && Locator.GetPlayerSuit().IsWearingSuit())
            {
                ticksMat.color = Color.Lerp(new Color(1f, 0.2f, 0f, 1f), Color.white, KeyboardThrottleControl.PlayerThrottle);
            }

            /*
            if (!initialized) return;

            bool uiActive = Settings.AllowThrottleScroll && !OWInput.UsingGamepad();
            throttleBg.enabled = uiActive;
            throttleArrow.enabled = uiActive;
            if (!uiActive) return;

            if (Locator.GetPlayerSuit().IsWearingSuit())
            {
                throttleArrow.transform.localRotation = Quaternion.Euler(KeyboardThrottleControl.PlayerThrottle * 90f, 0f, 0f);
            }
            */
        }
        void UpdateSignalscopeColors()
        {
            switch (signalscope.GetFrequencyFilter())
            {
                case SignalFrequency.Traveler: SetColors(new Color(1f, 0f, 0.1f), new Color(1f, 0.1f, 0f)); break;
                case SignalFrequency.Quantum: SetColors(new Color(1f, 0.3f, 1f), new Color(0.6f, 0.7f, 1f)); break;
                case SignalFrequency.EscapePod: SetColors(new Color(1.25f, 0.05f, 0f), new Color(1.25f, 0f, 0f)); break;
                case SignalFrequency.Radio: SetColors(new Color(0f, 0.1f, 0.7f), new Color(0.1f, 0.3f, 0.9f)); break;

                case SignalFrequency.HideAndSeek: SetColors(new Color(1f, 0.1f, 0.2f), new Color(1f, 0.2f, 0.1f)); break;

                case SignalFrequency.Statue: SetColors(new Color(0.2f, 0.2f, 0.7f), new Color(0.3f, 0.35f, 0.9f)); break;
                case SignalFrequency.WarpCore: SetColors(new Color(1.1f, 1f, 1f), new Color(1.1f, 1f, 1f)); break;

                default: SetColors(new Color(1f, 1f, 1f), new Color(1f, 1f, 1f)); break;
            }
        }
        void SetColors(Color endColor, Color startColor)
        {
            //waveformLineEnd.color = endColor; // Color.Lerp(endColor, waveformLineOriginalColor, (endColor.maxColorComponent + 0.5f) * 0.5f);
            waveform.endColor = endColor;
            waveform.startColor = startColor;
            //waveformLineStart.color = startColor; // * waveformLineOriginalColor;
        }
    }
}