using System;
using OWML.Common;

namespace GeneralEnhancements
{
    public enum JetpackColorSetting { Disabled, Enabled, Always_Green }

    public enum MinimapSetting { Disabled, Vanilla, Simple, Advanced }
    public enum GravityThrusterGauges { Disabled, Vanilla, Merged }
    public enum ResourceGauges { Disabled, Vanilla, Unused, Recolored }

    public enum BlinkSleep { Disabled, Blink_Only, Enabled }
    public enum Targeting { Disabled, Vanilla, Smooth }

    public static class Settings
    {
        public static MinimapSetting Minimap { get; private set; }
        public static float GiantsDeepMapScale { get; private set; }
        public static GravityThrusterGauges GravityThrusterGauge { get; private set; }
        public static ResourceGauges ResourceGauges { get; private set; }
        public static bool Helmet { get; private set; }
        public static bool HelmetEffects { get; private set; }
        public static bool Reticule { get; private set; }
        public static Targeting Targeting { get; private set; }

        public static bool TitleVariety { get; private set; }
        public static BlinkSleep BlinkSleep { get; private set; }
        public static bool AllowThrottleScroll { get; private set; }
        public static bool ContinuousMatchVelocity { get; private set; }
        public static JetpackColorSetting Jetpack { get; private set; }
        public static bool GiantsDeepDarkSide { get; private set; }

        public static bool StrangerShutdown { get; private set; }
        public static bool NicerRingedPlanet { get; private set; }

        //public static bool SuitAutoPilot { get; private set; }

        public static void UpdateSettings(IModConfig config)
        {
            Minimap = StringToEnum<MinimapSetting>(config.GetSettingsValue<string>("Minimap"));
            GiantsDeepMapScale = config.GetSettingsValue<float>("Giants Deep Minimap Scale");
            GravityThrusterGauge = StringToEnum<GravityThrusterGauges>(config.GetSettingsValue<string>("Gravity-Thruster Gauge"));
            ResourceGauges = StringToEnum<ResourceGauges>(config.GetSettingsValue<string>("Resource Gauges"));
            Helmet = config.GetSettingsValue<bool>("Helmet");
            HelmetEffects = config.GetSettingsValue<bool>("Helmet Effects");
            Reticule = config.GetSettingsValue<bool>("Reticule");
            Targeting = config.GetSettingsValue<Targeting>("Targeting");

            TitleVariety = config.GetSettingsValue<bool>("Title Variety");
            BlinkSleep = config.GetSettingsValue<BlinkSleep>("Blinking/Sleep Anywhere");
            AllowThrottleScroll = config.GetSettingsValue<bool>("Allow Throttle Scroll");
            ContinuousMatchVelocity = config.GetSettingsValue<bool>("Continuous Velocity Matching");
            Jetpack = StringToEnum<JetpackColorSetting>(config.GetSettingsValue<string>("Jetpack Color"));
            GiantsDeepDarkSide = config.GetSettingsValue<bool>("Giants Deep Dark Side");

            StrangerShutdown = config.GetSettingsValue<bool>("Stranger Shutdown");
            NicerRingedPlanet = config.GetSettingsValue<bool>("Nicer Ringed Planet");
        }

        static T StringToEnum<T>(string e) where T : Enum
        {
            var array = (T[])Enum.GetValues(typeof(T));
            for (int i = 0; i < array.Length; i++)
            {
                if (e == array[i].ToString()) return array[i];
            }

            Log.Error($"Could not parse enum from string: {e}");
            return default;
        }
    }
}