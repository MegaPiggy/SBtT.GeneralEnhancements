using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GeneralEnhancements
{
    public sealed class SkipToShip : Feature
    {
        PlayerCameraEffectController cameraEffectController;

        bool beenToShip;
        bool beenToStranger;
        float skipCharge;
        ScreenPrompt prompt;
        public SkipToShip()
        {

        }
        public override void LateInitialize()
        {
            cameraEffectController = Object.FindObjectOfType<PlayerCameraEffectController>();

            prompt = new ScreenPrompt(InputLibrary.interact, "", 5, ScreenPrompt.DisplayState.Normal, true);
            Locator.GetPromptManager().AddScreenPrompt(prompt, PromptPosition.UpperRight);
        }

        public override void Update()
        {
            if (TimeLoop.GetLoopCount() < 4) return;    //Changed from 2 to 4 to stop able to skip initial Slate dialogue
            if (PlayerState.InConversation()) { prompt.SetVisibility(false); return; }   //Fixed funny skipping while talking

            if (beenToShip)
            {
                if (!beenToStranger) HandleSkipToStranger();
            }
            else HandleSkipToShip();
        }
        void SkipCharge()
        {
            if (OWInput.IsPressed(InputLibrary.interact))
            {
                skipCharge += Time.deltaTime;
            }
            else
            {
                skipCharge = 0f;
            }
        }

        void HandleSkipToShip()
        {
            if (Locator.GetShipBody() == null) return;

            //Check not in NH system
            if (Locator._timberHearth == null || !Locator._timberHearth.gameObject.activeSelf) { OnGoToShip(); return; }

            //Don't allow skip Slate
            ShipLogManager shipLogManager = Locator.GetShipLogManager();
            if (!PlayerData.GetPersistentCondition("COMPLETED_SHIPLOG_TUTORIAL") && shipLogManager.IsFactRevealed("IP_RING_WORLD_X1")) return;

            //Weirdness if try to skip when in elevator.
            if (PlayerState.IsInsideShip() || PlayerState.IsAttached() || Time.timeSinceLevelLoad > 30f) { OnGoToShip(); return; }

            prompt.SetVisibility(true);
            prompt.SetText(GEText.SkipToShip() + "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt));

            SkipCharge();

            if (skipCharge > 1f)
            {
                var player = Locator._playerBody;
                var ship = Locator._shipBody;

                player.WarpToPositionRotation(ship.GetPosition() - ship.transform.up * 2f, ship.GetRotation());
                player.SetVelocity(ship.GetVelocity());

                Log.Print("Teleport to Ship.");
                Locator.GetPlayerAudioController().PlayPlayerSingularityTransit();

                var suit = ship.GetComponentInChildren<SuitPickupVolume>();
                suit.OnPressInteract(InputLibrary.interact);
                OnGoToShip();

                cameraEffectController.CloseEyes(0f);
                cameraEffectController.OpenEyes(ManualBlinking.openEyesDuration);
            }
        }
        
        void HandleSkipToStranger()
        {
            if (Locator.GetRingWorldController() == null) return;
            if (PlayerState.InCloakingField() || Time.timeSinceLevelLoad > 120f) { OnGoToStranger(); return; }
            if (!PlayerState.IsInsideShip() || ShipLogEntryHUDMarker.s_entryLocation == null || !ShipLogEntryHUDMarker.s_entryLocation.name.StartsWith("IP_"))
            {
                prompt.SetVisibility(false);
                return;
            }

            prompt.SetVisibility(true);
            prompt.SetText(GEText.SkipToStranger() + "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt));

            SkipCharge();

            if (skipCharge > 1f && Locator.GetShipBody() != null)
            {
                var ringworld = Locator._ringWorld.transform;
                //var rf = Locator._ringWorld.GetComponentInChildren<ReferenceFrame>();
                //if (rf != null) Locator._rfTracker._currentReferenceFrame = rf;

                var localPos = new Vector3(100f, -850f, -450f);
                var worldPos = ringworld.TransformPoint(localPos);

                var player = Locator._playerBody;
                var ship = Locator._shipBody;

                //var playerLocal = ship._transform.InverseTransformPoint(player.GetPosition());

                Quaternion rot = Quaternion.LookRotation(ringworld.position - worldPos, ringworld.right);
                if (!(PlayerState.UsingShipComputer() || PlayerState.AtFlightConsole()))
                {
                    player.WarpToPositionRotation(worldPos, rot);
                }
                ship.WarpToPositionRotation(worldPos, rot);
                //player._transform.position = ship._transform.TransformPoint(playerLocal); //Messes stuff up?

                var targetVelocity = Locator._ringWorld._owRigidbody.GetVelocity();
                player.SetVelocity(targetVelocity);
                ship.SetVelocity(targetVelocity);

                Log.Print("Teleport to Stranger.");
                Locator.GetPlayerAudioController().PlayPlayerSingularityTransit();

                OnGoToStranger();

                cameraEffectController.CloseEyes(0f);
                cameraEffectController.OpenEyes(ManualBlinking.openEyesDuration);
            }
        }

        void OnGoToShip()
        {
            skipCharge = 0f;
            beenToShip = true;
            prompt.SetVisibility(false);
            //ui.SetActive(false);
        }
        void OnGoToStranger()
        {
            beenToStranger = true;
            prompt.SetVisibility(false);
            //ui.SetActive(false);
        }
    }
}