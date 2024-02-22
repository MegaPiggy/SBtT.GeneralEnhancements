using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GeneralEnhancements
{
    /// <summary>
    /// Code used as a reference: Toggle Velocity Matching by Vesper-Works
    /// https://outerwildsmods.com/mods/togglevelocitymatching/
    /// </summary>
    public sealed class ContinuousMatchVelocity : Feature
    {
        public static bool matchVelocityShip { get; private set; }
        public static bool matchVelocityPlayer { get; private set; }
        public static bool showedMessage { get; set; }
        public static float distance { get; set; }


        IInputCommands[] stopInputs;
        public static Autopilot shipAutopilot { get; private set; }
        public static Autopilot playerAutopilot { get; private set; }
        static bool shipExists;
        static bool playerExists;

        public static ShipThrusterController shipThruster { get; private set; }

        public ContinuousMatchVelocity()
        {
            matchVelocityShip = false;
            matchVelocityPlayer = false;
            showedMessage = false;

            var pilots = GameObject.FindObjectsOfType<Autopilot>();
            foreach (var pilot in pilots)
            {
                if (pilot.name == "Ship_Body") shipAutopilot = pilot;
                if (pilot.name == "Player_Body") playerAutopilot = pilot;
            }
            shipThruster = Object.FindObjectOfType<ShipThrusterController>();

            shipExists = shipAutopilot != null;
            playerExists = playerAutopilot != null;

            stopInputs = new IInputCommands[] {
                InputLibrary.thrustUp, InputLibrary.thrustDown, InputLibrary.thrustX, InputLibrary.thrustZ
            };

            GlobalMessenger.RemoveListener("EnterShip", OnEnterShip);
            GlobalMessenger.AddListener("EnterShip", OnEnterShip);
        }
        static void OnEnterShip()
        {
            /*
            StopShipMatch();
            StopPlayerMatch();
            if (shipExists) shipAutopilot._referenceFrame = null;
            if (playerExists) playerAutopilot._referenceFrame = null;
            */

            if (shipThruster._landingManager.IsLanded()) KeyboardThrottleControl.ResetShip(); //Make lift off properly
            //KeyboardThrottleControl.ResetThrottle();
        }
        public override void OnSettingsUpdate()
        {
            if (!Settings.ContinuousMatchVelocity)
            {
                StopShipMatch();
                StopPlayerMatch();
            }
        }

        public static void StopShipMatch()
        {
            Log.Print("Stop SHIP Match Velocity");

            matchVelocityShip = false;
            shipAutopilot.StopMatchVelocity();
        }
        public static void StopPlayerMatch()
        {
            Log.Print("Stop Player Match Velocity");

            matchVelocityPlayer = false;
            playerAutopilot.StopMatchVelocity();
        }
        public override void Update()
        {
            if (!Settings.ContinuousMatchVelocity) return;

            bool controllingShip = OWInput.IsInputMode(InputMode.ShipCockpit);
            if (OWInput.IsNewlyPressed(InputLibrary.matchVelocity))
            {
                if (controllingShip && !matchVelocityShip)
                {
                    Log.Print("START SHIP Match Velocity");
                    matchVelocityShip = true;

                    return;
                }
                if (!controllingShip && !matchVelocityPlayer && PlayerState.InZeroG())
                {
                    Log.Print("START Player Match Velocity");
                    matchVelocityPlayer = true;

                    return;
                }
            }

            if (matchVelocityShip) KeyboardThrottleControl.ResetShip();
            if (matchVelocityPlayer) KeyboardThrottleControl.ResetPlayer();

            bool stop = false;
            foreach (var input in stopInputs)
            {
                if (OWInput.IsPressed(input)) { stop = true; break; }
            }
            if (stop)
            {
                if (controllingShip)
                {
                    showedMessage = false;
                    if (matchVelocityShip) StopShipMatch();
                }
                else
                {
                    if (matchVelocityPlayer) StopPlayerMatch();
                }
            }
        }
    }
}