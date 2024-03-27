using System.Collections.Generic;
using System.Linq;
using DV.Common;

namespace RandomizeCarSetups;

public static class SaveDataManager
{
    private const string SaveGameKeyRandomizedJobIDs = "SloppyShunters.RandomizedJobIDs";

    public static List<string> RandomizedJobIDs = [];

    public static void Init()
    {
        SaveGameManager.AboutToSave += SaveGameManagerOnAboutToSave;
    }
    public static void LoadFromSave(SaveGameManager saveManager)
    {
        var saveData = saveManager.data.GetStringArray(SaveGameKeyRandomizedJobIDs);
        if (saveData == null)
            Plugin.Logger.Log("SaveData doesn't contain any already RandomizedJobIDs.");
        else
            RandomizedJobIDs = saveData.ToList();
    }

    private static void SaveGameManagerOnAboutToSave(SaveType obj)
    {
        Plugin.Logger.Log("Saving RandomizedJobIDs to SaveGame.");
        SaveGameManager.Instance.data.SetStringArray(SaveGameKeyRandomizedJobIDs, RandomizedJobIDs.ToArray());
    }

    public static void UnTrackJobId(string id)
    {
        var toRemove = RandomizedJobIDs.Where(randomizedJobID => randomizedJobID.StartsWith(id)).ToList();

        foreach (var deStr in toRemove)
            RandomizedJobIDs.Remove(deStr);
    }
}