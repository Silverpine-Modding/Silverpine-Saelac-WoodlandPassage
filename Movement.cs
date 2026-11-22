using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace WoodlandPassage;

[HarmonyPatch]
internal static class Movement
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(EntityMover), nameof(EntityMover.TryMove));
        yield return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(EntityMover), "DoubleCheckAndAnimate"));
    }

    // Both the initial step and its queued animation check the destination. Pass the
    // mover into those two queries so other uses of the same turf colliders stay native.
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var nativeQuery = AccessTools.Method(typeof(TurfCollider), nameof(TurfCollider.TryGetImpassableColliders));
        var movementQuery = AccessTools.Method(typeof(Movement), nameof(TryGetMovementBlockers));
        var iteratorOwner = original.DeclaringType == typeof(EntityMover) ? null :
            AccessTools.GetDeclaredFields(original.DeclaringType).Single(field => field.FieldType == typeof(EntityMover));

        int replaced = 0;
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(nativeQuery))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(instruction);
                if (iteratorOwner != null)
                    yield return new CodeInstruction(OpCodes.Ldfld, iteratorOwner);
                instruction.operand = movementQuery;
                replaced++;
            }
            yield return instruction;
        }
        if (replaced != 1)
            throw new InvalidOperationException($"Expected one movement collision query in {original}, found {replaced}.");
    }

    private static bool TryGetMovementBlockers(Vector2Int position, out List<TurfCollider> blockers, EntityMover mover)
    {
        bool blocked = TurfCollider.TryGetImpassableColliders(position, out blockers);
        if (!blocked || Player.Instance == null || mover != Player.Instance.entityMover)
            return blocked;

        // The native query returns a fresh list. Preserve every other blocker on a shared tile.
        bool flying = mover.Flying;
        for (int index = blockers.Count - 1; index >= 0; index--)
        {
            string prefab = SerializationManager.GetPrefabName(blockers[index].gameObject);
            if (AllowsPassage(prefab, flying))
                blockers.RemoveAt(index);
        }
        return blockers.Count > 0;
    }

    private static bool AllowsPassage(string prefab, bool flying)
    {
        switch (prefab)
        {
            case "prefab_nature_wood":
            case "prefab_nature_felledtree":
                return Plugin.GroundWood.Value;
            case "prefab_boulder_small":
                return Plugin.SmallRocks.Value;
            case "prefab_boulder_medium":
                return Plugin.MediumRocks.Value;
            default:
                return !flying && Plugin.SmallTrees.Value && IsSmallTree(prefab);
        }
    }

    internal static bool IsSmallTree(string prefab) => prefab is
        "prefab_tree_oak" or "prefab_tree_pine" or "prefab_tree_swamp" or
        "prefab_tree_willow" or "prefab_tree_mushroom_glowing_blue";
}
