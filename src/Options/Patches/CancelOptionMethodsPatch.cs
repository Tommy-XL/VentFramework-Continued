using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using AmongUs.GameOptions;
using VentLib.Options.Extensions;
using LibCpp2IL;
using VentLib.Utilities.Extensions;
using VentLib.Options.UI.Controllers;

namespace VentLib.Options.Patches;

// this code is ugly but it works

//listen for categories so we can add them to the list to disable them
[HarmonyPatch(typeof(CategoryHeaderMasked), nameof(CategoryHeaderMasked.SetHeader))]
public static class CheckForSetHeaderPatch
{
    public static void Prefix(CategoryHeaderMasked __instance, StringNames name, int maskLayer)
    {
        // Role Headers are a different class so this should filter them out
        if (__instance.GetType() == typeof(CategoryHeaderMasked)) OptionExtensions.categoryHeaders.Add(__instance);
    }
}

// Initialize code here.
[HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.Initialize))]
public static class CancelToggleOptionInitializePatch
{
    public static bool Prefix(ToggleOption __instance) => __instance.boolOptionName != BoolOptionNames.Invalid;
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.Initialize))]
public static class CancelStringOptionInitializePatch
{
    public static bool Prefix(StringOption __instance) => __instance.stringOptionName != Int32OptionNames.Invalid;
}

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.Initialize))]
public static class CancelNumberOptionInitializePatch
{
    public static bool Prefix(NumberOption __instance) => __instance.floatOptionName != FloatOptionNames.Invalid || __instance.intOptionName != Int32OptionNames.Invalid;
}

// PlayerOption does not have an Initialize so uhh...
[HarmonyPatch(typeof(PlayerOption), nameof(PlayerOption.OnEnable))]
public static class CancelPlayerEnablePatch
{
    public static string? savedTextName;
    public static void Prefix(PlayerOption __instance)
    {
        if (__instance.optionName == Int32OptionNames.Invalid) savedTextName = __instance.TitleText.text;
    }
    public static void Postfix(PlayerOption __instance)
    {
        if (__instance.optionName == Int32OptionNames.Invalid) __instance.TitleText.text = savedTextName;
    }
}

// Update Value Code here
[HarmonyPatch(typeof(ToggleOption), nameof(ToggleOption.UpdateValue))]
public static class CancelToggleOptionUpdateePatch
{
    public static bool Prefix(ToggleOption __instance) => __instance.boolOptionName != BoolOptionNames.Invalid;
}

[HarmonyPatch(typeof(StringOption), nameof(StringOption.FixedUpdate))]
public static class CancelStringOptionUpdatePatch
{
    public static bool Prefix(StringOption __instance) => __instance.stringOptionName != Int32OptionNames.Invalid;
}

[HarmonyPatch(typeof(NumberOption), nameof(NumberOption.UpdateValue))]
public static class CancelNumberOptionUpdatePatch
{
    public static bool Prefix(NumberOption __instance) => __instance.floatOptionName != FloatOptionNames.Invalid || __instance.intOptionName != Int32OptionNames.Invalid;
}

[HarmonyPatch(typeof(PlayerOption), nameof(PlayerOption.UpdateValue))]
public static class CancelPlayerOptionUpdatePatch
{
    public static bool Prefix(PlayerOption __instance) => __instance.optionName != Int32OptionNames.Invalid;
}

[HarmonyPatch(typeof(RoleOptionSetting), nameof(RoleOptionSetting.UpdateValuesAndText))]
public static class CancelRoleOptionSettingUpdateValuesAndText
{
    public static bool Prefix(RoleOptionSetting __instance) => !RoleOptionController.Enabled;
}