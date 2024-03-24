using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityModManagerNet;

namespace RandomizeCarSetups;

[UsedImplicitly]
[EnableReloading]
internal static class Plugin
{
    internal static UnityModManager.ModEntry.ModLogger Logger { get; private set; }
    private static Harmony? Patcher { get; set; }
    
    [UsedImplicitly]
    internal static bool Load(UnityModManager.ModEntry modEntry)
    {
        Logger = modEntry.Logger;
        Patcher = new Harmony(modEntry.Info.Id);
        
        Config.Load(modEntry.Path);
        
        modEntry.OnToggle += OnToggle;
        modEntry.OnGUI += OnGUI;
        modEntry.OnSaveGUI += OnSaveGUI;
        
        return true;
    }

    private static void Do()
    {
        Patcher?.PatchAll();
    }
    private static void Undo()
    {
        Patcher?.UnpatchAll(Patcher.Id);
    }


    private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
    {
        Config.Save(modEntry.Path);
    }
    private static void OnGUI(UnityModManager.ModEntry obj)
    {
        GUILayout.Label("Chance to split chain of Cars");
        Config.ChanceToSplitChain = GUILayout.HorizontalSlider(Config.ChanceToSplitChain, 0f, 1f);
        GUILayout.Label("Chance to apply brake on a Car");
        Config.ChanceToBrakeSingleCar = GUILayout.HorizontalSlider(Config.ChanceToBrakeSingleCar, 0f, 1f);
        GUILayout.Label("Chance to apply brake on each Car");
        Config.ChanceToBrakeAllCars = GUILayout.HorizontalSlider(Config.ChanceToBrakeAllCars, 0f, 1f);
        GUILayout.Label("Chance to leave angle cocks open on chain of cars");
        Config.ChanceToLeavePipesOpen = GUILayout.HorizontalSlider(Config.ChanceToLeavePipesOpen, 0f, 1f);
    }
    private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
    {
        if (value) Do();
        else Undo();
        return true;
    }
}