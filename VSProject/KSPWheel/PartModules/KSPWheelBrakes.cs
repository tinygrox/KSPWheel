﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSPWheel
{
    /// <summary>
    /// Replacement for stock wheel motor module.<para/>
    /// Manages wheel motor input and resource use.
    /// TODO:
    /// Traction control / anti-slip.
    /// Torque curve vs rpm.
    /// </summary>
    public class KSPWheelBrakes : KSPWheelSubmodule
    {

        [KSPField]
        public float maxBrakeTorque = 0f;

        [KSPField]
        public float brakeResponse = 0f;
        
        [KSPField]
        public bool brakesLocked = false;

        [KSPField(guiName = "#KSPWheel_BrakesLimit", guiActive = true, guiActiveEditor = true, isPersistant = true), //Brakes Limit
         UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 0.5f, suppressEditorShipModified = true)]
        public float brakeLimit = 100f;

        [KSPField]
        public bool showGUIBrakeLimit = true;

        public float torqueOutput;

        private float brakeInput;
        private ModuleStatusLight statusLightModule;

        private void brakeLimitUpdated(BaseField field, System.Object obj)
        {
            this.wheelGroupUpdate(int.Parse(controller.wheelGroup), m =>
            {
                m.brakeLimit = brakeLimit;
            });
        }

        [KSPAction(actionGroup = KSPActionGroup.Brakes, guiName = "#KSPWheel_Brakes")] // Brakes
        public void brakesAction(KSPActionParam p)
        {
            //NOOP - blank method stub to get an AG action to show up in the AG list...
            //what a stupid system....
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            Fields[nameof(brakeLimit)].uiControlEditor.onFieldChanged = Fields[nameof(brakeLimit)].uiControlFlight.onFieldChanged = brakeLimitUpdated;
        }

        public void Start()
        {
            statusLightModule = part.GetComponent<ModuleStatusLight>();
        }

        internal override string getModuleInfo()
        {
            return LocalizationCache.str_ModuleInfo_BrakeTorque + " " + maxBrakeTorque; // Brake Torque:
        }

        internal override void onUIControlsUpdated(bool show)
        {
            base.onUIControlsUpdated(show);
            Fields[nameof(brakeLimit)].guiActive = Fields[nameof(brakeLimit)].guiActiveEditor = show && !brakesLocked && showGUIBrakeLimit;
        }

        internal override void preWheelPhysicsUpdate()
        {
            base.preWheelPhysicsUpdate();
            float bI = brakesLocked ? 1 : part.vessel.ActionGroups[Actions[nameof(brakesAction)].actionGroup] ? 1 : 0;
            if (!brakesLocked && brakeResponse > 0 && bI > 0)
            {
                bI = Mathf.Lerp(brakeInput, bI, brakeResponse * Time.deltaTime);
            }

            brakeInput = bI;
            torqueOutput = wheel.brakeTorque = maxBrakeTorque * brakeInput * controller.motorTorqueScalingFactor * (brakeLimit * 0.01f);
        }

        internal override void preWheelFrameUpdate()
        {
            base.preWheelFrameUpdate();
            if (statusLightModule != null)
            {
                statusLightModule.SetStatus(brakeInput != 0);
            }
        }

    }
}
