using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace KSPWheel
{
    public class KSPWheelVesselControl : VesselModule
    {

        private int id = 0;
        private Rect windowRect = new Rect(100, 100, 1024, 768);
        private Vector2 scrollPos;
        private bool guiInitialized = false;
        private List<KSPWheelBase> baseModules = new List<KSPWheelBase>();

        private static float w1 = 30;
        private static float w2 = 50;
        private static float w3 = 100;
        private static float w4 = 250;

        public void drawGUI()
        {
            if (!guiInitialized)
            {
                guiInitialized = true;
                baseModules.Clear();
                foreach (Part p in vessel.Parts)
                {
                    baseModules.AddUniqueRange(p.GetComponentsInChildren<KSPWheelBase>());
                }
            }
            if (vessel.isActiveVessel)
            {
                drawControlGUI();
            }
        }

        private void drawControlGUI()
        {
            windowRect = GUI.Window(id, windowRect, updateWindow, LocalizationCache.str_GUItitle); // "Wheel Controls"
        }

        private void updateWindow(int id)
        {
            GUILayout.BeginVertical();

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUILayout.BeginVertical();
            int len = baseModules.Count;
            int len2;
            KSPWheelSubmodule sub;
            Type type;
            float val;
            for (int i = 0; i < len; i++)
            {
                len2 = baseModules[i].subModules.Count;

                //per-base-module controls
                //wheel name, spring, damper, friction adjustment
                GUILayout.BeginHorizontal();
                baseModules[i].label = GUILayout.TextField(baseModules[i].label, GUILayout.Width(w4));//user-definable per-base-module label; merely to tell the parts/base-modules apart...
                if (GUILayout.Button(LocalizationCache.str_GUIHighlight, GUILayout.Width(w3))) // "Highlight"
                {
                    //TODO toggle highlighting of part
                }
                //TODO add group adjustment buttons
                GUILayout.Label(Localizer.Format("#KSPWheel_GUI_Group", baseModules[i].wheelGroup), GUILayout.Width(w3)); // "Group: " + baseModules[i].wheelGroup
                GUILayout.Label(LocalizationCache.str_GUISpring, GUILayout.Width(w3)); // "Spring"
                val = GUILayout.HorizontalSlider(baseModules[i].springRating, 0, 1, GUILayout.Width(w2));
                if (val != baseModules[i].springRating)
                {
                    baseModules[i].springRating = val;
                    baseModules[i].onLoadUpdated(null, null);
                }
                GUILayout.Label(LocalizationCache.str_GUI_DampRatio, GUILayout.Width(w3)); // "Damp Ratio"
                val = GUILayout.HorizontalSlider(baseModules[i].dampRatio, 0.35f, 1.0f, GUILayout.Width(w2));
                if (val != baseModules[i].dampRatio)
                {
                    baseModules[i].dampRatio = val;
                    baseModules[i].onLoadUpdated(null, null);
                }
                GUILayout.EndHorizontal();

                //for each wheel control module
                //check module type, call draw routine for that module
                for (int k = 0; k < len2; k++)
                {
                    GUILayout.BeginHorizontal();
                    sub = baseModules[i].subModules[k];
                    type = sub.GetType();
                    GUILayout.Label("", GUILayout.Width(w1));
                    GUILayout.Label(type.ToString(), GUILayout.Width(w3));
                    if (type == typeof(KSPWheelSteering))
                    {
                        drawSteeringControls((KSPWheelSteering)sub);
                    }
                    else if (type == typeof(KSPWheelTracks))
                    {
                        drawTrackControls((KSPWheelTracks)sub);
                    }
                    else if (type == typeof(KSPWheelMotor))
                    {
                        drawMotorControls((KSPWheelMotor)sub);
                    }
                    else if (type == typeof(KSPWheelBrakes))
                    {
                        drawBrakeControls((KSPWheelBrakes)sub);
                    }
                    else if (type == typeof(KSPWheelRepulsor))
                    {
                        drawRepulsorControls((KSPWheelRepulsor)sub);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
            
            GUILayout.EndScrollView();

            //close button at the bottom of the window, below the scroll bar
            if (GUILayout.Button(LocalizationCache.str_GUI_Close)) // "Close"
            {
                KSPWheelLauncher.instance.controlGuiDisable();
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void drawSteeringControls(KSPWheelSteering steering)
        {
            if (GUILayout.Button(Localizer.Format("#KSPWheel_GUI_Invert", steering.invertSteering))) // "Invert: " + steering.invertSteering
            {
                steering.invertSteering = !steering.invertSteering;
                steering.onSteeringInverted(null, null);
            }
            if (GUILayout.Button(Localizer.Format("#KSPWheel_GUI_Lock", steering.steeringLocked))) // "Lock: " + steering.steeringLocked
            {
                steering.steeringLocked = !steering.steeringLocked;
                steering.onSteeringLocked(null, null);
            }
            float val = 0f;
            GUILayout.Label(LocalizationCache.str_GUI_LowSpeedLimit, GUILayout.Width(w3)); // "Low Speed Limit"
            val = GUILayout.HorizontalSlider(steering.steeringLimit, 0, 1, GUILayout.Width(w2));
            if (val != steering.steeringLimit)
            {
                steering.steeringLimit = val;
                steering.onSteeringLimitUpdated(null, null);
            }
            GUILayout.Label(LocalizationCache.str_GUI_HighSpeedLimit, GUILayout.Width(w3)); // "High Speed Limit"
            val = GUILayout.HorizontalSlider(steering.steeringLimitHigh, 0, 1, GUILayout.Width(w2));
            if (val != steering.steeringLimitHigh)
            {
                steering.steeringLimitHigh = val;
                steering.onSteeringLimitUpdated(null, null);
            }
            GUILayout.Label(LocalizationCache.str_GUI_ResponseSpeed, GUILayout.Width(w3)); // "Response Speed"
            val = GUILayout.HorizontalSlider(steering.steeringResponse, 0, 1, GUILayout.Width(w2));
            if (val != steering.steeringResponse)
            {
                steering.steeringResponse = val;
                steering.onSteeringLimitUpdated(null, null);
            }
            GUILayout.Label(LocalizationCache.str_GUI_Bias, GUILayout.Width(w3)); // "Bias"
            val = GUILayout.HorizontalSlider(steering.steeringBias, 0, 1, GUILayout.Width(w2));
            if (val != steering.steeringBias)
            {
                steering.steeringBias = val;
                steering.onSteeringBiasUpdated(null, null);
            }
        }

        private void drawTrackControls(KSPWheelTracks tracks)
        {
            drawMotorControls(tracks);
        }

        private void drawMotorControls(KSPWheelMotor motor)
        {
            float val = 0f;
            if (GUILayout.Button(Localizer.Format("#KSPWheel_GUI_InvertMotor", motor.invertMotor), GUILayout.Width(w3))) // "Invert Motor: " + motor.invertMotor
            {
                motor.invertMotor = !motor.invertMotor;
                motor.onMotorInvert(null, null);
            }
            if (GUILayout.Button(Localizer.Format("#KSPWheel_GUI_LockMotor", motor.motorLocked), GUILayout.Width(w3))) // "Lock Motor: " + motor.motorLocked
            {
                motor.motorLocked = !motor.motorLocked;
                motor.onMotorLock(null, null);
            }
            GUILayout.Label(LocalizationCache.str_GUI_MotorLimit, GUILayout.Width(w3)); // "Motor Limit"
            val = GUILayout.HorizontalSlider(motor.motorOutput, 0, 100, GUILayout.Width(w2));
            if (val != motor.motorOutput)
            {
                motor.motorOutput = val;
                motor.onMotorLimitUpdated(null, null);
            }
            if (GUILayout.Button("<", GUILayout.Width(w1)))
            {
                motor.gearRatio = Mathf.Clamp(motor.gearRatio - 1f, 1f, 20f);
                motor.onGearUpdated(null, null);
            }
            GUILayout.Label(Localizer.Format("KSPWheel_GUI_GearRatio", motor.gearRatio)); // "Gear: " + motor.gearRatio
            if (GUILayout.Button(">", GUILayout.Width(w1)))
            {
                motor.gearRatio = Mathf.Clamp(motor.gearRatio + 1f, 1f, 20f);
                motor.onGearUpdated(null, null);
            }
            if (motor.tankSteering)
            {
                if (GUILayout.Button(Localizer.Format("#KSPWheel_GUI_LockSteering", motor.steeringLocked), GUILayout.Width(w3))) // "Lock Steering: " + motor.steeringLocked
                {
                    motor.steeringLocked = !motor.steeringLocked;
                    motor.onSteeringLock(null, null);
                }
                if (GUILayout.Button(Localizer.Format("#KSPWheel_GUI_InvertSteering", motor.invertSteering), GUILayout.Width(w3))) // "Invert Steering: " + motor.invertSteering
                {
                    motor.invertSteering = !motor.invertSteering;
                    motor.onSteeringInvert(null, null);
                }
                if (GUILayout.Button(Localizer.Format("#KSPWheel_GUI_HalfTrackMode", motor.halfTrackSteering), GUILayout.Width(w3))) // "Half Track Mode: " + motor.halfTrackSteering
                {
                    motor.halfTrackSteering = !motor.halfTrackSteering;
                    motor.onHalftrackToggle(null, null);
                }
            }
        }

        private void drawBrakeControls(KSPWheelBrakes brakes)
        {
            //TODO
        }

        private void drawRepulsorControls(KSPWheelRepulsor repulsor)
        {
            //TODO
        }
    }
}
