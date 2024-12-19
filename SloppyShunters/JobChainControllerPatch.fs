namespace SloppyShunters

open DV.Logic.Job
open DV.Simulation.Brake
open DV.ThingTypes
open HarmonyLib
open UnityEngine

[<HarmonyPatch(typeof<JobChainController>)>]
module JobChainControllerPatch =
    let private _random () = Random.Range(0f, 1f)
    let private _dataLog (origin: string) (msg: string) =
        Plugin.ModEntry.Value.Logger.Log $"[{origin}] {msg}"
    let TryToMessWithAirLink (chance: float32) (train: TrainCar) (job: Job) =
        if chance >= _random() then
            let coupler = 
                if train.frontCoupler.IsCoupled() then Some train.frontCoupler
                elif train.rearCoupler.IsCoupled() then Some train.rearCoupler
                else None
            
            if coupler.IsSome then
                coupler.Value.DisconnectAirHose(false)
                _dataLog (job.ID + "|" + train.ID) "Disconnected Air Hose"
            true
        else
            false
    let TryToMessWithChainScrew (chance: float32) (train: TrainCar) (job: Job) =
        if chance >= _random() then
            let coupler = 
                if train.frontCoupler.IsCoupled() then Some train.frontCoupler
                elif train.rearCoupler.IsCoupled() then Some train.rearCoupler
                else None
            if coupler.IsSome then
                coupler.Value.SetChainTight(false)
                if coupler.Value.coupledTo <> null then
                    coupler.Value.coupledTo.SetChainTight(false)
                    _dataLog (job.ID + "|" + train.ID) "Loosened Chain"
            true
        else
            false
    let TryToMessWithManualBrakes (chance: float32) (train: TrainCar) (job: Job) =
        if train.brakeSystem.hasHandbrake && chance >= _random() then
            _dataLog (job.ID + "|" + train.ID) "Adjusted Handbrake"
            train.brakeSystem.SetHandbrakePosition (Random.Range (0.5f, 1f))
            true
        else
            false
    let TryToMessWithAirBrakes (chance: float32) (train: TrainCar) (job: Job) =
        if chance > _random() then
            let ran = _random()
            let reverseCock (cock: HoseAndCock) =
                cock.SetCock (not cock.cockOpen)
                
            let targets = [|
                if ran > 0.66f then yield train.frontCoupler
                elif ran > 0.33f then yield train.rearCoupler
                else
                    yield train.frontCoupler
                    yield train.rearCoupler
            |]
            
            for target in targets do
                reverseCock target.hoseAndCock
                if target.IsCoupled() then
                    reverseCock target.coupledTo.hoseAndCock
                _dataLog (job.ID + "|" + train.ID) "Swapped Cock(s)"
            true
        else
            false
    let Sloppify (trains: TrainCar array) (generatedJob: Job) =
        CoroutineManager.Instance.StartCoroutine (seq {
            yield WaitForSeconds (Coupler.AUTO_COUPLE_DELAY * 2f)
            Plugin.ModEntry.Value.Logger.Log $"Sloppifying Job {generatedJob.ID} with {trains.Length} train(s).."
            for train in trains do
                let mutable messedWithAirLink = false
                let mutable messedWithChainScrew = false
                let mutable messedWithHandbrake = false
                let mutable messedWithAirBrakes = false
                for roll=1 to PluginConfig.RollsPerTrainCar do
                    if not messedWithAirLink then
                        messedWithAirLink <- TryToMessWithAirLink PluginConfig.ChanceToMessWithAirLink train generatedJob
                    if not messedWithChainScrew then
                        messedWithChainScrew <- TryToMessWithChainScrew PluginConfig.ChanceToMessWithChainScrew train generatedJob
                    if not messedWithHandbrake then
                        messedWithHandbrake <- TryToMessWithManualBrakes PluginConfig.ChanceToMessWithManualBrakes train generatedJob
                    if not messedWithAirBrakes then
                        messedWithAirBrakes <- TryToMessWithAirBrakes PluginConfig.ChanceToMessWithAirBrakes train generatedJob
        } |> (fun s -> s.GetEnumerator())) |> ignore
        ()
    
    [<HarmonyPatch("FinalizeSetupAndGenerateFirstJob")>] [<HarmonyPostfix>]
    let JobGenerated (__instance: JobChainController) (jobLoadedFromSavegame: bool) =
        if Plugin.Enabled then
            if jobLoadedFromSavegame then
                Plugin.ModEntry.Value.Logger.Log $"Skipping sloppification of Job[{__instance.currentJobInChain.ID}], Job is loaded from savegame!"
            elif PluginConfig.JobsToRollOn |> List.contains __instance.currentJobInChain.jobType then
                Sloppify (__instance.trainCarsForJobChain |> Array.ofSeq) __instance.currentJobInChain
            else
                Plugin.ModEntry.Value.Logger.Log $"Skipping sloppification of Job[{__instance.currentJobInChain.ID}], we don't roll on this job type."
