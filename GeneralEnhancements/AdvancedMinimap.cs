﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GeneralEnhancements
{
    public sealed class AdvancedMinimap : Feature
    {
        const string MainTex = "_MainTex";

        static Minimap minimap;
        static PlanetoidRuleset current;
        ProxyBody[] proxies;
        //List<ProxyBody> proxyClones;

        public static MinimapPlanetInfo minimapProxy { get; private set; }
        GameObject TimberHearth, EmberTwin, DarkBramble, Comet, BrittleHollow, AshTwin, WhiteHole;
        GameObject THMoon, BHMoon;
        GameObject GiantsDeepRoot;
        List<GDIslandInfo> GDIslands;
        Transform GDQuantumIsland;
        GameObject GDQuantumTower;

        Mesh sphereOldMesh;
        Material sphereOldMat;
        MeshRenderer sphereRenderer;
        Transform sphereTF;
        Transform playerMarker;

        MeshRenderer[] currentRenderers;
        MeshRenderer arrowRenderer;
        static Material dupArrowRenderer;
        MeshRenderer aboveGroundArrowRenderer;

        static Material dupParticleMat;

        Transform sandColumnAshTwin;
        SandLevelController sandAT, sandET, lavaHL;
        //SandFunnelController sandFunnel;
        Transform sandColumnScaleRoot;

        (TornadoController, Transform)[] tornadoMapRings;
        Transform northTornadoTF;

        public static Transform aboveGroundMarker { get; private set; }
        public static bool isBelowGround { get; private set; }
        public static bool IsReady { get; set; }

        int prop_colorID;
        int prop_centreID;
        int prop_uvLightUpID;
        int prop_rotationID;

        static LayerMask layerHUD;
        Vector3 sphereDefaultScale;
        Vector3 mapRootDefaultScale;

        GameObject errorText;

        ProxyBrittleHollowFragment[] fragments;

        ParticleSystemRenderer playerParticles;
        static float originalMaxParticleSize;
        static Color originalParticleColor;

        public AdvancedMinimap()
        {
            minimapProxy = null;
            current = null;
            IsReady = false;

            minimap = GameObject.Find("SecondaryGroup/HUD_Minimap/Minimap_Root").GetComponent<Minimap>();
            playerMarker = minimap.transform.Find("PlayerMarker");
            var playerMarkerArrow = playerMarker.Find("Arrow");
            playerMarkerArrow.localPosition = new Vector3(0f, 0f, 0.38f);
            playerMarkerArrow.localScale = new Vector3(35f, 35f, 35f); //Makes the marker spin on the spot instead of slightly infront

            aboveGroundMarker = GameObject.Instantiate(playerMarker.gameObject).transform;
            aboveGroundMarker.parent = minimap.transform;
            aboveGroundMarker.localScale = playerMarker.localScale;
            var aboveGroundMarkerArrow = aboveGroundMarker.Find("Arrow");
            aboveGroundMarkerArrow.localPosition = new Vector3(0f, 0f, 0.38f);
            aboveGroundMarkerArrow.localScale = new Vector3(35f, 35f, 35f);
            aboveGroundArrowRenderer = aboveGroundMarkerArrow.GetComponent<MeshRenderer>();
            arrowRenderer = playerMarkerArrow.GetComponent<MeshRenderer>();
            if (dupArrowRenderer == null)
            {
                dupArrowRenderer = arrowRenderer.material;
                dupArrowRenderer.color = new Color(0.7f, 0.4f, 0.1f);
            }
            arrowRenderer.sharedMaterial = dupArrowRenderer;

            layerHUD = LayerMask.NameToLayer("HeadsUpDisplay");

            prop_centreID = Shader.PropertyToID("_Centre");
            prop_uvLightUpID = Shader.PropertyToID("_UVLightUp");
            prop_rotationID = Shader.PropertyToID("_Rotation");
            prop_colorID = Shader.PropertyToID("_Color");
        }

        public override void LateInitialize()
        {
            try
            {
                sandET = Locator._hourglassTwinA.GetComponentInChildren<SandLevelController>();
                sandAT = Locator._hourglassTwinB.GetComponentInChildren<SandLevelController>();
                lavaHL = GameObject.Find("VolcanicMoon_Body/MoltenCore_VM").GetComponent<SandLevelController>();
            }
            catch
            {
                Log.Print("One of the Sand Controllers not found");
            }

            errorText = GameObject.Instantiate(HUDModification.hudTextTemplate.gameObject);
            var t = errorText.GetComponent<Text>();
            t.text = "ERROR";
            t.color = new Color(1f, 0.5f, 0.4f, 1f);
            t.transform.parent = minimap.transform.parent;
            t.transform.localPosition = new Vector3(-0.04f, 0.55f, 2f);
            t.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            t.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

            //---------------- Update default renderers ----------------//
            var m = minimap.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer rndr in m)
            {
                if (rndr.name.Contains("Sphere"))
                {
                    sphereTF = rndr.transform;
                    sphereRenderer = (MeshRenderer)rndr;
                    sphereOldMat = sphereRenderer.sharedMaterials[1];
                }
                if (rndr.name == "Poles")
                {

                }

                if (rndr.name == "PlayerTrailParticleSystem")
                {
                    playerParticles = rndr as ParticleSystemRenderer;
                    if (dupParticleMat == null)
                    {
                        dupParticleMat = playerParticles.material;
                        originalParticleColor = dupParticleMat.GetColor(prop_colorID);
                        originalMaxParticleSize = playerParticles.maxParticleSize;
                    }
                    playerParticles.sharedMaterial = dupParticleMat;
                }
            }

            var mf = sphereTF.GetComponent<MeshFilter>();
            sphereOldMesh = mf.sharedMesh;
            sphereDefaultScale = sphereTF.localScale;
            mapRootDefaultScale = minimap._globeMeshTransform.localScale;
            OnSettingsUpdate();

            //---------------- Get Proxies ----------------//
            proxies = Object.FindObjectsOfType<ProxyBody>();
            //proxyClones = new List<ProxyBody>();

            ProxyWhiteHole whiteHole = null;
            ProxyBrittleHollow proxyBH = null;
            mapList = new List<MinimapPlanetInfo>();
            errorMapNames = new List<string>();
            errorMapNames.Add("QuantumMoon_Body");
            errorMapNames.Add("EyeOfTheUniverse_Body");

            //var all = Object.FindObjectsOfType<OWRigidbody>(); //Temp
            //foreach (var owRB in all) Log.Success(owRB.name);

            IsReady = true;
            foreach (var proxy in proxies)
            {
                string n = proxy.name;
                if (!n.Contains("(Clone)")) continue;

                if (n.Contains("WhiteHole"))
                {
                    Clone(ref WhiteHole, proxy);
                    whiteHole = WhiteHole.GetComponentInChildren<ProxyWhiteHole>();
                    var map = new MinimapPlanetInfo("WhiteHole_Body", WhiteHole, 0.002f, 250f, 0f);
                    map.aboveMarkerMultiplier = 0f;
                    map.uvLightUp = 0f;
                    mapList.Add(map);
                }
                if (n.Contains("TimberHearth"))
                {
                    Clone(ref TimberHearth, proxy);
                    THMoon = TimberHearth.GetComponentInChildren<ProxyOrbiter>(true).gameObject;
                    MoveProxyToMap(THMoon);

                    mapList.Add(new MinimapPlanetInfo("TimberHearth_Body", TimberHearth, 0.002f, 254f, 185f));
                    var map = new MinimapPlanetInfo("Moon_Body", THMoon, 0.0062f, 80f, 0f); map.uvLightUp = 0f; mapList.Add(map);
                }
                if (n.Contains("DarkBramble"))
                {
                    Clone(ref DarkBramble, proxy);
                    var map = new MinimapPlanetInfo("DarkBramble_Body", DarkBramble, 0.0007f, 710f, 0f);
                    map.uvLightUp = -0.3f; map.aboveMarkerMultiplier = 0f;
                    mapList.Add(map);
                }
                if (n.Contains("Comet"))
                {
                    Clone(ref Comet, proxy);
                    var map = new MinimapPlanetInfo("Comet_Body", Comet, 0.0061f, 83f, 75f); map.uvLightUp = -0.5f; mapList.Add(map);
                }
                if (n.Contains("BrittleHollow"))
                {
                    Clone(ref BrittleHollow, proxy);
                    BHMoon = BrittleHollow.GetComponentInChildren<ProxyOrbiter>(true).gameObject;
                    proxyBH = BrittleHollow.GetComponentInChildren<ProxyBrittleHollow>();
                    MoveProxyToMap(BHMoon);
                    var map = new MinimapPlanetInfo("BrittleHollow_Body", BrittleHollow, 0.00185f, 272f, 245f); map.uvLightUp = 1f; mapList.Add(map);
                    mapList.Add(new MinimapPlanetInfo("VolcanicMoon_Body", BHMoon, 0.0052f, 97f, 0f, true));
                }
                if (n.Contains("EmberTwin"))
                {
                    Clone(ref EmberTwin, proxy);
                    mapList.Add(new MinimapPlanetInfo("CaveTwin_Body", EmberTwin, 0.003f, 170f, 120f, true));
                }
                if (n.Contains("AshTwin"))
                {
                    Clone(ref AshTwin, proxy);
                    sandColumnAshTwin = AshTwin.transform.Find("SandColumnRoot");
                    mapList.Add(new MinimapPlanetInfo("TowerTwin_Body", AshTwin, 0.003f, 170f, 30f, true));
                    sandColumnScaleRoot = GameObject.Find("SandFunnel_Body/ScaleRoot").transform;
                }
                if (n.Contains("GiantsDeep"))
                {
                    GiantsDeepRoot = new GameObject("GiantsDeep_Manual");
                    MoveProxyToMap(GiantsDeepRoot);
                    //OrbitalProbeCannon_Pivot ?
                    mapList.Add(new MinimapPlanetInfo("GiantsDeep_Body", GiantsDeepRoot, 0.00103f, -1f, -1f, true));
                }
            }

            //if (n.Contains("QuantumMoon")) return new MinimapPlanetInfo(QuantumMoon, 0.002f);

            if (GiantsDeepRoot != null)
            {
                GDIslands = new List<GDIslandInfo>();
                GetIslandProxyForMap("Proxy_GabbroIsland/GD_Gabbro_Isle_Proxy", "GabbroIsland_Body");
                GetIslandProxyForMap("Proxy_StatueIsland/GD_Statue_Isle_Proxy", "StatueIsland_Body");
                GetIslandProxyForMap("Proxy_ConstructionYard/GD_Constr_Isle_Proxy", "ConstructionYardIsland_Body");
                GetIslandProxyForMap("Proxy_BrambleIsland/DB_Isle_Proxy", "BrambleIsland_Body");
                var qi = GetIslandProxyForMap("Sector_QuantumIsland/Proxy_QuantumIsland", "QuantumIsland_Body");
                if (qi != null)
                {
                    GDQuantumIsland = qi.transform;
                    GDQuantumTower = GDQuantumIsland.Find("Proxy_GD_QuantumTower").gameObject;
                }
                
                var actualGD = GameObject.Find("GiantsDeep_Body");
                if (actualGD != null)
				{
					var tornadoes = actualGD.GetComponentsInChildren<TornadoController>(true);
					tornadoMapRings = new (TornadoController, Transform)[tornadoes.Length];
					for (int i = 0; i < tornadoes.Length; i++)
					{
						var obj = Object.Instantiate(GEAssets.ProxyTornado);
						var tf = obj.transform;
						tf.parent = GiantsDeepRoot.transform;
						tf.localScale = Vector3.one;

						var rndrs = obj.GetComponentsInChildren<MeshRenderer>();
						foreach (var rndr in rndrs)
						{
							rndr.sharedMaterial = GEAssets.MinimapMat;
							rndr.gameObject.layer = layerHUD;
						}

						tornadoMapRings[i] = (tornadoes[i], tf);
					}
				}

                var northTornado = Object.Instantiate(GEAssets.ProxyHurricane);
                northTornado.layer = layerHUD;
                northTornadoTF = northTornado.transform;
                northTornadoTF.parent = GiantsDeepRoot.transform;
                northTornadoTF.localPosition = new Vector3(0f, 430f, 0f);
                northTornadoTF.localScale = Vector3.one;

                var northTornadoRndr = northTornado.GetComponent<MeshRenderer>();
                northTornadoRndr.sharedMaterial = GEAssets.MinimapMat;
            }

            if (whiteHole != null && proxyBH != null)
            {
                proxyBH.AssignBrittleHollowReference();
                proxyBH.AssignWhiteHoleReference(whiteHole);
                fragments = proxyBH._fragments;
                //Log.Print($"Fragments: {fragments.Length}");

                var actualWH = GameObject.Find("WhiteHole_Body");
                if (actualWH != null)
                {
                    //WhiteholeStation_Body
                    // /RFVolume_WhiteholeStation
                    void CreateWHProxy(GameObject obj)
                    {
                        var newObj = GameObject.Instantiate(obj);
                        newObj.transform.parent = whiteHole.transform;
                        newObj.transform.localPosition = actualWH.transform.InverseTransformPoint(obj.transform.position);
                        newObj.transform.localRotation = actualWH.transform.InverseTransformRotation(obj.transform.rotation);
                        newObj.transform.localScale = Vector3.one;
                        var rndrs = newObj.GetComponentsInChildren<MeshRenderer>();
                        foreach (var r in rndrs)
                        {
                            var m = r.sharedMaterials;
                            for (int i = 0; i < m.Length; i++) m[i] = GEAssets.MinimapMat;
                            r.sharedMaterials = m;
                        }
                        newObj.layer = layerHUD;
                    }

                    var station = GameObject.Find("Sector_WhiteholeStation/Proxy_WhiteholeStation/Structure_NOM_WhiteHoleStation_Proxy");
                    if (station != null) CreateWHProxy(station);
                    var stationIce = GameObject.Find("Proxy_WhiteholeStationSuperstructure/Terrain_WH_StationIce_Proxy");
                    if (stationIce != null) CreateWHProxy(stationIce);


                    var rulesetObj = actualWH.transform.Find("Sector_WhiteHole/RulesetVolumes_WhiteHole");
                    var whRuleset = rulesetObj.gameObject.AddComponent<PlanetoidRuleset>();
                    whRuleset._altitudeCeiling = 0f;
                    whRuleset._altitudeFloor = 0f;
                    whRuleset._horizonRadius = 0f;
                    whRuleset._shuttleLandingRadius = 0f;
                    whRuleset._useAltimeter = false;
                    whRuleset._useMinimap = true;
                    whRuleset.ResetAttachedBody();
                    var fakeGravityVolume = actualWH.AddComponent<GravityVolume>();
                    whRuleset._attachedBody._attachedGravityVolume = fakeGravityVolume; //Not used, on obj that can't enter volume with
                }
            }
        }

        public override void OnSettingsUpdate()
        {
            var mats = sphereRenderer.sharedMaterials;
            var mf = sphereTF.GetComponent<MeshFilter>();
            
            if (Settings.Minimap == MinimapSetting.Disabled)
            {
                ResetMap();
                errorText.SetActive(false);
                minimap._globeMeshTransform.parent.gameObject.SetActive(false);
            }
            else minimap._globeMeshTransform.parent.gameObject.SetActive(true);

            if (Settings.Minimap == MinimapSetting.Vanilla)
            {
                dupParticleMat.SetColor(prop_colorID, originalParticleColor);
                playerParticles.maxParticleSize = originalMaxParticleSize;

                sphereTF.gameObject.SetActive(true);
                mf.sharedMesh = sphereOldMesh;
                mats[1] = sphereOldMat;
            }
            if (Settings.Minimap == MinimapSetting.Vanilla || Settings.Minimap == MinimapSetting.Simple)
            {
                ResetMap();
                errorText.SetActive(false);
                sphereTF.localScale = sphereDefaultScale;
                minimap._globeMeshTransform.localScale = mapRootDefaultScale;
            }
            if (Settings.Minimap == MinimapSetting.Advanced || Settings.Minimap == MinimapSetting.Simple)
            {
                mf.sharedMesh = GEAssets.SphereNoUVs;
                mats[1] = GEAssets.MinimapMat;

                playerParticles.maxParticleSize = 0.0015f;
            }

            sphereRenderer.sharedMaterials = mats;
        }

        void Clone(ref GameObject field, ProxyBody toClone)
        {
            field = Object.Instantiate(toClone.gameObject);
            MoveProxyToMap(field);
        }
        GameObject GetIslandProxyForMap(string path, string actualIslandPath)
        {
            var originalProxy = GameObject.Find(path);
            if (originalProxy == null) { Log.Print($"Proxy {path} was null"); return null; }
            var actualIsland = GameObject.Find(actualIslandPath);
            if (actualIsland == null) { Log.Print($"{actualIslandPath} was null"); return null; }

            var clone = Object.Instantiate(originalProxy.gameObject);
            var casters = clone.GetComponentsInChildren<ProxyShadowCaster>();
            for (int i = casters.Length - 1; i >= 0; i--) {
                Object.Destroy(casters[i]);
            }

            GDIslands.Add(new GDIslandInfo(clone.transform, originalProxy.transform, actualIsland.transform));

            MoveProxyToMap(clone, true);
            var tf = clone.transform;
            tf.parent = GiantsDeepRoot.transform;
            tf.localPosition = Vector3.zero;

            return clone;
        }
        void MoveProxyToMap(GameObject proxyObj, bool active = false)
        {
            var tf = proxyObj.transform;
            tf.parent = minimap._globeMeshTransform;
            tf.localPosition = Vector3.zero;

            if (proxyObj == AshTwin) //Mobius why       (scale fixed in ShowCurrent)
            {
                tf.localRotation = Quaternion.Euler(0f, 270f, 0f);
            }
            else
            {
                tf.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }

            var proxies = proxyObj.GetComponentsInChildren<Transform>(true);
            foreach (Transform proxy in proxies)
            {
                if (proxy.name.Contains("Atmo") || proxy.name.Contains("Fog") || proxy.name.Contains("Effect")) continue;

                if (!proxy.TryGetComponent(out MeshRenderer rndr)) continue;
                rndr.gameObject.layer = layerHUD;
                rndr.enabled = true;

                if (proxy.name == "BlackHoleRenderer" || proxy.name == "Singularity") continue; //Leave black and white hole as is

                if (rndr.sharedMaterials.Length == 1) rndr.sharedMaterial = GEAssets.MinimapMat;
                else
                {
                    var mats = rndr.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) mats[i] = GEAssets.MinimapMat;
                    rndr.sharedMaterials = mats;
                }
            }
            
            proxyObj.SetActive(active);
        }

        public override void Update()
        {
            playerMarker.gameObject.SetActive(Settings.Minimap != MinimapSetting.Vanilla);
            if (Settings.Minimap == MinimapSetting.Vanilla)
            {
                Vector3 localPos = playerMarker.localPosition.normalized * 0.51f * 1.0001f;
                aboveGroundMarker.localPosition = localPos;
                aboveGroundMarker.localRotation = playerMarker.localRotation;
                return;
            }

            Quaternion shaderRot;
            if (minimapProxy != null)
            {
                sphereTF.gameObject.SetActive(minimapProxy.keepOriginalSphere);
                shaderRot = minimapProxy.mapRoot.transform.rotation;
                HandleSpecificProxyMaps();
                HandleMarkersAndTrail(minimapProxy.undergroundRadius, minimapProxy.aboveMarkerMultiplier);

                bool show = minimap._updateMinimap;
                foreach (var item in currentRenderers)
                {
                    if (item.enabled != show) item.enabled = show;
                }
            }
            else
            {
                sphereTF.gameObject.SetActive(true);
                shaderRot = sphereTF.rotation;
                HandleMarkersAndTrail(0f, 0.6f);
            }

            errorText.SetActive(errorMap && minimap._updateMinimap && Settings.Minimap == MinimapSetting.Advanced);

            if (minimap._updateMinimap)
            {
                //Vertex object position wasn't good enough so using world space.
                shaderRot = Quaternion.Euler(0f, 0f, 180f) * Quaternion.Inverse(shaderRot);
                Vector4 rotVec = new Vector4(shaderRot.x, shaderRot.y, shaderRot.z, shaderRot.w);
                GEAssets.MinimapMat.SetVector(prop_rotationID, rotVec);

                Vector3 pos = minimap._globeMeshTransform.position;
                //if (current != null && current.gameObject == WhiteHole) pos += new Vector3(0f, 200f, 0f);
                GEAssets.MinimapMat.SetVector(prop_centreID, pos);
            }

            if (Settings.Minimap == MinimapSetting.Simple) return;

            var planet = minimap._playerRulesetDetector.GetPlanetoidRuleset();
            if (planet != current)
            {
                if (fragments != null) {
                    foreach (var f in fragments) f.enabled = false;
                }

                HideCurrent();
                current = planet;
                ShowCurrent();
            }
            /*
            if (planet != null && planet.name.Contains("_QM"))
            {
                Log.Print("QM Ruleset");
            }
            */
        }

        void HandleMarkersAndTrail(float undergroundRadius, float aboveMarkerRadius = 0.6f)
        {
            if (aboveGroundArrowRenderer.enabled != minimap._updateMinimap) aboveGroundArrowRenderer.enabled = minimap._updateMinimap;

            var ruleset = minimap._playerRulesetDetector.GetPlanetoidRuleset();
            if (ruleset != null)
            {
                float height = Vector3.Distance(minimap._playerTransform.position, ruleset.transform.position);

                if (height < undergroundRadius)
                {
                    isBelowGround = true;
                    dupParticleMat.SetColor(prop_colorID, new Color(0.3f, 0.7f, 0.9f, 0.5f));

                    //To do: replace marker/trail mats with ztest always when underground so can still see?
                }
                else
                {
                    isBelowGround = false;
                    dupParticleMat.SetColor(prop_colorID, new Color(0.9f, 0.6f, 0.3f, 1f));
                }

                float aboveGroundHeight = Mathf.Max(aboveMarkerRadius, playerMarker.localPosition.magnitude + 0.01f);

                Vector3 localPos = playerMarker.localPosition.normalized * aboveGroundHeight;
                aboveGroundMarker.localPosition = localPos;
                aboveGroundMarker.localRotation = playerMarker.localRotation;
            }
        }

        void HandleSpecificProxyMaps()
        {
            var mapRoot = minimapProxy.mapRoot;
            if (mapRoot == GiantsDeepRoot)
            {
                var gd = Locator._giantsDeep.transform;
                Vector3 vec = Locator.GetPlayerTransform().position - gd.position;
                float dot = Vector3.Dot(gd.up, vec.normalized);

                bool insideBigTornado = dot > 0.95f && PlayerState.InGiantsDeep();

                //Don't hide island existance, but avoid revealing the quantum tower if haven't gone there
                GDQuantumTower.SetActive(insideBigTornado || Locator.GetShipLogManager().IsFactRevealed("GD_QUANTUM_TOWER_X1"));
                if (insideBigTornado)
                {
                    //minimap._globeMeshTransform.localScale = mapRootDefaultScale;
                    minimapProxy.radius = 400f;
                    minimapProxy.undergroundRadius = 1000f;
                    minimapProxy.aboveMarkerMultiplier = 1.5f;
                    //GDQuantumIsland.localScale = Vector3.one * 2f;
                    minimap._globeMeshTransform.localScale = mapRootDefaultScale * Settings.GiantsDeepMapScale;
                }
                else
                {
                    minimapProxy.radius = 450f;
                    minimapProxy.undergroundRadius = 499.5f;
                    minimapProxy.aboveMarkerMultiplier = 0.6f;
                    float dist = vec.magnitude;
                    float t = Mathf.Clamp01((dist - 600f) / 400f);
                    t += Mathf.Clamp01((450f - dist) / 50f);
                    t = Mathf.Clamp01(t);
                    t = ModMain.Smooth(t);

                    minimap._globeMeshTransform.localScale = mapRootDefaultScale * Mathf.Lerp(Settings.GiantsDeepMapScale, 1f, t);
                }

                foreach (var island in GDIslands)
                {
                    float dist = Vector3.Distance(gd.position, island.actualIsland.position);
                    float t = 1f - Mathf.Clamp01((dist - 600f) / 600f);
                    t = ModMain.Smooth(t);

                    island.miniMapProxy.localPosition = gd.InverseTransformPoint(island.originalProxy.position);
                    island.miniMapProxy.localRotation = gd.InverseTransformRotation(island.originalProxy.rotation);
                    island.miniMapProxy.localScale = new Vector3(t, t, t);
                }
                foreach (var tornado in tornadoMapRings)
                {
                    var root = tornado.Item1._tornadoRoot;
                    tornado.Item2.gameObject.SetActive(root.activeSelf);

                    tornado.Item2.localPosition = gd.InverseTransformPoint(root.transform.position);
                    tornado.Item2.localRotation = gd.InverseTransformRotation(root.transform.rotation) * Quaternion.Euler(0f, Time.timeSinceLevelLoad * 60f, 0f);
                }

                northTornadoTF.localRotation = Quaternion.Euler(0f, Time.timeSinceLevelLoad * 30f, 0f);
            }

            if (mapRoot == BrittleHollow || mapRoot == WhiteHole)
            {
                var bh = Locator._brittleHollow.transform;
                var wh = Locator._whiteHole.transform;
                foreach (var f in fragments)
                {
                    if (f.detached || f.warped) f._initialized = false;

                    var fTF = f.transform;
                    if (!f.detached) continue;

                    var realObj = f._realObjectTransform;
                    if (f.warped)
                    {
                        foreach (var r in f._renderers) r.enabled = true;

                        fTF.localPosition = wh.InverseTransformPoint(realObj.position);
                        fTF.localRotation = wh.InverseTransformRotation(realObj.rotation);
                        fTF.localScale = realObj.lossyScale;
                    }
                    else
                    {
                        fTF.localPosition = bh.InverseTransformPoint(realObj.position);
                        fTF.localRotation = bh.InverseTransformRotation(realObj.rotation);
                        fTF.localScale = realObj.lossyScale;
                    }
                }
            }

            if (mapRoot == AshTwin)
            {
                SetMinimapSphereScale(sandAT.GetRadius() * minimapProxy.miniMapScale);

                Vector3 vec = (sandET.transform.position - sandAT.transform.position);
                Quaternion rot = Quaternion.LookRotation(vec, sandAT.transform.up);
                sandColumnAshTwin.localRotation = Quaternion.Euler(0f, 90f, 0f) * sandAT.transform.InverseTransformRotation(rot);
                sandColumnAshTwin.localScale = sandColumnScaleRoot.localScale;
            }
            if (mapRoot == EmberTwin)
            {
                SetMinimapSphereScale(sandET.GetRadius() * minimapProxy.miniMapScale);
            }
            if (mapRoot == BHMoon)
            {
                SetMinimapSphereScale(lavaHL.transform.lossyScale.x * 0.5f * 0.93f);
            }
        }

        void SetMinimapSphereScale(float scale)
        {
            sphereTF.localScale = new Vector3(scale, scale, scale);
        }

        static void HideCurrent()
        {
            if (minimapProxy == null) return;

            minimapProxy.mapRoot.SetActive(false);
        }
        void ShowCurrent()
        {
            if (current == null) return;

            var rb = current.GetAttachedOWRigidbody();

            minimapProxy = GetMinimapProxyBody(rb);
            sphereTF.localScale = sphereDefaultScale; //1.0.2 - Moved from after if - fix for Sand Twins -> Astral Codec

            if (minimapProxy == null)
            {
                Log.Print($"No map found for {rb.name}");
                return;
            }
            GameObject mapRoot = minimapProxy.mapRoot;

            float scale = minimapProxy.miniMapScale;
            mapRoot.transform.localScale = new Vector3(-scale, -scale, scale); //flipped x and y for some reason
            minimap._globeMeshTransform.localScale = mapRootDefaultScale;
            mapRoot.SetActive(true);
            currentRenderers = mapRoot.GetComponentsInChildren<MeshRenderer>();

            GEAssets.MinimapMat.SetFloat(prop_uvLightUpID, minimapProxy.uvLightUp);

            Log.Print($"Activating map {mapRoot.name}");
        }

        static List<MinimapPlanetInfo> mapList;
        static List<string> errorMapNames = new List<string>();
        bool errorMap;
        MinimapPlanetInfo GetMinimapProxyBody(OWRigidbody rb)
        {
            string rbName = rb.name;
            Log.Print($"Get map for {rbName}");

            errorMap = false;
            foreach (var errorMapName in errorMapNames)
            {
                if (rbName == errorMapName)
                {
                    errorMap = true;
                    return null;
                }
            }

            foreach (var map in mapList)
            {
                if (rbName == map.owrbName) return map;
            }
            /*
            foreach (var map in mapList)    //Removed, using exact names now to not mess up NH
            {
                if (n.Contains(map.owrbName)) return map;
            }
            */

            return null;
        }
        static void ResetMap()
        {
            HideCurrent();
            current = null;
            minimapProxy = null;
        }


        public static void AddOtherModMap(string owrbName, GameObject map, float radius)
        {
            var rndrs = map.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in rndrs)
            {
                r.gameObject.layer = layerHUD;
                if (r.name.Contains("KeepMat")) continue; //Render obj as is on map

                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = GEAssets.MinimapMat;
                r.sharedMaterials = mats;
            }
            var tf = map.transform;
            tf.parent = minimap._globeMeshTransform;
            tf.localPosition = Vector3.zero;
            tf.localRotation = Quaternion.identity;

            var mm = new MinimapPlanetInfo(owrbName, map, 0.5f * (1f / radius), radius, -1f);
            mm.uvLightUp = 0f;
            mapList.Add(mm);

            ResetMap();
        }

        public static void UpdateAdvancedMap(string owrbName, GameObject map)
        {
            for (int i = mapList.Count - 1; i >= 0; i--)
            {
                if (mapList[i].owrbName == owrbName) mapList[i].mapRoot = map;
            }
        }
        public static void RemoveAdvancedMap(string owrbName)
        {
            for (int i = mapList.Count - 1; i >= 0; i--)
            {
                if (mapList[i].owrbName == owrbName) mapList.RemoveAt(i);
            }

            ResetMap();
        }
        public static void AddErrorMap(string owrbName)
        {
            errorMapNames.Add(owrbName);
        }
    }
    public sealed class MinimapPlanetInfo
    {
        public string owrbName { get; private set; }
        public GameObject mapRoot { get; set; }
        public float miniMapScale { get; private set; }
        public float radius { get; set; }
        public float undergroundRadius { get; set; }
        public float aboveMarkerMultiplier { get; set; }
        public bool keepOriginalSphere { get; private set; }
        public float uvLightUp { get; set; }

        public MinimapPlanetInfo(string owrbName, GameObject proxyBody, float miniMapScale, float radius, float undergroundRadius, bool keepOriginalSphere = false)
        {
            this.owrbName = owrbName;
            this.mapRoot = proxyBody;
            this.miniMapScale = miniMapScale;
            this.radius = radius;
            this.undergroundRadius = undergroundRadius;
            this.keepOriginalSphere = keepOriginalSphere;
            this.aboveMarkerMultiplier = 0.6f;
            this.uvLightUp = 2f;
        }
    }

    public sealed class GDIslandInfo
    {
        public Transform miniMapProxy { get; private set; }
        public Transform originalProxy { get; private set; } //Pivots are messed up
        public Transform actualIsland { get; private set; }

        public GDIslandInfo(Transform miniMapProxy, Transform originalProxy, Transform actualIsland)
        {
            this.miniMapProxy = miniMapProxy;
            this.originalProxy = originalProxy;
            this.actualIsland = actualIsland;
        }
    }
}