using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GeneralEnhancements
{
    public sealed class MiscFeatures : Feature
    {
        public MiscFeatures()
        {
            
        }
        public override void LateInitialize()
        {
            /*
            if (Locator.GetQuantumMoon() == null) return;   //Tried to add Quantum Poem to QM similar to Alpha

            var quantumPoemObj = SearchUtilities.Find("Interactables_QuantumGrove/QuantumSaplings/Quantum_PoemTree");
            if (quantumPoemObj != null)
            {
                GameObject newObj = GameObject.Instantiate(quantumPoemObj);

                var sector = Locator.GetQuantumMoon().transform.Find("Sector_QuantumMoon").GetComponent<Sector>();
                var thState = Locator.GetQuantumMoon().transform.Find("Sector_QuantumMoon/State_TH");
                var thSockets = thState.GetComponentsInChildren<QuantumSocket>(true);

                newObj.transform.parent = thState;

                var quantumPoem = newObj.GetComponent<SocketedQuantumObject>();
                quantumPoem.SetQuantumSockets(thSockets);
                quantumPoem.SetSector(sector);
            }
            */
        }
        public override void Update()
        {
            /*
            var suit = Locator.GetPlayerSuit();
            if (suit == null) return;

            if (Keyboard.current[Key.H].wasPressedThisFrame && suit.IsWearingSuit())
            {
                if (suit.IsWearingHelmet())
                {
                    suit.RemoveHelmet();
                    HUDModification.Reticule.SetActive(false);
                }
                else
                {
                    suit.PutOnHelmet();
                    HUDModification.Reticule.SetActive(false);
                }
            }
            */

            /*
            var held = Locator.GetToolModeSwapper().GetItemCarryTool().GetHeldItem();
            if (OWInput.IsNewlyPressed(InputLibrary.cancel) && held != null && held.GetItemType() == ItemType.DreamLantern)
            {
                Locator.GetToolModeSwapper().EquipToolMode(ToolMode.Probe); //Doesn't work
            }
            */

        }
    }
}