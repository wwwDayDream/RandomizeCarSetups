using System.Collections.Generic;
using System.Linq;
using DV.Common;
using HarmonyLib;
using JetBrains.Annotations;

namespace RandomizeCarSetups.Patches;

[HarmonyPatch(typeof(SaveGameManager), nameof(SaveGameManager.FindStartGameData))]
[UsedImplicitly]
public static class SaveGameManagerFindStartGameData
{
    [UsedImplicitly]
    public static void Postfix(SaveGameManager __instance)
    {
        SaveDataManager.LoadFromSave(__instance);
    }
}