using GeneralEnhancements.DebugStuff;
using OWML.Common;
using OWML.ModHelper;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace GeneralEnhancements
{
    public sealed class ModMain : ModBehaviour
    {
        public static bool isDevelopmentVersion => false;
        public static bool IsLoaded { get; private set; }
        public static bool HasRootAccess { get; private set; }
        static InitializeState initialized = InitializeState.Not;
        public enum InitializeState
        {
            Not, Created, Initialized
        }

        public override object GetApi() => new GeneralEnhancementsApi();

        DebugControls debugControls;
        Feature[] features;
        GEAssets assets;
        TitleScreenVariation titleScreenVariation;
        OWScene currentScene = OWScene.None;
        void Start()
        {
            //---------------- Start Up ----------------//
            Log.Initialize(ModHelper.Console);
            if (IsLoaded)
            {
                Log.Warning($"Error: Multiple Instances of General Enhancements Detected.");
                return;
            }
            else Log.Success($"General Enhancements Loaded!");

            IsLoaded = true;
            if (isDevelopmentVersion)
            {
                Log.Error("Debug activated. THIS MESSAGE SHOULD NOT APPEAR!");
                debugControls = new DebugControls(ModHelper);
            }
            HasRootAccess = ModHelper.Interaction.ModExists("2walker2.OWJam4ModProject");

            ModHelper.Events.Unity.FireOnNextUpdate(() => OnLoadScene(SceneManager.GetActiveScene()));
            LoadManager.OnCompleteSceneLoad += OnLoadScene;

            assets = new GEAssets(ModHelper.Assets.LoadBundle("gebundle"));

            ModCompatibility.Initialize(this);
            Patches.SetUp(this);
            Settings.UpdateSettings(ModHelper.Config);
        }

        void OnLoadScene(Scene scene)
        {
            if (scene.name == "TitleScreen")
            {
                currentScene = OWScene.TitleScreen;
                ClearCaches();
                OnTitleScreenLoad();
            }
        }
        void OnLoadScene(OWScene scene, OWScene loadScene)
        {
            currentScene = loadScene;
            ClearCaches();
            if (loadScene == OWScene.TitleScreen) OnTitleScreenLoad();
            else if (loadScene == OWScene.SolarSystem || loadScene == OWScene.EyeOfTheUniverse) OnGameSceneLoaded();
            else OnOtherSceneLoaded();
        }
        void OnGameSceneLoaded()
        {
            initialized = InitializeState.Not;
            features = new Feature[]
            {
                new ContinuousMatchVelocity(),
                new StrangerShutdown(),
                new SkipToShip(),
                new KeyboardThrottleControl(),
                new SkipFlashbackButton(),
                new AdvancedMinimap(),
                new JetpackColor(),
                new ManualBlinking(),
                new HUDModification(),
                new NotUglyDreamworldPlanet(),
                new GiantsDeepDarkSide(),
                new MiscFeatures()
            };
            initialized = InitializeState.Created;

            Log.Print($"Loop {TimeLoop.GetLoopCount()}");
        }
        void OnOtherSceneLoaded()
        {
            initialized = InitializeState.Not;
            features = new Feature[]
            {
            };
        }
        void OnTitleScreenLoad()
        {
            var obj = SearchUtilities.Find("OW_Logo_Anim/OW_Logo_Anim/WILDS");
            if (obj != null) titleWILDSParent = obj.transform;

            titleScreenVariation = new TitleScreenVariation();
        }

        private void ClearCaches()
        {
            // Caches of GameObjects must always be cleared
            SearchUtilities.ClearCache();
        }

        public static Transform titleWILDSParent { get; private set; }

        void Update() //I changed it, you happy John?
        {
            if (currentScene == OWScene.TitleScreen)
            {
                titleScreenVariation.Update();
                return;
            }
            if (Locator.GetPlayerBody() == null) return; //Expects at least player to exist.

            if (initialized == InitializeState.Not) return;
            if (initialized == InitializeState.Created)
            {
                initialized = InitializeState.Initialized;
                foreach (var feature in features) feature.LateInitialize();
                return;
            }
            if (OWTime.IsPaused(OWTime.PauseType.Menu)) return;

            foreach (var feature in features) feature.Update();

            if (isDevelopmentVersion) {
                debugControls.OnUpdate();
            }
        }

        public override void Configure(IModConfig config)
        {
            Settings.UpdateSettings(config);

            if (currentScene == OWScene.TitleScreen)
            {
                titleScreenVariation.OnSettingsUpdate();
                return;
            }

            if (initialized == InitializeState.Initialized)
            {
                foreach (var feature in features) feature.OnSettingsUpdate();
            }
        }

        public static float Smooth(float t) => t * t * (3f - 2f * t);
    }
}