using OWML.Common;
using OWML.ModHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GeneralEnhancements
{
    public sealed class Patches
    {
        //SnooPING AS usual I see, JOHN CORBY?
        public static void SetUp(ModMain main)
        {
            IHarmonyHelper harmony = main.ModHelper.HarmonyHelper;
            var t = typeof(Patches);

            harmony.AddPrefix<Autopilot>("FlyToDestination", t, nameof(Patches.FlyToDestination));
            harmony.AddPrefix<ShipCockpitController>("OnTargetReferenceFrame", t, nameof(Patches.OnTargetReferenceFrame));
            harmony.AddPrefix<ShipCockpitController>("OnUntargetReferenceFrame", t, nameof(Patches.OnUntargetReferenceFrame));

            harmony.AddPrefix<Autopilot>("StopMatchVelocity", t, nameof(Patches.StopMatchVelocity));
            harmony.AddPrefix<AutopilotGUI>("OnMatchedVelocity", t, nameof(Patches.OnMatchedVelocity));

            harmony.AddPostfix<ReferenceFrameGUI>("Update", t, nameof(Patches.Update));
            harmony.AddPostfix<TitleAnimationController>("Update", t, nameof(Patches.TitleAnimationController_Update));

            harmony.AddPostfix<CanvasMarker>("UpdateDistanceText", t, nameof(Patches.UpdateDistanceText));

            //harmony.Transpile<ProbeLauncher>("UpdatePreLaunch", t, nameof(Patches.UpdatePreLaunch));

            harmony.AddPrefix<ThrusterController>("FixedUpdate", t, nameof(Patches.FixedUpdate));
            harmony.AddPostfix<OWInput>("GetAxisValue", t, nameof(Patches.GetAxisValue));

            harmony.AddPrefix<Minimap>("GetLocalMapPosition", t, nameof(Patches.GetLocalMapPosition));
            harmony.AddPrefix<Minimap>("UpdateTrails", t, nameof(Patches.UpdateTrails));
            harmony.AddPrefix<Minimap>("UpdateMarkers", t, nameof(Patches.UpdateMarkers));
            harmony.AddPrefix<Minimap>("OnPutOnSuit", t, nameof(Patches.KeepMinimapDisabled));

            harmony.AddPrefix<PlayerResources>("StartRefillResources", t, nameof(Patches.StartRefillResources));

            harmony.AddPostfix<QuantumObject>("Update", t, nameof(Patches.QuantumObject_Update));
            harmony.AddPostfix<QuantumMoon>("GetRandomStateIndex", t, nameof(Patches.GetRandomStateIndex));

            harmony.AddPrefix<HUDCanvas>("UpdateGForce", t, nameof(Patches.UpdateGForce));
            //harmony.AddPostfix<HUDCanvas>("Start", t, nameof(Patches.Start));
        }

        //--------------------------------------------- Match Velocity ---------------------------------------------//
        public static bool StopMatchVelocity(Autopilot __instance)
        {
            //Log.Print($"STOP MATCH {__instance} {ContinuousMatchVelocity.matchVelocityShip}");

            if (__instance == ContinuousMatchVelocity.shipAutopilot) return !ContinuousMatchVelocity.matchVelocityShip;
            if (__instance == ContinuousMatchVelocity.playerAutopilot) return !ContinuousMatchVelocity.matchVelocityPlayer;

            return true;
        }

        public static void Update(ReferenceFrameGUI __instance)
        {
            if (Settings.Targeting == Targeting.Disabled)
            {
                if (__instance._showVisuals) __instance.SetVisibility(false);
            }
            else
            {
                if (!__instance._showVisuals) __instance.SetVisibility(__instance._showVisuals);
            }

            if (Settings.Targeting != Targeting.Smooth) return;

            if (Mathf.Abs(__instance._orientedRelativeVelocity.z) < 5f)
            {
                float t01 = 1f - Mathf.Clamp01( (Mathf.Max(1f, Mathf.Abs(__instance._orientedRelativeVelocity.z)) - 1f) / 5f );

                bool matchingVelocity = OWInput.IsInputMode(InputMode.ShipCockpit) ?
                    ContinuousMatchVelocity.matchVelocityShip : ContinuousMatchVelocity.matchVelocityPlayer;
                Color targetColor = matchingVelocity ? new Color(0.2f, 0.95f, 0.5f) : __instance._staticColor;
                __instance._reticuleColor = Color.Lerp(__instance._reticuleColor, targetColor, t01);

                LockOnReticule reticuleWithState = __instance.GetReticuleWithState(LockOnReticule.LockState.LOCK);
                if (reticuleWithState != null)
                {
                    reticuleWithState.SetColorWithoutAlpha(__instance._reticuleColor);
                }
            }
        }

        public static bool OnMatchedVelocity(AutopilotGUI __instance)
        {
            if ( (__instance._autopilot == ContinuousMatchVelocity.shipAutopilot && !ContinuousMatchVelocity.matchVelocityShip)
                || (__instance._autopilot == ContinuousMatchVelocity.playerAutopilot && !ContinuousMatchVelocity.matchVelocityPlayer) )
            {
                __instance._matchVelocityCompleteNotification.displayMessage = UITextLibrary.GetString(UITextType.NotificationVelocityAbort);
                return true;
            }

            if (!ContinuousMatchVelocity.showedMessage)
            {
                __instance._matchVelocityCompleteNotification.displayMessage = UITextLibrary.GetString(UITextType.NotificationVelocityMatching);
                ContinuousMatchVelocity.showedMessage = true;
                return true;
            }
            return false;
        }
        public static void FlyToDestination(Autopilot __instance, ReferenceFrame referenceFrame)
        {
            //Autopilot Retro-rockets sometimes fail from velocity match if don't do this
            __instance._stopMatchingNextFrame = false;
            ContinuousMatchVelocity.StopShipMatch();
            KeyboardThrottleControl.ResetShip();
        }

        public static bool OnUntargetReferenceFrame(ShipCockpitController __instance)
        {
            if (!PlayerState.IsInsideShip())
            {
                Log.Print("Keep ship matching (untarget)");
                return false;
            }
            return true;
        }
        public static bool OnTargetReferenceFrame(ShipCockpitController __instance, ReferenceFrame referenceFrame)
        {
            if (!PlayerState.IsInsideShip())
            {
                Log.Print("Keep ship matching (target)");
                return false;
            }
            return true;
        }

        public static void UpdateDistanceText(CanvasMarker __instance)
        {
            if (__instance._visualTarget != Locator.GetShipTransform()) return;
            if (__instance._stringBuilder == null) return;
            if (Locator.GetShipTransform() == null) return;
            if (!ContinuousMatchVelocity.matchVelocityShip) return;
            //if (ContinuousMatchVelocity.shipThruster._landingManager.IsLanded()) return;

            var rf = ContinuousMatchVelocity.shipAutopilot._referenceFrame;
            if (rf == null) return;

            string s = rf.GetHUDDisplayName();
            if (!string.IsNullOrWhiteSpace(s)) __instance._stringBuilder.Append($" -> {rf.GetHUDDisplayName()}");

            string result = __instance._stringBuilder.ToString();
            __instance._mainTextField.text = result;
            //__instance._offScreenIndicator.SetText(result);
        }

        //--------------------------------------------- Title Animator ---------------------------------------------//
        static float progression;
        static float smooth;
        public static void TitleAnimationController_Update(TitleAnimationController __instance)
        {
            //float loopBreak = Mathf.Clamp01(Time.timeSinceLevelLoad / 1320f);

            if (!Settings.TitleVariety) return;
            if (ModMain.titleWILDSParent == null) return;
            if (__instance._titleAnimator == null) return;
            if (Camera.current == null) return;
            if (TitleScreenVariation.optionsObj == null) return;

            Vector3 titlePos = ModMain.titleWILDSParent.position;
            Vector3 screenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            screenPos.z = Camera.current.nearClipPlane + 100f;
            Vector3 mousePos = Camera.current.ScreenToWorldPoint(screenPos);

            float d = Vector3.Distance(titlePos, mousePos);
            d *= 0.005f;
            if (TitleScreenVariation.optionsObj.activeSelf || OWInput.UsingGamepad()) d = 1f;
            d = 1f - Mathf.Clamp01(d);
            d += Mathf.Sin(Time.time) * 0.01f;
            d *= d;
            d *= d;

            if (d > progression)
            {
                progression = Mathf.SmoothDamp(progression, d + (d - progression) * 0.1f, ref smooth, 0.5f);
            }
            else progression = Mathf.SmoothDamp(progression, d, ref smooth, 2f);

            __instance._titleAnimator.SetFloat("Progression", progression);
        }

        /*
        //--------------------------------------------- Probe Charge Shot (unimplemented) ---------------------------------------------//
        static float probeCharge;
        public static void UpdatePreLaunch(ProbeLauncher __instance)
        {
            if (__instance._name == ProbeLauncher.Name.Player && (OWInput.IsNewlyPressed(InputLibrary.toolOptionLeft, InputMode.All) || OWInput.IsNewlyPressed(InputLibrary.toolOptionRight, InputMode.All)))
            {
                __instance._photoMode = !__instance._photoMode;
                __instance._effects.PlayChangeModeClip();
                return;
            }
            if (OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary, InputMode.All))
            {
                if (__instance.InPhotoMode())
                {
                    if (__instance._launcherGeometry != null)
                    {
                        __instance._launcherGeometry.SetActive(false);
                    }
                    __instance.TakeSnapshotWithCamera(__instance._preLaunchCamera);
                    if (__instance._launcherGeometry != null)
                    {
                        __instance._launcherGeometry.SetActive(true);
                        return;
                    }
                }
            }

            if (__instance.InPhotoMode()) return;

            if (OWInput.IsPressed(InputLibrary.toolActionPrimary, InputMode.All)) {
                if (__instance.AllowLaunchMode()) probeCharge += Time.deltaTime;
            }
            else probeCharge = 0f;

            probeCharge = Mathf.Clamp01(probeCharge);

            if (OWInput.IsNewlyReleased(InputLibrary.toolActionPrimary, InputMode.All)) return;

            if (probeCharge > 0.95f)
            {
                //Lauch sound
            }

            __instance.LaunchProbe();
        }
        */

        //--------------------------------------------- Throttle ---------------------------------------------//
        public static bool FixedUpdate(ThrusterController __instance)
        {
            float throttle = __instance is JetpackThrusterController ? KeyboardThrottleControl.PlayerThrottle : KeyboardThrottleControl.ShipThrottle;
            
            __instance._translationalInput = __instance.ReadTranslationalInput() * throttle;

            if (!__instance.AllowHorizontalThrust())
            {
                __instance._translationalInput.x = 0f;
                __instance._translationalInput.z = 0f;
            }
            __instance._thrusterModel.AddTranslationalInput(__instance._translationalInput);
            if (__instance._isRotationalThrustEnabled)
            {
                __instance._rotationalInput = __instance.ReadRotationalInput();
                __instance._thrusterModel.AddRotationalInput(__instance._rotationalInput);
            }

            return false;
        }
        public static void GetAxisValue(ref Vector2 __result, IInputCommands command, InputMode mask = InputMode.All)
        {
            if (command == InputLibrary.moveXZ && mask == (InputMode.Character | InputMode.NomaiRemoteCam))
            {
                __result *= KeyboardThrottleControl.PlayerThrottle; //Walking
            }
        }

        //--------------------------------------------- Map ---------------------------------------------//
        public static bool GetLocalMapPosition(Minimap __instance, ref Vector3 __result, Transform worldTransform)
        {
            if (AdvancedMinimap.minimapProxy == null) return true;

            Vector3 localPos = __instance._playerRulesetDetector.GetPlanetoidRuleset().transform.InverseTransformPoint(worldTransform.position);
            if (AdvancedMinimap.isBelowGround && worldTransform == __instance._playerTransform)
            {
                __result = localPos.normalized * 0.6f;
                return false;
            }

            //if (worldTransform == __instance.probe)

            localPos /= AdvancedMinimap.minimapProxy.radius;
            localPos *= 0.51f;
            __result = Vector3.Scale(localPos, __instance._globeMeshTransform.localScale);
            return false;
        }
        public static bool UpdateTrails(Minimap __instance)
        {
            //if (Settings.GiantsDeepMapScale < 1.45f) return true;

            //Make trails on GD drop more often
            if (PlayerState.InGiantsDeep() && Vector3.Angle(__instance._playerMarkerTransform.localPosition, __instance._lastPlayerTrailPos) > 2f)
            {
                Vector3 vec = __instance._lastPlayerTrailPos - __instance._playerMarkerTransform.localPosition;
                __instance._lastPlayerTrailPos = __instance._playerMarkerTransform.localPosition + vec.normalized * 10f;
            }
            return true;
        }
        public static bool UpdateMarkers(Minimap __instance)
        {
            // Disable ship and probe markers if not on same planetoid ruleset as player
            __instance._shipMarkerTransform.gameObject.SetActive(__instance._shipRulesetDetector != null && __instance._shipRulesetDetector.GetPlanetoidRuleset() == __instance._playerRulesetDetector.GetPlanetoidRuleset());
            __instance._probeMarkerTransform.gameObject.SetActive(__instance._probeRulesetDetector != null && __instance._probeRulesetDetector.GetPlanetoidRuleset() == __instance._playerRulesetDetector.GetPlanetoidRuleset());
            return true;
        }
        public static bool KeepMinimapDisabled(Minimap __instance)
        {
            return Settings.Minimap != MinimapSetting.Disabled;
        }

        //--------------------------------------------- Green Fuel ---------------------------------------------//
        public static bool StartRefillResources(PlayerResources __instance, bool fuel, bool health, bool dlcFuelTank)
        {
            if (fuel && dlcFuelTank)
            {
                JetpackColor.AddGreenFuel();
            }
            return true;
        }

        //--------------------------------------------- Blinking/Quantum Moon Fixes ---------------------------------------------//
        public static void QuantumObject_Update(QuantumObject __instance)
        {
            if (ManualBlinking.QuantumObjectsShouldRecollapse)
            {
                if (Locator.GetQuantumMoon() != null && Locator.GetQuantumMoon().IsPlayerInsideShrine())
                {
                    if (__instance is QuantumShrine) return; //Keep player inside properly
                }

                __instance.Collapse(true);
            }
        }

        public static void GetRandomStateIndex(QuantumMoon __instance, ref int __result)
        {
            if (ManualBlinking.PlanetPlayerWantsQMFor != -1)
            {
                Log.Print($"Forced moon for player {ManualBlinking.QuantumObjectsShouldRecollapse}");
                __result = ManualBlinking.PlanetPlayerWantsQMFor;
            }

            if (!__instance.IsPlayerInside()) return;  //Otherwise behave normally outside and in shrines.
            if (__instance.IsPlayerInsideShrine()) return;

            if (__result == QuantumMoon.EYE_INDEX)
            {
                var dir = (Locator.GetPlayerBody().GetPosition() - __instance.transform.position).normalized;
                float dot = Vector3.Dot(dir, __instance.transform.up);
                if (dot < 0.955f) //Re-roll if not at north pole
                {
                    __result = UnityEngine.Random.Range(0, 5);
                }
            }
        }

        //--------------------------------------------- G-Force in air fix ---------------------------------------------//
        public static bool UpdateGForce(HUDCanvas __instance)
        {
            if (Settings.GravityThrusterGauge == GravityThrusterGauges.Vanilla) return true;

            if (__instance._timeSinceGForceRefresh < 0.1f) //0.2f
            {
                __instance._timeSinceGForceRefresh += Time.deltaTime;
                return false;
            }

            var player = __instance._playerController;
            float acceleration = 0f;
            if (player.IsGrounded())
            {
                acceleration = player._normalAcceleration.magnitude * Mathf.Sign(Vector3.Dot(player._normalAcceleration, player._transform.up));
            }
            else if (Locator.GetPlayerRulesetDetector().GetPlanetoidRuleset() != null)
            {
                var planetBody = Locator.GetPlayerRulesetDetector().GetPlanetoidRuleset().GetAttachedOWRigidbody();
                acceleration = CalculateAcceleration(player, planetBody);
            }
            else if (PlayerState.InCloakingField())
            {
                var ringworldBody = Locator.GetRingWorldController().GetAttachedOWRigidbody();
                acceleration = CalculateAcceleration(player, ringworldBody);
            }

            string text = (acceleration / 12f).ToString("F1") + "g";
            __instance._gForceDisplay.text = text;
            __instance._timeSinceGForceRefresh = 0f;

            return false;
        }

        private static float CalculateAcceleration(PlayerCharacterController player, OWRigidbody planetBody)
        {
            float acceleration;
            Vector3 pointAcceleration = planetBody.GetPointAcceleration(player._transform.position);
            Vector3 forceAcceleration = player._forceDetector.GetForceAcceleration();
            acceleration = (pointAcceleration - forceAcceleration).magnitude; // Vector3.Dot(, player._transform.up);
            return acceleration;
        }
        /*
        public static void Start(HUDCanvas __instance)
        {
            HUDModification.Instance.PostHUDStart();
        }
        */
    }
}