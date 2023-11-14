﻿using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.Localization;

namespace KSPWheel
{

    public class KSPWheelDamage : KSPWheelSubmodule
    {
                
        [KSPField]
        public string wheelName = "wheel";

        [KSPField]
        public string bustedWheelName = "bustedWheel";

        [KSPField]
        public int repairLevel = 3;

        [KSPField(guiName = "#KSPWheel_MaxSafeSpeed", guiActive = true, guiActiveEditor = true, guiUnits = "m/s", guiFormat = "F2")] // Max Safe Speed
        public float maxSafeSpeed = 0f;

        [KSPField(guiName = "#KSPWheel_MaxSafeLoad", guiActive = true, guiActiveEditor = true, guiUnits = "t", guiFormat = "F2")] // Max Safe Load
        public float maxSafeLoad = 0f;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "#KSPWheel_WheelStatus")] // Wheel Status: 
        public string displayStatus = LocalizationCache.str_WheelStatus_Operational; // "Operational"

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "#KSPWheel_WheelStress", guiFormat = "F2"), // Wheel Stress
         UI_ProgressBar(minValue = 0, maxValue = 1.5f, suppressEditorShipModified = true, scene = UI_Scene.Flight)]
        public float loadStress = 0f;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "#KSPWheel_FailureTime", guiFormat = "F2"), // Failure Time
         UI_ProgressBar(minValue = 0, maxValue = 1, suppressEditorShipModified = true, scene = UI_Scene.Flight)]
        public float stressTime = 0f;

        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "#KSPWheel_WheelWear", guiFormat = "F2", isPersistant = true), // Wheel Wear
         UI_ProgressBar(minValue = 0, maxValue = 1, suppressEditorShipModified = true, scene = UI_Scene.Flight)]
        public float wheelWear = 0f;

        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "#KSPWheel_MotorWear", guiFormat = "F2", isPersistant = true), // Motor Wear
         UI_ProgressBar(minValue = 0, maxValue = 1, suppressEditorShipModified = true, scene = UI_Scene.Flight)]
        public float motorWear = 0f;

        [KSPField(guiActive = false, guiActiveEditor = false, guiName = "#KSPWheel_SuspensionWear", guiFormat = "F2", isPersistant = true), // Suspension Wear
         UI_ProgressBar(minValue = 0, maxValue = 1, suppressEditorShipModified = true, scene = UI_Scene.Flight)]
        public float suspensionWear = 0f;

        private float speed = 0f;
        private float load = 0f;
        private float invulnerableTime = 0f;

        private float[] defaultRollingResistance;//per wheel collider rolling resistance tracking
        private float[] defaultMotorEfficiency;//per-motor-module efficiency tracking
        
        private Transform[] wheelMeshes;
        private Transform[] bustedWheelMeshes;

        private KSPWheelMotor[] motors;
        
        [KSPEvent(guiName = "#KSPWheel_RepairWheel", guiActive = false, guiActiveEditor = false, guiActiveUnfocused = false, externalToEVAOnly = true, unfocusedRange = 8f)] // Repair Wheel/Gear
        public void repairWheel()
        {
            KSPWheelWearType wearType = HighLogic.CurrentGame.Parameters.CustomParams<KSPWheelSettings>().wearType;
            if (controller.wheelState == KSPWheelState.BROKEN || (controller.wheelState == KSPWheelState.DEPLOYED && wearType == KSPWheelWearType.ADVANCED))
            {
                MonoBehaviour.print("Repairing wheel!");
                switch (wearType)
                {
                    case KSPWheelWearType.NONE:
                        break;
                    case KSPWheelWearType.SIMPLE:
                        changeWheelState(KSPWheelState.DEPLOYED);
                        invulnerableTime += 5f;
                        controller.wheelRepairTimer = 0.0001f;
                        MonoBehaviour.print("Repaired wheel Simple");
                        break;
                    case KSPWheelWearType.ADVANCED:
                        if (HighLogic.CurrentGame.Parameters.CustomParams<GameParameters.AdvancedParams>().KerbalExperienceEnabled(HighLogic.CurrentGame.Mode) && FlightGlobals.ActiveVessel.VesselValues.RepairSkill.value < repairLevel)
                        {
                            ScreenMessages.PostScreenMessage(Localizer.Format("#KSPWheel_RepairWheel_Message", controller.wheelType.ToLower(), repairLevel)); // "Crew member has insufficient repair skill to fix this " + controller.wheelType.ToLower() + "\nLevel " + repairLevel + " or higher is required."
                            return;
                        }
                        changeWheelState(KSPWheelState.DEPLOYED);
                        motorWear = 0f;
                        wheelWear = 0f;
                        suspensionWear = 0f;
                        invulnerableTime += 5f;
                        controller.wheelRepairTimer = 0.0001f;
                        MonoBehaviour.print("Repaired wheel.  Damage: " + motorWear + " : " + wheelWear + " : " + suspensionWear);
                        //TODO -- add a delay before repairing based on how damaged things were
                        break;
                    default:
                        break;
                }
                changeWheelState(KSPWheelState.DEPLOYED);
                updateWheelMeshes(controller.wheelState);
                updateDisplayState();
            }
        }

        public void Start()
        {
            motors = this.getControllerSubmodules<KSPWheelMotor>();
            int len = motors.Length;
            defaultMotorEfficiency = new float[len];
            for (int i = 0; i < len; i++)
            {
                defaultMotorEfficiency[i] = motors[i].motorEfficiency;
            }

            len = controller.wheelData.Length;
            defaultRollingResistance = new float[len];
            for (int i = 0; i < len; i++)
            {
                defaultRollingResistance[i] = controller.wheelData[i].wheel.rollingResistance;
            }
        }

        public override void OnIconCreate()
        {
            base.OnIconCreate();
            if (!String.IsNullOrEmpty(wheelName))
            {
                wheelMeshes = part.transform.FindChildren(wheelName);
            }
            if (!String.IsNullOrEmpty(bustedWheelName))
            {
                bustedWheelMeshes = part.transform.FindChildren(bustedWheelName);
            }
            //clear out broken wheel meshes from icon rendering
            updateWheelMeshes(KSPWheelState.DEPLOYED);
        }

        internal override void onUIControlsUpdated(bool show)
        {
            base.onUIControlsUpdated(show);
        }

        internal override void onScaleUpdated()
        {
            base.onScaleUpdated();
            maxSafeSpeed = controller.maxSpeed * controller.wheelMaxSpeedScalingFactor;
            maxSafeLoad = controller.maxLoadRating * controller.wheelMaxLoadScalingFactor;
        }

        internal override void postControllerSetup()
        {
            base.postControllerSetup();
            if (!String.IsNullOrEmpty(wheelName))
            {
                wheelMeshes = part.transform.FindChildren(wheelName);
            }
            if (!String.IsNullOrEmpty(bustedWheelName))
            {
                bustedWheelMeshes = part.transform.FindChildren(bustedWheelName);
            }
            updateWheelMeshes(controller.wheelState);
            updateDisplayState();
            onScaleUpdated();
            //TODO -- update stats for initial persistent wear setup
        }

        internal override void postWheelPhysicsUpdate()
        {
            base.postWheelPhysicsUpdate();
            if (invulnerableTime > 0)
            {
                invulnerableTime -= Time.fixedDeltaTime;
                return;
            }
            if (controller.wheelState != KSPWheelState.DEPLOYED)
            {
                return;
            }
            switch (HighLogic.CurrentGame.Parameters.CustomParams<KSPWheelSettings>().wearType)
            {
                case KSPWheelWearType.NONE:
                    //NOOP
                    break;
                case KSPWheelWearType.SIMPLE:
                    wearUpdateSimple();
                    break;
                case KSPWheelWearType.ADVANCED:
                    wearUpdateAdvanced();
                    break;
                default:
                    //NOOP
                    break;
            }
        }

        private void wearUpdateSimple()
        {
            // -- SIMPLE MODE LOAD HANDLING --
            load = 0f;
            int len = controller.wheelData.Length;
            for (int i = 0; i < len; i++)
            {
                load += controller.wheelData[i].wheel.springForce / 10f;
            }
            loadStress = load / maxSafeLoad;
            if (load > maxSafeLoad)
            {
                float overStress = (load / maxSafeLoad) - 1f;
                stressTime += Time.fixedDeltaTime * overStress * HighLogic.CurrentGame.Parameters.CustomParams<KSPWheelWearSettings>().stressDamageMultiplier * 0.25f;
            }

            // -- SIMPLE MODE SPEED HANDLING --
            speed = 0f;
            for (int i = 0; i < len; i++)
            {
                speed += Mathf.Abs(controller.wheelData[i].wheel.linearVelocity) / TimeWarp.CurrentRate;
            }
            speed /= controller.wheelData.Length;
            if (speed > maxSafeSpeed )
            {
                float overSpeedPercent = (speed / maxSafeSpeed) - 1f;
                stressTime += Time.fixedDeltaTime * overSpeedPercent * HighLogic.CurrentGame.Parameters.CustomParams<KSPWheelWearSettings>().speedDamageMultiplier;
            }

            // -- SIMPLE MODE BREAKAGE HANDLING --
            if (stressTime >= 1.0f)
            {
                MonoBehaviour.print("Wheel broke from overstressing! load: " + load + " max: " + maxSafeLoad + " speed: " + speed + " maxSpeed: " + maxSafeSpeed);
                ScreenMessages.PostScreenMessage(Localizer.Format("#KSPWheel_WheelBreak_Message", this.part), 5f, ScreenMessageStyle.UPPER_LEFT); // "<color=orange><b>[" + this.part + "]:</b> Broke from overstressing.</color>"
                changeWheelState(KSPWheelState.BROKEN);
                stressTime = 0f;
                updateWheelMeshes(controller.wheelState);
                updateDisplayState();
            }
            if (speed < maxSafeSpeed && load < maxSafeLoad)
            {
                stressTime = Mathf.Max(0, stressTime - Time.fixedDeltaTime);
            }
        }

        private void wearUpdateAdvanced()
        {
            wearUpdateSimple();
            // -- ADVANCED MODE MOTOR WEAR UPDATING --
            int len = motors.Length;
            float heatProduction = 0f;
            for (int i = 0; i < len; i++)
            {
                //TODO - this should be reduced by the motors 'min power' figure, the amount of power that is actually consumed to maintain the magnetic field
                heatProduction += (motors[i].powerInKW - motors[i].powerOutKW) * HighLogic.CurrentGame.Parameters.CustomParams<KSPWheelWearSettings>().motorHeatMultiplier;
            }
            part.AddThermalFlux(heatProduction);
            //TODO these should both be config fields
            float heatTolerance = 400f;
            float peakDamageHeat = 1000f;
            if (part.temperature > heatTolerance)
            {
                float heatWear = (float)part.temperature - heatTolerance / (peakDamageHeat - heatTolerance);
                motorWear += heatWear * Time.fixedDeltaTime * HighLogic.CurrentGame.Parameters.CustomParams<KSPWheelWearSettings>().motorDamageMultiplier;
                len = motors.Length;
                for (int i = 0; i < len; i++)
                {
                    motors[i].motorEfficiency = defaultMotorEfficiency[i] * (1f - motorWear);
                }
            }

            // -- ADVANCED MODE SPEED WEAR UPDATING --
            float speedPercent = Mathf.Pow(Mathf.Max((speed / maxSafeSpeed) - 0.75f, 0), 4);
            if (speedPercent > 0)
            {
                wheelWear += speedPercent * Time.fixedDeltaTime * 0.05f * HighLogic.CurrentGame.Parameters.CustomParams<KSPWheelWearSettings>().speedDamageMultiplier;//should give ~80 minutes at max speed before wear hits 1.0
                len = controller.wheelData.Length;
                for (int i = 0; i < len; i++)
                {
                    controller.wheelData[i].wheel.rollingResistance = defaultRollingResistance[i] + defaultRollingResistance[i] * wheelWear;
                }
            }

            //// -- ADVANCED MODE SLIP WEAR UPDATING --
            //float slip = 0f;
            //for (int i = 0; i < len; i++)
            //{
            //    slip += Mathf.Abs(controller.wheelData[i].wheel.wheelLocalVelocity.x);
            //}
            //slip /= controller.wheelData.Length;
            //float slipPercent = Mathf.Pow(Mathf.Max((slip / (maxSafeSpeed * 0.1f)), 0), 4);
            //if (speedPercent > 0)
            //{
            //    wheelWear += speedPercent * Time.fixedDeltaTime * 0.05f * HighLogic.CurrentGame.Parameters.CustomParams<KSPWheelWearSettings>().slipDamageMultiplier;//should give ~80 minutes at max speed before wear hits 1.0
            //    len = controller.wheelData.Length;
            //    for (int i = 0; i < len; i++)
            //    {
            //        controller.wheelData[i].wheel.rollingResistance = defaultRollingResistance[i] + defaultRollingResistance[i] * wheelWear;
            //    }
            //}

            // -- ADVANCED MODE SUSPENSION WEAR UPDATING --
            float loadpercent = Mathf.Pow(Mathf.Max((load / maxSafeLoad) - 0.9f, 0), 2);
            suspensionWear += loadpercent * Time.fixedDeltaTime * HighLogic.CurrentGame.Parameters.CustomParams<KSPWheelWearSettings>().stressDamageMultiplier;
            suspensionWear = Mathf.Clamp01(suspensionWear);
            controller.wheelRepairTimer = 1f - suspensionWear;
        }

        private void updateWheelMeshes(KSPWheelState wheelState)
        {
            if (wheelState == KSPWheelState.BROKEN)
            {
                if (bustedWheelMeshes != null)
                {
                    int len = bustedWheelMeshes.Length;
                    for (int i = 0; i < len; i++)
                    {
                        bustedWheelMeshes[i].gameObject.SetActive(true);
                    }
                    if (wheelMeshes != null)
                    {
                        len = wheelMeshes.Length;
                        for (int i = 0; i < len; i++)
                        {
                            wheelMeshes[i].gameObject.SetActive(false);
                        }
                    }
                }
                if (wheel != null)
                {
                    int len = controller.wheelData.Length;
                    for (int i = 0; i < len; i++)
                    {
                        controller.wheelData[i].wheel.angularVelocity = 0f;
                        controller.wheelData[i].wheel.motorTorque = 0f;
                        controller.wheelData[i].wheel.brakeTorque = 0f;
                    }
                }
            }
            else
            {
                if (bustedWheelMeshes != null)
                {
                    int len = bustedWheelMeshes.Length;
                    for (int i = 0; i < len; i++)
                    {
                        bustedWheelMeshes[i].gameObject.SetActive(false);
                    }
                }
                if (wheelMeshes != null)
                {
                    int len = wheelMeshes.Length;
                    for (int i = 0; i < len; i++)
                    {
                        wheelMeshes[i].gameObject.SetActive(true);
                    }
                }
            }
        }

        private void updateDisplayState()
        {
            KSPWheelState wheelState = controller.wheelState;
            KSPWheelWearType wearType = HighLogic.CurrentGame.Parameters.CustomParams<KSPWheelSettings>().wearType;
            Events[nameof(repairWheel)].guiActiveUnfocused = wheelState == KSPWheelState.BROKEN || wearType==KSPWheelWearType.ADVANCED;
            Fields[nameof(loadStress)].guiActive = wearType != KSPWheelWearType.NONE;
            Fields[nameof(stressTime)].guiActive = wearType != KSPWheelWearType.NONE;
            Fields[nameof(displayStatus)].guiActive = wearType != KSPWheelWearType.NONE;
            Fields[nameof(wheelWear)].guiActive = wearType == KSPWheelWearType.ADVANCED;
            Fields[nameof(motorWear)].guiActive = wearType == KSPWheelWearType.ADVANCED;
            Fields[nameof(suspensionWear)].guiActive = wearType == KSPWheelWearType.ADVANCED;
            Fields[nameof(maxSafeSpeed)].guiActive = Fields[nameof(maxSafeSpeed)].guiActiveEditor = wearType != KSPWheelWearType.NONE;
            Fields[nameof(maxSafeLoad)].guiActive = Fields[nameof(maxSafeLoad)].guiActiveEditor = wearType != KSPWheelWearType.NONE;
            switch (wheelState)
            {
                case KSPWheelState.RETRACTED:
                case KSPWheelState.RETRACTING:
                case KSPWheelState.DEPLOYED:
                case KSPWheelState.DEPLOYING:
                    displayStatus = LocalizationCache.str_WheelStatus_Operational; // "Operational"
                    break;
                case KSPWheelState.BROKEN:
                    displayStatus = LocalizationCache.str_WheelStatus_Broken; // "Broken"
                    break;
                default:
                    break;
            }
        }

    }
}
