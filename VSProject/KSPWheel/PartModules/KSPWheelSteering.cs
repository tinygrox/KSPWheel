﻿using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace KSPWheel
{
    public class KSPWheelSteering : KSPWheelSubmodule
    {

        /// <summary>
        /// Name of the transform that will be rotated for visual steering effect
        /// </summary>
        [KSPField]
        public string steeringName = string.Empty;
        
        /// <summary>
        /// Maximum deflection angle of the steering transform, measured from its default state (rotation = 0,0,0)
        /// </summary>
        [KSPField]
        public float maxSteeringAngle = 0f;

        /// <summary>
        /// If true the steering will be locked to zero and will not respond to steering input.
        /// </summary>
        [KSPField(guiName = "#KSPWheel_SteeringLock", guiActive = true, guiActiveEditor = true, isPersistant = true), // Steering Lock
         UI_Toggle(enabledText = "#KSPWheel_SteeringLock_Locked", disabledText = "#KSPWheel_SteeringLock_Free", suppressEditorShipModified = true, affectSymCounterparts = UI_Scene.Editor)] // Locked | Free
        public bool steeringLocked;

        /// <summary>
        /// If true, steering will be inverted for this wheel.  Toggleable in editor and flight.  Persistent.
        /// </summary>
        [KSPField(guiName = "#KSPWheel_InvertSteering", guiActive = true, guiActiveEditor = true, isPersistant = true), // Invert Steering
         UI_Toggle(enabledText = "#autoLOC_6001077", disabledText = "#autoLOC_6001075", suppressEditorShipModified = true, affectSymCounterparts = UI_Scene.Editor)] // Inverted | Normal
        public bool invertSteering = false;

        [KSPField(guiName = "#KSPWheel_SteeringLimitLow", guiActive = true, guiActiveEditor = true, isPersistant = true), // Steering Limit Low
         UI_FloatRange(minValue = 0, maxValue = 1, stepIncrement = 0.01f, suppressEditorShipModified = true, affectSymCounterparts = UI_Scene.Editor)]
        public float steeringLimit = 1f;

        [KSPField(guiName = "#KSPWheel_SteeringLimitHigh", guiActive = true, guiActiveEditor = true, isPersistant = true), // Steering Limit High
         UI_FloatRange(minValue = 0, maxValue = 1, stepIncrement = 0.01f, suppressEditorShipModified = true, affectSymCounterparts = UI_Scene.Editor)]
        public float steeringLimitHigh = 1f;

        [KSPField(guiName = "#KSPWheel_SteeringResponse", guiActive = true, guiActiveEditor = true, isPersistant = true), // Steering Response
         UI_FloatRange(minValue = 0, maxValue = 1, stepIncrement = 0.01f, suppressEditorShipModified = true, affectSymCounterparts = UI_Scene.Editor)]
        public float steeringResponse = 1f;

        [KSPField(guiName = "#KSPWheel_SteeringBias", guiActive = true, guiActiveEditor = true, isPersistant = true), // Steering Bias
         UI_FloatRange(minValue = -1, maxValue = 1, stepIncrement = 0.025f, suppressEditorShipModified = true, affectSymCounterparts = UI_Scene.Editor)]
        public float steeringBias = 0f;

        /// <summary>
        /// If true, will update the wheels internal steering values.  If false, will only update the visible steering mesh (if specified).
        /// </summary>
        [KSPField]
        public bool updateWheelSteering = true;
        
        /// <summary>
        /// The local axis of the steering transform to rotate around.  Defaults to 0, 1, 0 -- rotate around y+ axis, with z+ facing forward.
        /// </summary>
        [KSPField]
        public Vector3 steeringAxis = Vector3.up;

        [KSPField]
        public bool useSteeringCurve = true;

        [KSPField]
        public FloatCurve steeringCurve = new FloatCurve();

        [KSPField]
        public bool showGUISteerLock = true;
        [KSPField]
        public bool showGUISteerInvert = true;
        [KSPField]
        public bool showGUISteerBias = true;
        [KSPField]
        public bool showGUISteerResponse = true;
        [KSPField]
        public bool showGUISteerLimit = true;

        private Transform steeringTransform;
        private Quaternion defaultRotation;
        private float rotInput;

        internal void onSteeringLocked(BaseField field, System.Object obj)
        {
            this.wheelGroupUpdate(int.Parse(controller.wheelGroup), m =>
            {
                m.steeringLocked = steeringLocked;
            });
        }

        internal void onSteeringInverted(BaseField field, System.Object obj)
        {
            this.wheelGroupUpdate(int.Parse(controller.wheelGroup), m =>
            {
                m.invertSteering = invertSteering;
            });
        }

        internal void onSteeringLimitUpdated(BaseField field, System.Object obj)
        {
            this.wheelGroupUpdate(int.Parse(controller.wheelGroup), m =>
            {
                m.steeringLimit = steeringLimit;
                m.steeringLimitHigh = steeringLimitHigh;
                m.steeringResponse = steeringResponse;
            });
        }

        internal void onSteeringBiasUpdated(BaseField field, System.Object obj)
        {
            this.wheelGroupUpdate(int.Parse(controller.wheelGroup), m =>
            {
                m.steeringBias = steeringBias;
                m.updateUIFloatEditControl(nameof(m.steeringBias), m.steeringBias);
            });
        }

        [KSPAction("#KSPWheel_Action_LockSteering")] // Lock Steering
        public void steeringLockAction(KSPActionParam param)
        {
            steeringLocked = !steeringLocked;
        }

        [KSPAction("#KSPWheel_Action_InvertSteering")] // Invert Steering
        public void steeringInvertAction(KSPActionParam param)
        {
            invertSteering = !invertSteering;
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            Fields[nameof(steeringLocked)].uiControlFlight.onFieldChanged = onSteeringLocked;
            Fields[nameof(invertSteering)].uiControlFlight.onFieldChanged = onSteeringInverted;
            Fields[nameof(steeringLimit)].uiControlFlight.onFieldChanged = onSteeringLimitUpdated;
            Fields[nameof(steeringLimitHigh)].uiControlFlight.onFieldChanged = onSteeringLimitUpdated;
            Fields[nameof(steeringResponse)].uiControlFlight.onFieldChanged = onSteeringLimitUpdated;
            Fields[nameof(steeringBias)].uiControlFlight.onFieldChanged = onSteeringBiasUpdated;
        }

        internal override string getModuleInfo()
        {
            return Localizer.Format("#KSPWheel_SteeringModuleInfo", maxSteeringAngle); // "Max Steering Deflection: " + maxSteeringAngle + " degrees"
        }

        internal override void onUIControlsUpdated(bool show)
        {
            base.onUIControlsUpdated(show);
            Fields[nameof(steeringLocked)].guiActive = Fields[nameof(steeringLocked)].guiActiveEditor = show && showGUISteerLock;
            Fields[nameof(invertSteering)].guiActive = Fields[nameof(invertSteering)].guiActiveEditor = show && showGUISteerInvert;
            Fields[nameof(steeringLimit)].guiActive = Fields[nameof(steeringLimit)].guiActiveEditor = show && showGUISteerLimit;
            Fields[nameof(steeringLimitHigh)].guiActive = Fields[nameof(steeringLimitHigh)].guiActiveEditor = show && showGUISteerLimit;
            Fields[nameof(steeringResponse)].guiActive = Fields[nameof(steeringResponse)].guiActiveEditor = show && showGUISteerResponse;
            Fields[nameof(steeringBias)].guiActive = Fields[nameof(steeringBias)].guiActiveEditor = show && showGUISteerBias;
        }

        internal override void postControllerSetup()
        {
            base.postControllerSetup();
            if (!string.IsNullOrEmpty(steeringName))
            {
                steeringTransform = part.transform.FindChildren(steeringName)[wheelData.indexInDuplicates];
                defaultRotation = steeringTransform.localRotation;
            }
            if (steeringCurve == null || steeringCurve.Curve.length == 0)
            {
                steeringCurve = new FloatCurve();
                steeringCurve.Add(0, 1f, -0.9f, -0.9f);
                steeringCurve.Add(1, 0.1f, -0.9f, -0.9f);
            }
        }

        internal override void preWheelPhysicsUpdate()
        {
            base.preWheelPhysicsUpdate();
            float rI = -(part.vessel.ctrlState.wheelSteer + part.vessel.ctrlState.wheelSteerTrim);
            if (steeringLocked) { rI = 0; }
            if (invertSteering) { rI = -rI; }
            if (rI < 0)
            {
                rI = rI * (1 - steeringBias);
            }
            if (rI > 0)
            {
                rI = rI * (1 + steeringBias);
            }
            rI = Mathf.Clamp(rI, -1, 1);
            rotInput = Mathf.MoveTowards(rotInput, rI, steeringResponse);
            float perc = Mathf.Clamp01(Mathf.Abs(wheel.wheelLocalVelocity.z) / (controller.maxSpeed * controller.wheelMaxSpeedScalingFactor));
            float limit = ((1 - perc) * steeringLimit) + (perc * steeringLimitHigh);
            if (useSteeringCurve)
            {
                limit *= steeringCurve.Evaluate(perc);
            }
            if (updateWheelSteering)
            {
                wheel.steeringAngle = maxSteeringAngle * rotInput * limit;
            }
        }

        public void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight || steeringTransform == null || wheel==null) { return; }
            steeringTransform.localRotation = defaultRotation;
            if (controller.wheelState == KSPWheelState.DEPLOYED)
            {
                steeringTransform.Rotate(wheel.steeringAngle * steeringAxis, Space.Self);
            }
        }

    }
}
