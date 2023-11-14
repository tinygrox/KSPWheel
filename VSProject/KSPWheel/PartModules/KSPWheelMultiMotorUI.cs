﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KSPWheel
{
    /// <summary>
    /// Module that manages the UI interaction for multiple KSPWheelMotor modules.
    /// Maintains their relative invert settings, so that some may still be inverted relative to the rest.
    /// </summary>
    public class KSPWheelMultiMotorUI : KSPWheelSubmodule
    {

        #region Config File Fields
        [KSPField]
        public string wheelIndices = "0";

        /// <summary>
        /// User-selectable motor output limiter
        /// </summary>
        [KSPField(guiName = "#KSPWheel_MotorLimit", guiActive = true, guiActiveEditor = true, isPersistant = true, guiUnits = "%"), // Motor Limit
         UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 0.5f)]
        public float motorOutput = 100f;

        /// <summary>
        /// If true, motor response will be inverted for this wheel.  Toggleable in editor and flight.  Persistent.
        /// </summary>
        [KSPField(guiName = "#KSPWheel_InvertMotor", guiActive = true, guiActiveEditor = true, isPersistant = true), // Invert Motor
         UI_Toggle(enabledText = "#autoLOC_6001077", disabledText = "#autoLOC_6001075", suppressEditorShipModified = true, affectSymCounterparts = UI_Scene.None)] // Inverted | Normal
        public bool invertMotor;

        /// <summary>
        /// If true, motor response will be inverted for this wheel.  Toggleable in editor and flight.  Persistent.
        /// </summary>
        [KSPField(guiName = "#KSPWheel_MotorLock", guiActive = true, guiActiveEditor = true, isPersistant = true), // Motor Lock
         UI_Toggle(enabledText = "#KSPWheel_MotorLock_Locked", disabledText = "#KSPWheel_MotorLock_Locked", suppressEditorShipModified = true, affectSymCounterparts = UI_Scene.Editor)] // Locked | Free
        public bool motorLocked;

        [KSPField(guiName = " #KSPWheel_TankSteerInvert", guiActive = false, guiActiveEditor = false, isPersistant = true), // Tank Steer Invert
         UI_Toggle(enabledText = "#autoLOC_6001077", disabledText = "#autoLOC_6001075", suppressEditorShipModified = true, affectSymCounterparts = UI_Scene.None)] // Inverted | Normal
        public bool invertSteering = false;

        [KSPField(guiName = "#KSPWheel_TankSteerLock", guiActive = false, guiActiveEditor = false, isPersistant = true), // Tank Steer Lock
         UI_Toggle(enabledText = "#KSPWheel_TankSteerLock_Locked", disabledText = "#KSPWheel_TankSteerLock_Free", suppressEditorShipModified = true, affectSymCounterparts = UI_Scene.None)] // Locked | Free
        public bool steeringLocked = false;

        [KSPField(guiName = "#KSPWheel_HalfTrackSteering", guiActive = false, guiActiveEditor = false, isPersistant = true), // Half-Track
         UI_Toggle(enabledText = "#KSPWheel_HalfTrackSteering_Enabled", disabledText = "#KSPWheel_HalfTrackSteering_Disabled", suppressEditorShipModified = true, affectSymCounterparts = UI_Scene.None)] // Enabled | Disabled
        public bool halfTrackSteering = false;

        [KSPField(guiName = "#KSPWheel_GearRatio", guiActive = true, guiActiveEditor = true, isPersistant = true), // Gear Ratio (x:1)
         UI_FloatEdit(suppressEditorShipModified = true, minValue = 0.25f, maxValue = 20f, incrementSlide = 0.05f, incrementLarge = 1f, incrementSmall = 0.25f, sigFigs = 2)]
        public float gearRatio = 4f;

        [KSPField]
        public float minGearRatio = 0.25f;

        [KSPField]
        public float maxGearRatio = 20f;
        
        /// <summary>
        /// GUI display values below here -- tallied/averaged across all controlled wheels
        /// </summary>
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "#KSPWheel_MaxDriveSpeed", guiUnits = "m/s")] // Max Drive Speed
        public float maxDrivenSpeed = 0f;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "#KSPWheel_MotorRPM")] // Motor RPM
        public float motorCurRPM = 0f;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "#KSPWheel_TorqueToWheel", guiUnits = "kN/M")] // Torque To Wheel
        public float torqueOut = 0f;

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "#KSPWheel_PowerOutKW", guiUnits = "kW")] // Mech. Output
        public float powerOutKW = 0f;

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "#KSPWheel_powerInKW", guiUnits = "kW")] // Elec. Input
        public float powerInKW = 0f;

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "#KSPWheel_powerEff", guiUnits = "%")] // Efficiency
        public float powerEff = 0f;

        [KSPField(guiActive = true, guiName = "#KSPWheel_guiResourceUse", guiUnits = "ec/s")] // Motor EC Use
        public float guiResourceUse = 0f;

        [KSPField]
        public bool showGUIMotorLimit = true;
        [KSPField]
        public bool showGUIMotorInvert = true;
        [KSPField]
        public bool showGUIMotorLock = true;
        [KSPField]
        public bool showGUISteerLock = true;
        [KSPField]
        public bool showGUISteerInvert = true;
        [KSPField]
        public bool showGUIGearRatio = true;
        [KSPField]
        public bool showGUIHalfTrack = true;
        [KSPField]
        public bool showGUIStats = true;

        #endregion

        private KSPWheelMotor[] motorModules;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            UI_FloatRange motorLimitField = (UI_FloatRange)(HighLogic.LoadedSceneIsEditor ? Fields[nameof(motorOutput)].uiControlEditor : Fields[nameof(motorOutput)].uiControlFlight);
            motorLimitField.onFieldChanged = (a , b) =>
            {
                this.symmetryUpdate(m =>
                {
                    m.motorOutput = motorOutput;
                    int len = m.motorModules.Length;
                    for (int i = 0; i < len; i++)
                    {
                        m.motorModules[i].motorOutput = motorOutput;
                    }
                });
            };

            UI_Toggle invertMotorField = (UI_Toggle)(HighLogic.LoadedSceneIsEditor ? Fields[nameof(invertMotor)].uiControlEditor : Fields[nameof(invertMotor)].uiControlFlight);
            invertMotorField.onFieldChanged = (a, b) =>
            {
                this.symmetryUpdate(m =>
                {
                    m.invertMotor = invertMotor;
                    int len = m.motorModules.Length;
                    for (int i = 0; i < len; i++)
                    {
                        m.motorModules[i].invertMotor = !m.motorModules[i].invertMotor;
                    }
                });
            };

            UI_Toggle motorLockedField = (UI_Toggle)(HighLogic.LoadedSceneIsEditor ? Fields[nameof(motorLocked)].uiControlEditor : Fields[nameof(motorLocked)].uiControlFlight);
            motorLockedField.onFieldChanged = (a, b) =>
            {
                this.symmetryUpdate(m =>
                {
                    m.motorLocked = motorLocked;
                    int len = m.motorModules.Length;
                    for (int i = 0; i < len; i++)
                    {
                        m.motorModules[i].motorLocked = !m.motorModules[i].motorLocked;
                    }
                    m.updatePowerStats();
                });
            };

            UI_Toggle invertSteeringField = (UI_Toggle)(HighLogic.LoadedSceneIsEditor ? Fields[nameof(invertSteering)].uiControlEditor : Fields[nameof(invertSteering)].uiControlFlight);
            invertSteeringField.onFieldChanged = (a, b) =>
            {
                this.symmetryUpdate(m =>
                {
                    m.invertSteering = invertSteering;
                    int len = m.motorModules.Length;
                    for (int i = 0; i < len; i++)
                    {
                        m.motorModules[i].invertSteering = !m.motorModules[i].invertSteering;
                    }
                });
            };

            UI_Toggle steeringLockedField = (UI_Toggle)(HighLogic.LoadedSceneIsEditor ? Fields[nameof(steeringLocked)].uiControlEditor : Fields[nameof(steeringLocked)].uiControlFlight);
            steeringLockedField.onFieldChanged = (a, b) =>
            {
                this.symmetryUpdate(m =>
                {
                    m.steeringLocked = steeringLocked;
                    int len = m.motorModules.Length;
                    for (int i = 0; i < len; i++)
                    {
                        m.motorModules[i].steeringLocked = !m.motorModules[i].steeringLocked;
                    }
                });
            };

            UI_Toggle halfTrackSteeringField = (UI_Toggle)(HighLogic.LoadedSceneIsEditor ? Fields[nameof(halfTrackSteering)].uiControlEditor : Fields[nameof(halfTrackSteering)].uiControlFlight);
            halfTrackSteeringField.onFieldChanged = (a, b) =>
            {
                this.symmetryUpdate(m =>
                {
                    m.halfTrackSteering = halfTrackSteering;
                    int len = m.motorModules.Length;
                    for (int i = 0; i < len; i++)
                    {
                        m.motorModules[i].halfTrackSteering = !m.motorModules[i].halfTrackSteering;
                    }
                });
            };

            UI_FloatEdit gearRatioField = (UI_FloatEdit)(HighLogic.LoadedSceneIsEditor ? Fields[nameof(gearRatio)].uiControlEditor : Fields[nameof(gearRatio)].uiControlFlight);
            gearRatioField.minValue = minGearRatio;
            gearRatioField.maxValue = maxGearRatio;
            gearRatioField.onFieldChanged = (a, b) =>
            {
                this.symmetryUpdate(m =>
                {
                    m.gearRatio = gearRatio;
                    int len = m.motorModules.Length;
                    for (int i = 0; i < len; i++)
                    {
                        m.motorModules[i].gearRatio = m.gearRatio;
                        m.motorModules[i].calcPowerStats();
                    }
                    m.updatePowerStats();
                });
            };
        }

        internal override void onUIControlsUpdated(bool show)
        {
            base.onUIControlsUpdated(show);

            Fields[nameof(motorOutput)].guiActive = Fields[nameof(motorOutput)].guiActiveEditor = show && showGUIMotorLimit;
            Fields[nameof(invertMotor)].guiActive = Fields[nameof(invertMotor)].guiActiveEditor = show && showGUIMotorInvert;
            Fields[nameof(motorLocked)].guiActive = Fields[nameof(motorLocked)].guiActiveEditor = show && showGUIMotorLock;

            Fields[nameof(invertSteering)].guiActive = Fields[nameof(invertSteering)].guiActiveEditor = show && showGUISteerInvert;
            Fields[nameof(steeringLocked)].guiActive = Fields[nameof(steeringLocked)].guiActiveEditor = show && showGUISteerLock;
            Fields[nameof(halfTrackSteering)].guiActive = Fields[nameof(halfTrackSteering)].guiActiveEditor = show && showGUIHalfTrack;

            Fields[nameof(gearRatio)].guiActive = Fields[nameof(gearRatio)].guiActiveEditor = show && HighLogic.CurrentGame.Parameters.CustomParams<KSPWheelSettings>().manualGearing && showGUIGearRatio;

            Fields[nameof(maxDrivenSpeed)].guiActive = Fields[nameof(maxDrivenSpeed)].guiActiveEditor = show && showGUIStats;
            Fields[nameof(motorCurRPM)].guiActive = Fields[nameof(motorCurRPM)].guiActiveEditor = show && showGUIStats;
            Fields[nameof(torqueOut)].guiActive = Fields[nameof(torqueOut)].guiActiveEditor = show && showGUIStats;
            Fields[nameof(powerOutKW)].guiActive = Fields[nameof(powerOutKW)].guiActiveEditor = show && showGUIStats;
            Fields[nameof(powerInKW)].guiActive = Fields[nameof(powerInKW)].guiActiveEditor = show && showGUIStats;
            Fields[nameof(powerEff)].guiActive = Fields[nameof(powerEff)].guiActiveEditor = show && showGUIStats;
            Fields[nameof(guiResourceUse)].guiActive = Fields[nameof(guiResourceUse)].guiActiveEditor = show && showGUIStats;
        }

        internal override void postWheelCreated()
        {
            base.postWheelCreated();
            int[] indices = Utils.parseIntCSV(wheelIndices);
            List<KSPWheelMotor> motors = new List<KSPWheelMotor>();
            KSPWheelMotor[] rawMotors = part.GetComponents<KSPWheelMotor>();
            int len = rawMotors.Length;
            int len2 = indices.Length;
            for (int i = 0; i < len; i++)
            {
                for (int k = 0; k < len2; k++)
                {
                    if (rawMotors[i].wheelIndex == indices[k]) { motors.Add(rawMotors[i]); }
                }
            }
            motorModules = motors.ToArray();
            updatePowerStats();
        }

        internal override void postWheelPhysicsUpdate()
        {
            base.postWheelPhysicsUpdate();
            //only update UI fields if window is open
            if (!UIPartActionController.Instance.ItemListContains(part, false)) { return; }
            int len = motorModules.Length;
            guiResourceUse = 0f;
            torqueOut = 0f;
            motorCurRPM = 0f;
            powerEff = 0f;
            powerOutKW = 0f;
            powerInKW = 0f;
            for (int i = 0; i < len; i++)
            {
                guiResourceUse += motorModules[i].guiResourceUse;
                torqueOut += motorModules[i].torqueOut;
                motorCurRPM += motorModules[i].motorCurRPM;
                powerEff += motorModules[i].powerEff;
                powerOutKW += motorModules[i].powerOutKW;
                powerInKW += motorModules[i].powerInKW;
            }
            powerEff /= len;
            motorCurRPM /= len;
        }

        private void updatePowerStats()
        {
            maxDrivenSpeed = 0f;
            int len = motorModules.Length;
            for (int i = 0; i < len; i++)
            {
                maxDrivenSpeed += motorModules[i].maxDrivenSpeed;
            }
            maxDrivenSpeed /= len;
        }

    }
}
