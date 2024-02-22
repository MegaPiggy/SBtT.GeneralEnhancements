using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GeneralEnhancements
{
    public sealed class GEAssets
    {
        AssetBundle bundle;
        public static Material MinimapMat { get; private set; }
        public static Mesh SphereNoUVs { get; private set; }
        /*
        public static Mesh SphereNoUVsInverted { get; private set; }
        public static Mesh Ring { get; private set; }
        public static Mesh RingThin { get; private set; }
        public static Mesh Arrow { get; private set; }
        public static Mesh CylinderInverted { get; private set; }
        */
        public static Mesh InvertedAtmosphere { get; private set; }
        public static Mesh HomePlanetRing { get; private set; }
        public static GameObject ProxyTornado { get; private set; }
        public static GameObject ProxyHurricane { get; private set; }

        public GEAssets(AssetBundle bundle)
        {
            this.bundle = bundle;
            if (Log.ErrorIf(bundle == null, "Bundle is null!")) return;

            MinimapMat = bundle.LoadAsset<Material>("GE_Minimap_Shader.mat");
            SphereNoUVs = LoadMesh("Sphere_NoUVs");

            /*
            SphereNoUVsInverted = LoadMesh("Sphere_NoUVs_Inverted");
            Ring = LoadMesh("Ring");
            RingThin = LoadMesh("Ring_Thin");
            Arrow = LoadMesh("Arrow");
            CylinderInverted = LoadMesh("Cylinder_Inverted");
            */

            InvertedAtmosphere = LoadMesh("Atmosphere_Inverted");
            HomePlanetRing = LoadMesh("PlanetRing");

            ProxyTornado = bundle.LoadAsset<GameObject>("ProxyTornado.prefab");
            ProxyHurricane = bundle.LoadAsset<GameObject>("ProxyHurricane.prefab");
        }

        Mesh LoadMesh(string name)
        {
            //You found the jank

            var prefab = bundle.LoadAsset<GameObject>($"{name}.prefab");
            if (prefab == null) { Log.Error($"Mesh {name} not found"); return null; }

            if (prefab.TryGetComponent(out MeshFilter mf)) return mf.sharedMesh;

            Log.Error($"Prefab {name} does not have a mesh");
            return null;
        }
    }
}