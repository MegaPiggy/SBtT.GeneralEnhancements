using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GeneralEnhancements
{
    class ManualBlinking : Feature
    {
        PlayerCameraEffectController cameraEffectController;
        public ManualBlinking() {

        }

        float alarmStartTime;

        ShipAudioController shipAudioController;

        bool storeBlinkInput;

        InputMode previousInputMode; //Fixes sleeping while driving
        ScreenPrompt _wakePrompt;
        float _fastForwardMultiplier;
        float _fastForwardStartTime;

        List<QuantumSocket[]> allQMSockets;
        public override void LateInitialize()
        {
            alarmStartTime = -1f;
            shipAudioController = Object.FindObjectOfType<ShipAudioController>();
            cameraEffectController = Object.FindObjectOfType<PlayerCameraEffectController>();
            _wakePrompt = new ScreenPrompt(InputLibrary.interact, UITextLibrary.GetString(UITextType.WakeUpPrompt), 0, ScreenPrompt.DisplayState.Normal, false);
            ResetRecollapse();
            PlanetPlayerWantsQMFor = -1;

            if (Locator.GetQuantumMoon() != null)
            {
                var qm = Locator.GetQuantumMoon().transform.Find("Sector_QuantumMoon");
                QuantumSocket[] GetSockets(string name)
                {
                    var state = qm.Find(name);
                    if (state == null) { Log.Error($"QM {name} not found"); return null; }
                    return state.GetComponentsInChildren<QuantumSocket>(true);
                }

                allQMSockets = new List<QuantumSocket[]>() {
                    GetSockets("State_HT"),
                    GetSockets("State_TH"),
                    GetSockets("State_BH"),
                    GetSockets("State_GD"),
                    GetSockets("State_DB"),
                    GetSockets("State_EYE"),
                };
            }
            else allQMSockets = new List<QuantumSocket[]>();
        }

        public const float closeEyesDuration = 0.05f;
        public const float openEyesDuration = 0.45f;
        float blinkTimer;
        public enum BlinkState { Not, Blinking, WaitForRelease, Unblinking, ForwardingTime }
        BlinkState state;

        public static bool QuantumObjectsShouldRecollapse { get; private set; }
        bool waitForRecollapse;
        bool updateState;
        void ResetRecollapse()
        {
            updateState = false;
            waitForRecollapse = false;
            QuantumObjectsShouldRecollapse = false;
        }

        float timeOfLastBlink;
        public static int PlanetPlayerWantsQMFor { get; private set; }
        void CheckIfPlayerIsSeekingQuantumMoonchan()
        {
            PlanetPlayerWantsQMFor = -1;

            if ((PlayerState.IsInsideShip() || PlayerState.InZeroG()) && Locator.GetReferenceFrame() != null)
            {
                if (Time.timeSinceLevelLoad - timeOfLastBlink < 4f) // && Random.value > 0.75f)
                {
                    var orbit = Locator.GetReferenceFrame().GetOWRigidBody().GetComponentInChildren<QuantumOrbit>();
                    if (orbit != null)
                    {
                        PlanetPlayerWantsQMFor = orbit.GetStateIndex();
                    }
                }
            }
        }

        bool StartBlink()
        {
            if (PlayerState.UsingShipComputer()) return false;
            if (OWInput.UsingGamepad())
            {
                if (Locator.GetToolModeSwapper().GetToolMode() != ToolMode.None || OWInput.IsInputMode(InputMode.ShipCockpit)) {
                    return false;
                }
                return Gamepad.current.leftShoulder.wasPressedThisFrame;
            }
            return Keyboard.current[Key.B].wasPressedThisFrame;
        }
        bool ContinueBlink()
        {
            if (PlayerState.UsingShipComputer()) return false;
            if (OWInput.UsingGamepad())
            {
                if (Locator.GetToolModeSwapper().GetToolMode() != ToolMode.None || OWInput.IsInputMode(InputMode.ShipCockpit)) {
                    return false;
                }
                return Gamepad.current.leftShoulder.isPressed;
            }
            return Keyboard.current[Key.B].isPressed;
        }
        public override void Update()
        {
            if (alarmStartTime > 0f)
            {
                if (Time.timeSinceLevelLoad - alarmStartTime > 2f)
                {
                    if (shipAudioController != null) shipAudioController.StopAlarm();
                    alarmStartTime = -1f;
                }
            }

            switch (state)
            {
                case BlinkState.Not:

                    if (StartBlink() || storeBlinkInput)
                    {
                        if (Settings.BlinkSleep == BlinkSleep.Disabled) return;
                        if (PlayerState.OnQuantumMoon() && PlayerState.IsInsideShip()) return;

                        storeBlinkInput = false;
                        blinkTimer = 0f;
                        ResetRecollapse();
                        state = BlinkState.Blinking;
                        cameraEffectController.CloseEyes(closeEyesDuration);
                    }
                    break;
                case BlinkState.Blinking:

                    blinkTimer += Time.deltaTime;
                    if (blinkTimer > closeEyesDuration)
                    {
                        //GlobalMessenger.FireEvent("PlayerBlink"); //Want to re-collapse all quantum objects.
                        QuantumObjectsShouldRecollapse = true;

                        if (waitForRecollapse)
                        {
                            Log.Print("Blink");
                            CheckIfPlayerIsSeekingQuantumMoonchan();
                            timeOfLastBlink = Time.timeSinceLevelLoad;
                            ResetRecollapse();
                            state = BlinkState.WaitForRelease;
                        }
                        waitForRecollapse = true;
                    }
                    break;
                case BlinkState.WaitForRelease:

                    if (updateState)
                    {
                        TeleportPlayerToSafeQMPosition();
                        FixQuantumState();
                    }
                    if (!updateState)
                    {
                        updateState = true;
                        return;
                    }

                    blinkTimer += Time.deltaTime;
                    if (blinkTimer > 2f && Settings.BlinkSleep == BlinkSleep.Enabled
                        && LoadManager.GetCurrentScene() != OWScene.EyeOfTheUniverse && !PlayerState.InDreamWorld())
                    {
                        StartSleeping();
                        forceWakeUp = false;
                        state = BlinkState.ForwardingTime;
                    }
                    else if (!ContinueBlink())
                    {
                        blinkTimer = 0f;
                        state = BlinkState.Unblinking;
                        cameraEffectController.OpenEyes(openEyesDuration);
                    }
                    break;
                case BlinkState.Unblinking:
                    blinkTimer += Time.deltaTime;
                    if (blinkTimer > openEyesDuration)
                    {
                        state = BlinkState.Not;
                    }
                    if (StartBlink()) storeBlinkInput = true;
                    break;

                case BlinkState.ForwardingTime:

                    ResetRecollapse();

                    if (ShouldWakeUp(out bool sudden))
                    {
                        StopSleeping(sudden);
                        state = BlinkState.Not;
                        return;
                    }

                    if (!OWTime.IsPaused())
                    {
                        _fastForwardMultiplier = Mathf.MoveTowards(_fastForwardMultiplier, 10f, 2f * Time.unscaledDeltaTime);
                        OWTime.SetTimeScale(_fastForwardMultiplier);
                    }
                    break;
            }
        }


        //--------------------------------------------- Quantum Moon Fixes ---------------------------------------------//
        QuantumSocket closestQMSocket;
        void TeleportPlayerToSafeQMPosition()
        {
            QuantumMoon qm = Locator.GetQuantumMoon();
            closestQMSocket = null;

            if (qm == null) return;
            if (!qm.IsPlayerInside()) return;  //Behave normally outside and in shrines.
            if (qm.IsPlayerInsideShrine()) return;
            if (PlayerState.IsInsideShuttle()) return; //Make player teleport with shuttle

            var qmTF = qm.transform;

            /*
            //Find if player can exist here, go to socket if can't?
            bool groundBelow = false;
            var pos = Locator.GetPlayerBody().GetPosition();
            RaycastHit hitInfo;
            if (RaycastToQM(qmTF, pos, out hitInfo))
            {
                groundBelow = true;
            }
            */

            var pos = Locator.GetPlayerTransform().position;
            if (qm._stateIndex == QuantumMoon.EYE_INDEX) //Other Eye shrine sockets aren't any good.
            {
                var northPole = qm._shrine._northSocket;
                SetPlayerPos(northPole, qm);
                return;
            }

            var sockets = qm._shrine._socketList;
            float closestDist = float.MaxValue;
            foreach (var socket in sockets)
            {
                float dist = (pos - socket.transform.position).sqrMagnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestQMSocket = socket;
                }
            }
            //Log.Print($"Dist: {closestDist}");

            if (closestDist > 8f) //Allow warping to any socket if not too close to shrine/shuttle socket.
            {
                QuantumSocket[] currentSockets = allQMSockets[qm.GetStateIndex()];
                //Log.Print($"Count for state {qm.GetStateIndex()}: {currentSockets.Length}");

                var oldClosestQMSocket = closestQMSocket;
                float oldClosestDist = closestDist;
                closestDist = float.MaxValue;
                foreach (var socket in currentSockets)
                {
                    if (socket.IsOccupied() || !socket.IsActive()) continue;

                    float dist = (pos - socket.transform.position).sqrMagnitude;
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestQMSocket = socket;
                    }
                }
                if (closestDist > oldClosestDist) closestQMSocket = oldClosestQMSocket; //Check if actually closer
            }

            if (closestQMSocket == null) return;
            SetPlayerPos(closestQMSocket, qm);
        }
        void SetPlayerPos(QuantumSocket socket, QuantumMoon qm)
        {
            //Get Position
            var pos = socket.transform.position;
            var qmTF = qm.transform;
            var qmPos = qmTF.position;
            var upVec = (pos - qmPos);
            var upDir = upVec.normalized;
            var raycastOrigin = pos + upDir * 25f;
            if (Physics.Raycast(raycastOrigin, -upDir, out RaycastHit hitInfo, upVec.magnitude, OWLayerMask.groundMask))
            {
                pos = hitInfo.point;    //Fix instances of sockets being in the ground.
            }
            else
            {
                Log.Print("Failed to find ground point");
            }
            pos += upDir;

            //Get Velocity
            var qmRB = qm.GetAttachedOWRigidbody();
            var playerPos = Locator.GetPlayerTransform().position;
            Vector3 velocityOffset = Locator.GetPlayerBody().GetVelocity() - qmRB.GetPointVelocity(playerPos);

            //Set Player
            Locator.GetPlayerBody().SetPosition(pos);
            Locator.GetPlayerBody().SetRotation(Quaternion.LookRotation(Locator.GetPlayerTransform().forward, upDir));
            Locator.GetPlayerBody().SetVelocity(qmRB.GetPointVelocity(socket.transform.position) + velocityOffset);
            GlobalMessenger.FireEvent("WarpPlayer"); //Fix shuttle volumes being stupid.
        }

        void FixQuantumState()
        {
            var qm = Locator.GetQuantumMoon();
            if (qm == null) return;
            if (!qm.IsPlayerInside()) return;
            if (PlayerState.IsInsideShuttle()) return;
            if (closestQMSocket == null) return;

            var shrine = qm._shrine;

            /*
            var shuttle = qm.GetComponentInChildren<NomaiShuttleController>(); //Spawning on top now, so not needed.
            if (shuttle != null)
            {
                if (Vector3.Distance(shuttle.transform.position, closestQMSocket.transform.position) < 15f)
                {
                    //Causes issues if blink while tractor beaming
                    shuttle._tractorBeam.SetActivation(false, false);
                }
            }
            */

            //Log.Print($"{shrine.GetCurrentSocket()} | {closestQMSocket}");

            shrine._fading = true;  //Is this ever turned back to false?

            if (shrine.GetCurrentSocket() == closestQMSocket)
            {
                Log.Print("Spawned inside shrine");
                shrine._triggerVolume.AddObjectToVolume(Locator.GetPlayerDetector()); //Make player exit properly
                shrine._isPlayerInside = true;
                shrine._exteriorLightController.FadeTo(0f, 0f);

                shrine._fadeFraction = shrine._isPlayerInside ? (shrine._gate.GetOpenFraction() * 0.7f) : 1f;
                shrine._ambientLight.intensity = shrine._origAmbientIntensity * shrine._fadeFraction;
                shrine._fogOverride.tint = Color.Lerp(Color.black, shrine._origFogTint, shrine._fadeFraction);
            }

            closestQMSocket = null;
        }



        //--------------------------------------------- Sleeping ---------------------------------------------//
        static bool forceWakeUp;
        public static void ForceWakeUp()
        {
            forceWakeUp = true;
        }
        void WakeyWakey()
        {
            if (shipAudioController == null) return;
            if (shipAudioController._alarmSource.isPlaying) return;

            shipAudioController.PlayAlarm();
            alarmStartTime = Time.timeSinceLevelLoad;
        }
        bool ShouldWakeUp(out bool sudden)
        {
            sudden = true;
            if (Locator.GetDeathManager().IsPlayerDying()) return true;

            if (PlayerState.IsInsideShip())
            {
                float sunDist = Vector3.Distance(Locator.GetPlayerTransform().position, Locator.GetSunTransform().position);
                if (forceWakeUp || sunDist < Locator.GetSunController().GetSurfaceRadius() * 1.2f)
                {
                    WakeyWakey();
                    return true;
                }
            }

            sudden = false;
            return ( OWInput.IsInputMode(InputMode.None) &&
                (OWInput.IsNewlyPressed(InputLibrary.interact, InputMode.All) ||
                OWInput.IsNewlyPressed(InputLibrary.cancel, InputMode.All) ||
                OWInput.IsNewlyPressed(InputLibrary.interactSecondary, InputMode.All)) ) ||
                TimeLoop.GetSecondsRemaining() < 85f;
        }

        void StartSleeping()
        {
            Locator.GetToolModeSwapper().UnequipTool();

            bool nearDreamCampfire = Locator.GetDeathManager()._nearbyDreamFire != null;

            Locator.GetAudioMixer().MixSleepAtCampfire(3f);
            Locator.GetPlayerAudioController().OnStartSleepingAtCampfire(nearDreamCampfire);
		    _fastForwardStartTime = Time.timeSinceLevelLoad;
            Locator.GetPromptManager().AddScreenPrompt(_wakePrompt, PromptPosition.Center, true);
            previousInputMode = OWInput.GetInputMode();
            OWInput.ChangeInputMode(InputMode.None);

            GlobalMessenger<bool>.FireEvent("StartSleepingAtCampfire", nearDreamCampfire);

            StartFastForwarding();
        }

        void StartFastForwarding()
        {
            _fastForwardMultiplier = 1f;
            Locator.GetPlayerCamera().enabled = false;
            OWTime.SetMaxDeltaTime(0.033333335f);
            GlobalMessenger.FireEvent("StartFastForward");
        }

        void StopSleeping(bool sudden = false)
        {
            StopFastForwarding();
            
            cameraEffectController.OpenEyes(1f, sudden);
            Locator.GetAudioMixer().UnmixSleepAtCampfire(sudden ? 1f : 3f);
            Locator.GetPlayerAudioController().OnStopSleepingAtCampfire(sudden || Time.timeSinceLevelLoad - _fastForwardStartTime > 60f, sudden);
            Locator.GetPromptManager().RemoveScreenPrompt(_wakePrompt);
            OWInput.ChangeInputMode(previousInputMode);

            GlobalMessenger.FireEvent("StopSleepingAtCampfire");

            if (Locator.GetDeathManager()._nearbyDreamFire != null)
            {
                Locator.GetDeathManager()._nearbyDreamFire.WakeInDreamWorld();
            }
        }

        private void StopFastForwarding()
        {
            Locator.GetPlayerCamera().enabled = true;
            OWTime.SetTimeScale(1f);
            OWTime.SetMaxDeltaTime(0.06666667f);
            GlobalMessenger.FireEvent("EndFastForward");
        }
    }
}