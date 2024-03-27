using HarmonyLib;
using JetBrains.Annotations;

namespace RandomizeCarSetups.Patches;

[HarmonyPatch(typeof(SaveGameManager), nameof(SaveGameManager.Start))]
[UsedImplicitly]
public static class SaveGameManagerStart
{
    [UsedImplicitly]
    public static void Postfix()
    {
        SaveDataManager.Init();
    }
}