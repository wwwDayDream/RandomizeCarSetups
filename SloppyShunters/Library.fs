namespace SloppyShunters

open System
open System.IO
open DV.ThingTypes
open UnityEngine
open UnityModManagerNet

module PluginConfig =
    let mutable ChanceToMessWithLink: float32 = 0.025f
    let mutable ChanceToMessWithManualBrakes: float32 = 0.025f
    let mutable ChanceToMessWithAirBrakes: float32 = 0.01f
    let mutable RollsPerTrainCar: int = 1
    let mutable JobsToRollOn: JobType list = [
        JobType.Transport;JobType.ComplexTransport;JobType.EmptyHaul
        JobType.ShuntingLoad;JobType.ShuntingUnload
    ]
    
    let private configFilePath (modEntry: UnityModManager.ModEntry) =
        Path.Combine(modEntry.Path, "config.dat")
    
    let Save (modEntry: UnityModManager.ModEntry) =
        use writer = new BinaryWriter (File.OpenWrite(configFilePath modEntry))
        writer.Write ChanceToMessWithLink
        writer.Write ChanceToMessWithManualBrakes
        writer.Write ChanceToMessWithAirBrakes
        writer.Write RollsPerTrainCar
        
        writer.Write JobsToRollOn.Length
        for job in JobsToRollOn do
            writer.Write(byte job)
        
    let Load (modEntry: UnityModManager.ModEntry) =
        if File.Exists (configFilePath modEntry) then
            use reader = new BinaryReader (File.OpenRead(configFilePath (modEntry)))
            try
                ChanceToMessWithLink <- reader.ReadSingle()
                ChanceToMessWithManualBrakes <- reader.ReadSingle()
                ChanceToMessWithAirBrakes <- reader.ReadSingle()
                RollsPerTrainCar <- reader.ReadInt32()
               
                let jobsCount = reader.ReadInt32()
                let mutable jobs = []
                for _=0 to jobsCount - 1 do
                   let byteAsJob: JobType = Enum.ToObject(typeof<JobType>, reader.ReadByte()) :?> JobType
                   jobs <- byteAsJob :: jobs
                JobsToRollOn <- jobs
            with _ ->
                Save(modEntry)
               
module Plugin =
    open HarmonyLib
    open UnityModManagerNet
    open Operators
    
    let mutable internal Patcher: Harmony option = None
    let mutable internal Enabled: bool = false
    let mutable internal ModEntry: UnityModManager.ModEntry option = None

    let OnToggle (modEntry: UnityModManager.ModEntry) (value: bool): bool =
        Enabled <- value
        ModEntry <- Some modEntry
        Patcher <- Harmony modEntry.Info.Id |> Some
        
        PluginConfig.Load modEntry
        
        if Patcher.IsNone then
            Patcher <- Harmony("wwwDayDream.SloppyShunters") |> Some
            
        if Enabled then
            Patcher.Value.PatchAll()
        else
            Patcher.Value.UnpatchAll()
            
        let methodCount = Patcher.Value.GetPatchedMethods() |> Seq.length
        let methodCountPlural = if methodCount = 1 then "" else "s"
        
        ModEntry.Value.Logger.Log $"Patched {methodCount} method{methodCountPlural}."
            
        true
    
    let OnSaveGUI (modEntry: UnityModManager.ModEntry) =
        PluginConfig.Save modEntry
        
    let OnGUI (modEntry: UnityModManager.ModEntry) =
        GUILayout.Label "How many rolls of all chances per TrainCar"
        let rollsPerTrainCarField, rollsPerTrainCarInt = Int32.TryParse(GUILayout.TextField(string PluginConfig.RollsPerTrainCar, 2))
        PluginConfig.RollsPerTrainCar <- if rollsPerTrainCarField then rollsPerTrainCarInt else PluginConfig.RollsPerTrainCar
        
        GUILayout.Space(5f)
        GUILayout.Label "What Job Types to roll on"
        GUILayout.BeginHorizontal()
        let mutable jobTypesToRandomize = []
        let mutable idx = 1
        for enumEntry in Enum.GetNames(typeof<JobType>) do
            let curJobType = Enum.Parse(typeof<JobType>, enumEntry) :?> JobType
            let curJobEnabled = GUILayout.Toggle (PluginConfig.JobsToRollOn |> List.contains curJobType, enumEntry)
            if curJobEnabled then
                jobTypesToRandomize <- curJobType :: jobTypesToRandomize
            if idx % 3 = 0 then
                GUILayout.EndHorizontal()
                GUILayout.BeginHorizontal()
            idx <- idx + 1
        PluginConfig.JobsToRollOn <- jobTypesToRandomize
        GUILayout.EndHorizontal()
        
        GUILayout.Space(5f)
        GUILayout.Label "Chance to mess with link(s)"
        PluginConfig.ChanceToMessWithLink <- GUILayout.HorizontalSlider(PluginConfig.ChanceToMessWithLink, 0f, 1f)
        GUILayout.Label "Chance to mess with manual brakes" 
        PluginConfig.ChanceToMessWithManualBrakes <- GUILayout.HorizontalSlider(PluginConfig.ChanceToMessWithManualBrakes, 0f, 1f)
        GUILayout.Label "Chance to mess with air brakes" 
        PluginConfig.ChanceToMessWithAirBrakes <- GUILayout.HorizontalSlider(PluginConfig.ChanceToMessWithAirBrakes, 0f, 1f)
        
        
    let Load (modEntry: UnityModManager.ModEntry): bool =
        modEntry.OnToggle <- OnToggle
        modEntry.OnGUI <- OnGUI;
        modEntry.OnSaveGUI <- OnSaveGUI;

        true