using System;
using HarmonyLib;
using UnityEngine;

namespace WoodlandPassage;

[HarmonyPatch]
internal static class TreeFooting
{
    private static TurfCollider playerLeavingTree;

    internal static void OnSettingChanged(object sender, EventArgs args)
    {
        playerLeavingTree = null;
        var player = Player.Instance;
        if (Plugin.SmallTrees.Value || !player || !player.entityMover || player.entityMover.Flying)
            return;

        // Turning passage off while standing in a tree must leave time to walk out.
        foreach (var collider in Turfs.GetTurfComponents<TurfCollider>(player.transform.GetVector2IntPosition()))
        {
            if (Movement.IsSmallTree(SerializationManager.GetPrefabName(collider.gameObject)))
            {
                playerLeavingTree = player.turfCollider;
                break;
            }
        }
    }

    [HarmonyPostfix, HarmonyPatch(typeof(TurfCollider), nameof(TurfCollider.OnTurfUnregistration))]
    private static void OnLeavingTile(TurfCollider __instance)
    {
        if (ReferenceEquals(__instance, playerLeavingTree))
            playerLeavingTree = null;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(TurfCollider), nameof(TurfCollider.OnStayedOn))]
    private static bool CheckFooting(TurfCollider __instance, GameObject caller)
    {
        // Silverpine uses the abyss flag for both flying over trees and falling when stationary.
        if (!__instance.abyss || (!Plugin.SmallTrees.Value && !playerLeavingTree))
            return true;
        var player = Player.Instance;
        return !player || caller != player.gameObject ||
               (!Plugin.SmallTrees.Value && player.turfCollider != playerLeavingTree) ||
               !Movement.IsSmallTree(SerializationManager.GetPrefabName(__instance.gameObject));
    }
}
