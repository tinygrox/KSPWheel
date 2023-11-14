using System;

namespace KSPWheel
{
    public class KSPWheelSettings : GameParameters.CustomParameterNode
    {

        [GameParameters.CustomParameterUI("#KSPWheel_Setting_ManualGear", toolTip = "#KSPWheel_Setting_ManualGear_tooltip")] // Manual Gear Selection | If enabled motors will have multiple gear ratios available (configurable).
        public bool manualGearing = true;

        [GameParameters.CustomParameterUI("#KSPWheel_Setting_WheelDustEffects", toolTip = "#KSPWheel_Setting_WheelDustEffects_tooltip")] // Wheel Dust Effects | If enabled wheels will kick up dust when traversing terrain.
        public bool wheelDustEffects = true;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_WheelDustPower", minValue = 0, maxValue = 4, stepCount = 15, displayFormat = "F2", toolTip = "#KSPWheel_Setting_WheelDustPower_tooltip")] // Wheel Dust Power | Increases or decreases dust emission rate. 1=standard, 0=off
        public float wheelDustPower = 1f;

        [GameParameters.CustomParameterUI("#KSPWheel_Setting_WearType", toolTip = "#KSPWheel_Setting_WearType_tooltip")] // Wear and Damage | Wear and damage model.\nNone = No wheel wear or breakage.\nSimple = Stock equivalent, break on impact/over-stress.\nAdvanced = Time, speed, load, heat, and impact based wheel wear + breakage.
        public KSPWheelWearType wearType = KSPWheelWearType.SIMPLE;

        [GameParameters.CustomParameterUI("#KSPWheel_Setting_EnableDebugging", toolTip = "#KSPWheel_Setting_EnableDebugging_tooltip")] // Enable Debugging | If enabled debug tools will be available in the app-launcher bar..
        public bool debugMode = false;

        [GameParameters.CustomParameterUI("#KSPWheel_Setting_EnableFrictionControl", toolTip = "#KSPWheel_Setting_EnableFrictionControl_tooltip")] // Enable Friction Control | If enabled, per-part friction controls will be available.\n WARNING: Adjustments to friction are un-supported, and may cause instability.  You are on your own for support if you adjust these settings.
        public bool enableFrictionControl = false;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_GlobalFrictionMultiplier", minValue = 0, maxValue = 4, stepCount = 15, displayFormat = "F2", toolTip = "#KSPWheel_Setting_GlobalFrictionMultiplier_tooltip")] // Global Friction Multiplier | Increases or decreases friction for all wheels/legs. 0 = No friction, 1 = Standard, 2 = 2x standard, etc...\n WARNING: Adjustments to friction are un-supported, and may cause instability.  You are on your own for support if you adjust these settings.
        public float globalFrictionAdjustment = 1f;

        public override string Section { get { return "KSPWheel"; } }

        public override int SectionOrder { get { return 1; } }

        public override string Title { get { return LocalizationCache.str_Setting_BasicOptions; } } // "Basic Options"

        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }

        public override bool HasPresets { get { return false; } }

        public override string DisplaySection { get { return LocalizationCache.str_SettingSection; } } // "KSPWheel"

    }

    public class KSPWheelScaleSettings : GameParameters.CustomParameterNode
    {

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_PartMassScalePower", minValue = 1, maxValue = 4, stepCount = 11, displayFormat = "F2", toolTip = "#KSPWheel_Setting_PartMassScalePower_tooltip")] // Part Mass Scale Power | Sets the exponent to which part mass is scaled when scaling up or down
        public float partMassScalingPower = 3f;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_PartCostScalePower", minValue = 1, maxValue = 4, stepCount = 11, displayFormat = "F2", toolTip = "#KSPWheel_Setting_PartCostScalePower_tooltip")] // Part Cost Scale Power | Sets the exponent to which part cost is scaled when scaling up or down
        public float partCostScalingPower = 3f;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_WheelMassScalePower", minValue = 1, maxValue = 4, stepCount = 11, displayFormat = "F2", toolTip = "#KSPWheel_Setting_WheelMassScalePower_tooltip")] // Wheel Mass Scale Power | Sets the exponent to which wheel mass is scaled when scaling up or down
        public float wheelMassScalingPower = 3f;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_WheelMaxSpeedScalePower", minValue = 0, maxValue = 4, stepCount = 15, displayFormat = "F2", toolTip = "#KSPWheel_Setting_WheelMaxSpeedScalePower_tooltip")] // Wheel Max Speed Scale Power | Sets the exponent to which wheel max safe speed is scaled when scaling up or down
        public float wheelMaxSpeedScalingPower = 1f;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_WheelMaxLoadScalePower", minValue = 1, maxValue = 4, stepCount = 11, displayFormat = "F2", toolTip = "#KSPWheel_Setting_WheelMaxLoadScalePower_tooltip")] // Wheel Max Load Scale Power | Sets the exponent to which wheel min/max load are scaled when scaling up or down
        public float wheelMaxLoadScalingPower = 3f;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_WheelRollingResistanceScalePower", minValue = 1, maxValue = 4, stepCount = 11, displayFormat = "F2", toolTip = "#KSPWheel_Setting_WheelRollingResistanceScalePower_tooltip")] // Wheel Rolling Resistance Scale Power | Sets the exponent to which rolling resistance is scaled when scaling up or down
        public float rollingResistanceScalingPower = 1f;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_MotorTorqueScalePower", minValue = 1, maxValue = 4, stepCount = 11, displayFormat = "F2", toolTip = "#KSPWheel_Setting_MotorTorqueScalePower_tooltip")] // Motor Torque Scale Power | Sets the exponent to which motor torque is scaled when scaling up or down
        public float motorTorqueScalingPower = 3f;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_MotorPowerScalePower", minValue = 1, maxValue = 4, stepCount = 11, displayFormat = "F2", toolTip = "#KSPWheel_Setting_MotorPowerScalePower_tooltip")] // Motor Power Scale Power | Sets the exponent to which motor power draw is scaled when scaling up or down
        public float motorPowerScalingPower = 3f;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_MotorRPMScalePower", minValue = 0, maxValue = 4, stepCount = 15, displayFormat = "F2", toolTip = "#KSPWheel_Setting_MotorRPMScalePower_tooltip")] // Motor RPM Scale Power | Sets the exponent to which motor max rpm is scaled when scaling up or down
        public float motorMaxRPMScalingPower = 0f;

        public override string Section { get { return "KSPWheel"; } }

        public override int SectionOrder { get { return 3; } }

        public override string Title { get { return LocalizationCache.str_Setting_ScalingOptions; } } // "Scaling Options"

        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }

        public override bool HasPresets { get { return false; } }

        public override string DisplaySection { get { return LocalizationCache.str_SettingSection; } } // "KSPWheel"

    }

    public class KSPWheelWearSettings : GameParameters.CustomParameterNode
    {

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_WheelStressDamageRate", minValue = 0, maxValue = 4, stepCount = 40, displayFormat = "F2", toolTip = "#KSPWheel_Setting_WheelStressDamageRate_tooltip")] // Wheel Stress Damage Rate | Determines how quickly wheels break from being overloaded or absorbing impact forces.  Lower values result in increased load and impact stress tolerance, setting to zero disables stress based damage.
        public float stressDamageMultiplier = 1f;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_WheelSpeedDamageRate", minValue = 0, maxValue = 4, stepCount = 40, displayFormat = "F2", toolTip = "#KSPWheel_Setting_WheelSpeedDamageRate_tooltip")] // Wheel Speed Damage Rate | Determines how quickly wheels break from being driven past their maximum safe speed.  Lower values result in increased over-speed tolerance, setting to zero disables speed based damage.\nIn advanced wear mode this setting influences the overall rate of wheel wear accumulation that is contributed to speed.
        public float speedDamageMultiplier = 1f;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_WheelSlipDamageRate", minValue = 0, maxValue = 4, stepCount = 40, displayFormat = "F2", toolTip = "#KSPWheel_Setting_WheelSlipDamageRate_tooltip")] // Wheel Slip Damage Rate | ADVANCED WEAR MODE ONLY\nDetermines how quickly wheels accumulate wear from wheel slip.  Lower values result in increased slip tolerance, setting to zero disables slip based damage.
        public float slipDamageMultiplier = 1f;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_MotorUseWearRate", minValue = 0, maxValue = 4, stepCount = 40, displayFormat = "F2", toolTip = "#KSPWheel_Setting_MotorUseWearRate_tooltip")] // Motor Use Wear Rate | ADVANCED WEAR MODE ONLY\nDetermines how quickly motors accumulate wear from standard use.  Lower values result in increased motor lifespan, setting to zero disables use based damage.
        public float motorDamageMultiplier = 1f;

        [GameParameters.CustomFloatParameterUI("#KSPWheel_Setting_MotorHeatOutputRate", minValue = 0, maxValue = 4, stepCount = 40, displayFormat = "F2", toolTip = "#KSPWheel_Setting_MotorHeatOutputRate_tooltip")] // Motor Heat Output Rate | ADVANCED WEAR MODE ONLY\nDetermines how much heat motors output wear from being used.  Lower values result in decreased motor heat output, setting to zero disables motor heat output.
        public float motorHeatMultiplier = 1f;

        public override string Section { get { return "KSPWheel"; } }

        public override int SectionOrder { get { return 2; } }

        public override string Title { get { return LocalizationCache.str_Setting_DamageOptions; } } // "Damage Options"

        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }

        public override bool HasPresets { get { return false; } }

        public override string DisplaySection { get { return LocalizationCache.str_SettingSection; } } // "KSPWheel"

    }

    public enum KSPWheelWearType
    {
        NONE,
        SIMPLE,
        ADVANCED
    }
}
