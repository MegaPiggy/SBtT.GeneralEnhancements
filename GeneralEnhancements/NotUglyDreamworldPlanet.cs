using UnityEngine;

namespace GeneralEnhancements
{
    // Streetlights named it this not me - MegaPiggy
    public sealed class NotUglyDreamworldPlanet : Feature
    {
        GameObject atmoObj;
        //GameObject fogObj;

        static GameObject atmoRoot;
        static Material atmoMat;

        //static PlanetaryFogController fog;
        //static Material fogMat;
        //static Texture3D fogTexture;
        //static Texture2D fogRampTexture;

        int _propID_SunPosition, _propID_OWSunPositionRange, _propID_OWSunColorIntensity;
        static Light planetSunLight;
        static Transform sunLightPivot;
        static Transform gasPlanetTF;
        static MeshFilter rings;

        static SunLightParamUpdater[] sunLightParamUpdaters;
        static bool[] enabledStates;

        static GameObject ambientLight;

        Vector3 originalPlanetPosition;
        Quaternion originalPlanetRotation;
        Vector3 originalSunLightPivotPosition;
        Quaternion originalSunLightPivotRotation;
        Vector3 originalPlanetSunLightPosition;
        Quaternion originalPlanetSunLightRotation;
        float originalPlanetSunLightIntensity;
        float originalPlanetSunLightRange;
        private LightType originalPlanetSunLightType;
        private float originalPlanetSunLightSpotAngle;
        static Mesh originalRingsMesh;
        static Color originalRingsColor;

        static OWRigidbody rootAccessPlanetSurface;
        Position position = Position.Original;

        public NotUglyDreamworldPlanet()
        {

        }

        public enum Position
        {
            Original,
            Nicer,
            ShroudedWoodlands,
            StarlitCove,
            EndlessCanyon,
            SubmergedStructure
        }

        private void SetGasPlanetPositionAndRotation(Vector3 position, Vector3 euler) => SetGasPlanetPositionAndRotation(position, Quaternion.Euler(euler));

        private void SetGasPlanetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            gasPlanetTF.localPosition = position;
            gasPlanetTF.localRotation = rotation;
            if (ModMain.HasRootAccess)
            {
                rootAccessPlanetSurface.SetPosition(gasPlanetTF.position);
                rootAccessPlanetSurface.SetRotation(gasPlanetTF.rotation);
            }
        }

        public override void LateInitialize()
        {
            atmoRoot = null;

            var atmoTH = SearchUtilities.Find("TimberHearth_Body/Atmosphere_TH");
            var gasPlanet = SearchUtilities.Find("Sector_DreamWorld/Atmosphere_Dreamworld/Prefab_IP_VisiblePlanet");
            if (atmoTH == null || gasPlanet == null) return;
            gasPlanetTF = gasPlanet.transform;

            if (ModMain.HasRootAccess) rootAccessPlanetSurface = SearchUtilities.Find("PlanetSurface_Body").GetComponent<OWRigidbody>();

            originalPlanetPosition = gasPlanetTF.localPosition;
            originalPlanetRotation = gasPlanetTF.localRotation;

            sunLightParamUpdaters = GameObject.FindObjectsOfType<SunLightParamUpdater>();
            enabledStates = new bool[sunLightParamUpdaters.Length];

            Log.Print($"Updaters LI: {sunLightParamUpdaters.Length}");

            atmoRoot = Object.Instantiate(atmoTH);
            atmoRoot.name = "Atmosphere_IP_VisiblePlanet";

            atmoObj = atmoRoot.transform.Find("AtmoSphere").gameObject; //Can't be scaled
            //fogObj = atmoRoot.transform.Find("FogSphere").gameObject; //Can be scaled
            var planetPivot = gasPlanetTF.Find("VisiblePlanet_Pivot");
            rings = planetPivot.Find("Rings_IP_VisiblePlanet").GetComponent<MeshFilter>();
            originalRingsMesh = rings.sharedMesh;
            originalRingsColor = rings.GetComponent<MeshRenderer>().sharedMaterial.color;
            //Fix rings -> They used a clip shader but also didn't actually really use it?


            float radius = 449.5f;
            /*
            fog = fogObj.GetComponent<PlanetaryFogController>();
            var fogRndr = fogObj.GetComponent<Renderer>();
            if (fogTexture == null)
            {
                fogTexture = fog.fogLookupTexture;
                fogRampTexture = fog.fogColorRampTexture;
            }
            if (fogMat == null)
            {
                fogMat = fogRndr.material;
                //fogMat.SetFloat("_Radius", radius);
                //fogMat.SetFloat("_Density", 1f);
                //fogMat.SetFloat("_DensityExp", 1f);
            }
            */
            if (atmoMat == null)
            {
                atmoMat = atmoObj.GetComponentInChildren<Renderer>().material;
                atmoMat.SetFloat("_InnerRadius", radius);
                atmoMat.SetFloat("_OuterRadius", radius + 75f);
            }

            var renderers = atmoObj.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                r.GetComponent<MeshFilter>().sharedMesh = GEAssets.InvertedAtmosphere; //So appears on top
                r.sharedMaterial = atmoMat;
            }
            //fogRndr.sharedMaterial = fogMat;

            var atmoRootTF = atmoRoot.transform;
            atmoRootTF.parent = gasPlanetTF;
            atmoRootTF.localPosition = Vector3.zero;
            atmoRootTF.localRotation = Quaternion.identity;
            atmoRootTF.localScale = Vector3.one * 1.25f;
            atmoRoot.SetActive(false);

            position = Position.Nicer;
            SetGasPlanetPositionAndRotation(new Vector3(-1600f, 1320f, 333f), new Vector3(0f, 0f, 10f));

            ambientLight = gasPlanetTF.Find("AmbientLight_IP").gameObject;
            sunLightPivot = gasPlanetTF.Find("SunLightPivot");
            originalSunLightPivotPosition = sunLightPivot.localPosition;
            originalSunLightPivotRotation = sunLightPivot.localRotation;
            sunLightPivot.localPosition = Vector3.zero;
            sunLightPivot.localRotation = Quaternion.Euler(0f, 140f, 0f);
            planetSunLight = sunLightPivot.Find("Directional light").GetComponent<Light>();
            originalPlanetSunLightPosition = planetSunLight.transform.localPosition;
            originalPlanetSunLightRotation = planetSunLight.transform.localRotation;
            planetSunLight.transform.localPosition = new Vector3(0f, 0f, 1800f);
            originalPlanetSunLightIntensity = planetSunLight.intensity;
            originalPlanetSunLightRange = planetSunLight.range;
            originalPlanetSunLightType = planetSunLight.type;
            originalPlanetSunLightSpotAngle = planetSunLight.spotAngle;

            _propID_SunPosition = Shader.PropertyToID("_SunPosition");
            _propID_OWSunPositionRange = Shader.PropertyToID("_OWSunPositionRange");
            _propID_OWSunColorIntensity = Shader.PropertyToID("_OWSunColorIntensity");

            GlobalMessenger.RemoveListener("EnterDreamWorld", OnEnterDreamworld);
            GlobalMessenger.AddListener("EnterDreamWorld", OnEnterDreamworld);

            GlobalMessenger.RemoveListener("ExitDreamWorld", OnExitDreamworld);
            GlobalMessenger.AddListener("ExitDreamWorld", OnExitDreamworld);
        }
        static void OnEnterDreamworld()
        {
            Log.Print("Enter Dreamworld");

            bool active = Settings.NicerRingedPlanet;
            if (atmoRoot != null) atmoRoot.SetActive(active);
            if (ambientLight != null) ambientLight.SetActive(!active);
            if (sunLightParamUpdaters != null) {
                for (int i = 0; i < sunLightParamUpdaters.Length; i++)
                {
                    if (sunLightParamUpdaters[i] != null) {
                        enabledStates[i] = sunLightParamUpdaters[i].enabled;
                        sunLightParamUpdaters[i].enabled = !active;
                    }
                }
            }
        }
        static void OnExitDreamworld()
        {
            Log.Print("Exit Dreamworld");

            if (atmoRoot != null) atmoRoot.SetActive(false);
            if (sunLightParamUpdaters != null) {
                for (int i = 0; i < sunLightParamUpdaters.Length; i++)
                {
                    if (sunLightParamUpdaters[i] != null) {
                        sunLightParamUpdaters[i].enabled = enabledStates[i];
                    }
                }
            }
        }

        public override void OnSettingsUpdate()
        {
            if (PlayerState.InDreamWorld())
            {
                OnEnterDreamworld();
            }
        }
        public override void Update()
        {
            if (!PlayerState.InDreamWorld()) return;

            /*
            if (fog != null)
            {
                fog.transform.position = Locator.GetPlayerTransform().position;
            }
            */

            if (Settings.NicerRingedPlanet)
            {
                var sectorsIn = Locator.GetPlayerSectorDetector()._sectorList;
                foreach (var sector in sectorsIn)
                {
                    string n = sector.name;
                    if (!n.Contains("DreamZone")) continue; //Joj Corbroc

                    if (n.EndsWith("1") && position != Position.ShroudedWoodlands)
                    {
                        position = Position.ShroudedWoodlands;
                        SetGasPlanetPositionAndRotation(new Vector3(-1750f, 1320f, -400f), new Vector3(320f, 354f, 31f));
                        break;
                    }
                    if (n.EndsWith("2") && position != Position.StarlitCove)
                    {
                        position = Position.StarlitCove;
                        SetGasPlanetPositionAndRotation(new Vector3(2950f, 1320f, -685f), new Vector3(320f, 91f, 98f));
                        break;
                    }
                    if (n.EndsWith("3") && position != Position.EndlessCanyon)
                    {
                        position = Position.EndlessCanyon;
                        SetGasPlanetPositionAndRotation(new Vector3(-370f, 2080f, -390f), new Vector3(333f, 320f, 340f));
                        break;
                    }
                    if (n.EndsWith("4") && position != Position.SubmergedStructure)
                    {
                        position = Position.SubmergedStructure;
                        SetGasPlanetPositionAndRotation(new Vector3(-1880f, 1080f, 0f), new Vector3(270f, 37f, 0f));
                        break;
                    }
                }
            }
            else if (position != Position.Original)
            {
                position = Position.Original;
                SetGasPlanetPositionAndRotation(originalPlanetPosition, originalPlanetRotation);
            }

            if (sunLightPivot != null)
            {
                if (Settings.NicerRingedPlanet)
                {
                    sunLightPivot.localPosition = Vector3.zero;
                    sunLightPivot.localRotation = Quaternion.Euler(0f, 140f, 0f);
                }
                else
                {
                    sunLightPivot.localPosition = originalSunLightPivotPosition;
                    sunLightPivot.localRotation = originalSunLightPivotRotation;
                }
            }

            if (rings != null)
            {
                if (Settings.NicerRingedPlanet)
                {
                    rings.sharedMesh = GEAssets.HomePlanetRing;
                    rings.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(1.1f, 0.9f, 1f, 0.04f);
                }
                else
                {
                    rings.sharedMesh = originalRingsMesh;
                    rings.GetComponent<MeshRenderer>().sharedMaterial.color = originalRingsColor;
                }
            }

            if (planetSunLight != null)
            {
                if (Settings.NicerRingedPlanet)
                {
                    planetSunLight.transform.localPosition = new Vector3(0f, 0f, 1800f);
                    var lightTF = planetSunLight.transform;
                    Vector3 dir = (lightTF.position - atmoRoot.transform.position).normalized;
                    Vector3 position = atmoRoot.transform.position + dir * 10000f;

                    float w = planetSunLight.range;
                    float range = planetSunLight.range;
                    Color color = Color.white;
                    Shader.SetGlobalVector(_propID_SunPosition, new Vector4(position.x, position.y, position.z, w));
                    Shader.SetGlobalVector(_propID_OWSunPositionRange, new Vector4(position.x, position.y, position.z, 1f / (range * range)));
                    Shader.SetGlobalVector(_propID_OWSunColorIntensity, new Vector4(color.r, color.g, color.b, 1f));

                    lightTF.rotation = Quaternion.LookRotation(-dir);

                    planetSunLight.intensity = 3f;
                    //planetSunLight.intensity = Settings.RingedPlanetBrightness; //Didn't darken atmosphere
                    planetSunLight.range = 2500f;
                    if (planetSunLight.type != LightType.Spot)
                    {
                        planetSunLight.type = LightType.Spot;
                        planetSunLight.spotAngle = 135f;
                    }
                }
                else
                {
                    planetSunLight.transform.localPosition = originalPlanetSunLightPosition;
                    planetSunLight.transform.localRotation = originalPlanetSunLightRotation;
                    planetSunLight.intensity = originalPlanetSunLightIntensity;
                    planetSunLight.range = originalPlanetSunLightRange;
                    planetSunLight.type = originalPlanetSunLightType;
                    planetSunLight.spotAngle = originalPlanetSunLightSpotAngle;
                }
            }
        }
    }
}