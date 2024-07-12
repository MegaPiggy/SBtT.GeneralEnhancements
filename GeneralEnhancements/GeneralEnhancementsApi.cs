using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GeneralEnhancements
{
    public class GeneralEnhancementsApi : IGeneralEnhancements
    {
        public bool isReady => AdvancedMinimap.IsReady;
        public void AddAdvancedMap(string owrbName, GameObject map, float radius = 250f) => AdvancedMinimap.AddOtherModMap(owrbName, map, radius);
        public void UpdateAdvancedMap(string owrbName, GameObject map) => AdvancedMinimap.UpdateAdvancedMap(owrbName, map);
        public void RemoveAdvancedMap(string owrbName) => AdvancedMinimap.RemoveAdvancedMap(owrbName);
        public void AddErrorMap(string owrbName) => AdvancedMinimap.AddErrorMap(owrbName);
    }
}
