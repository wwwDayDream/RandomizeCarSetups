using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace RandomizeCarSetups.Patches;

[HarmonyPatch(typeof(StationController), nameof(StationController.Update))]
[UsedImplicitly]
public static class StationControllerUpdate
{
    // In order to appear before PersistentJobs we inject our call after the method that PJ would hook TryToGenerateJobs()
    // Then we act on the jobs within the station as a whole instead of previously acting on each chain individually.
    [UsedImplicitly]
    public static IEnumerable<CodeInstruction> Transpiler(MethodBase original,
        IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        foreach (var codeInstruction in instructions)

        {
            if (codeInstruction.Calls(
                    typeof(StationProceduralJobsController).GetMethod(nameof(StationProceduralJobsController
                        .TryToGenerateJobs))))
            {
                // Call our pre-TryToGen
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call,
                    typeof(Randomizer).GetMethod(nameof(Randomizer.PreTryToGenerateJobs)));
                // Call TryToGen
                yield return codeInstruction;
                // Call post TryToGen (After persistent jobs and/or vanilla spawning completes)
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call,
                    typeof(Randomizer).GetMethod(nameof(Randomizer.RandomizeStationJobs)));
                yield return new CodeInstruction(OpCodes.Call,
                    typeof(MonoBehaviour).GetMethod(nameof(MonoBehaviour.StartCoroutine), BindingFlags.Instance | BindingFlags.Public, null, default
                        , [typeof(IEnumerator)], null));
                yield return new CodeInstruction(OpCodes.Pop);

            } else 
                yield return codeInstruction;

        }
    }

}