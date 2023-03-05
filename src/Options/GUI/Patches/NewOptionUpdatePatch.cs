using HarmonyLib;

namespace VentLib.Options.GUI.Patches;

[HarmonyPriority(Priority.Low)]
[HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
internal static class NewOptionUpdatePatch
{
    internal static void Postfix(GameOptionsMenu __instance)
    {
        GameOptionController.DoRender(__instance);
    }
}