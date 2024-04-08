using UnityEngine;

namespace GeneralEnhancements
{
    public sealed class JetpackColor : Feature
    {
        public static float greenFuel { get; set; } //insert g-fuel joke here
        static PlayerResources resources;
        static Material thrusterMat;
        static Color normalColor;
        //static Color colorO2Thrust;
        static Color greenColor;
        static int propID_color;
        public JetpackColor()
        {
            greenFuel = 0f;
            resources = Object.FindObjectOfType<PlayerResources>();
            propID_color = Shader.PropertyToID("_Color");


            if (thrusterMat == null)
            {
                //Make jetpack new mat so ship color doesn't change.
                thrusterMat = resources._jetpackFlameColorSwapper._thrusterRenderers[0].material;
                normalColor = thrusterMat.color;
                greenColor = new Color(0f, 1f, 0.7f) * thrusterMat.color;
                //colorO2Thrust = new Color(0f, 1f, 0.7f) * thrusterMat.color;
            }
            else
            {
                thrusterMat.SetColor(propID_color, normalColor); //Fixed not resetting
            }

            foreach (var item in resources._jetpackFlameColorSwapper._thrusterRenderers)
            {
                item.sharedMaterial = thrusterMat;
            }
        }
        public override void OnSettingsUpdate()
        {
            if (Settings.Jetpack == JetpackColorSetting.Disabled)
            {
                var swapper = resources._jetpackFlameColorSwapper;
                for (int l = 0; l < swapper._thrusterLights.Length; l++)
                {
                    swapper._thrusterLights[l].color = swapper._baseLightColor;
                }
                thrusterMat.SetColor(propID_color, normalColor);
            }
        }
        public static void AddGreenFuel()
        {
            greenFuel += (PlayerResources._maxFuel - resources.GetFuel()) * 1.2f; //Mult to account for transition back.
            greenFuel = Mathf.Min(PlayerResources._maxFuel, greenFuel);
            //resources._jetpackFlameColorSwapper.SetFlameColor(true);
        }
        public override void Update()
        {
            if (Settings.Jetpack == JetpackColorSetting.Disabled) return;
            if (Settings.Jetpack == JetpackColorSetting.Always_Green) greenFuel = 25f;

            if (greenFuel <= 0f) return;

            float magnitude = resources._jetpackThruster.GetLocalAcceleration().magnitude;
            if (resources._isRefueling)
            {

            }
            else if (magnitude > 0f && (!resources._fluidDetector.InFluidType(FluidVolume.Type.WATER) || resources._jetpackThruster.IsBoosterFiring()))
            {
                float thrust = Mathf.Min(magnitude, resources._jetpackThruster.GetMaxTranslationalThrust() * 2f) * 0.1f;
                if (resources._invincible || PlayerState.IsInsideTheEye()) thrust = 0f;

                greenFuel -= thrust * Time.deltaTime;

                float f = Mathf.Min(resources.GetFuel(), PlayerResources._maxFuel * 0.2f); //So that stays green if all green, else fade to normal
                f = Mathf.Max(f, 0.00001f);
                float t = Mathf.Clamp01(greenFuel / f);

                var swapper = resources._jetpackFlameColorSwapper;
                for (int l = 0; l < swapper._thrusterLights.Length; l++)
                {
                    swapper._thrusterLights[l].color = Color.Lerp(swapper._baseLightColor, swapper._thrusterLightsSwapColor, t);
                }

                t = Mathf.Pow(t, 0.4f);
                var color = Color.Lerp(normalColor, greenColor, t);
                thrusterMat.SetColor(propID_color, color);
            }

            if (greenFuel < 0f)
            {
                greenFuel = 0f;

                var swapper = resources._jetpackFlameColorSwapper;
                for (int l = 0; l < swapper._thrusterLights.Length; l++)
                {
                    swapper._thrusterLights[l].color = swapper._baseLightColor;
                }
                thrusterMat.SetColor(propID_color, normalColor);

                /*
                if (resources._usingOxygenAsPropellant)
                {
                    Log.Success("O2");

                    var colorO2Light = swapper._baseLightColor * 0.5f;
                    for (int l = 0; l < swapper._thrusterLights.Length; l++) {
                        swapper._thrusterLights[l].color = colorO2Light;
                    }
                    thrusterMat.SetColor(propID_color, colorO2Thrust);
                }
                else
                {
                    Log.Success("Normal");


                }
                */
            }
        }
    }
}