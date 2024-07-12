using UnityEngine;

namespace GeneralEnhancements
{
    public class GeneralEnhancementsApi : IGeneralEnhancements
    {
        public bool isReady => AdvancedMinimap.IsReady;
        public void AddAdvancedMap(string owrbName, GameObject map, float radius = 250f) => AdvancedMinimap.AddOtherModMap(owrbName, map, radius);
        public GameObject GetAdvancedMap(string owrbName) => AdvancedMinimap.GetAdvancedMap(owrbName);
        public void UpdateAdvancedMap(string owrbName, GameObject map, float radius = 0) => AdvancedMinimap.UpdateAdvancedMap(owrbName, map, radius);
        public void ReplaceAdvancedMap(string owrbName, GameObject map, float radius = 0) => AdvancedMinimap.ReplaceAdvancedMap(owrbName, map, radius);
        public void RemoveAdvancedMap(string owrbName) => AdvancedMinimap.RemoveAdvancedMap(owrbName);
        public void AddErrorMap(string owrbName) => AdvancedMinimap.AddErrorMap(owrbName);
        public GameObject MakeMinimapTornado() => AdvancedMinimap.MakeTornado();
        public GameObject MakeMinimapHurricane() => AdvancedMinimap.MakeHurricane();
    }
}
