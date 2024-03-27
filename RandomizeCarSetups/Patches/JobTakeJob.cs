using System.Linq;
using DV.Logic.Job;
using HarmonyLib;
using JetBrains.Annotations;

namespace RandomizeCarSetups.Patches;

[HarmonyPatch(typeof(Job), nameof(Job.TakeJob))]
[UsedImplicitly]
public static class JobTakeJob
{
    [UsedImplicitly]
    public static void Postfix(Job __instance)
    {
        SaveDataManager.UnTrackJobId(__instance.ID);
    }
}