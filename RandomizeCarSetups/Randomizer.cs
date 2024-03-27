using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using RandomizeCarSetups.Patches;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RandomizeCarSetups;

public static partial class Randomizer
{
    [UsedImplicitly]
    public static readonly List<Event> ActiveEvents =
    [
        new Event("dd.brake", f => f <= Config.ChanceToBrakeSingleCar, OnEventBrakeOne),
        new Event("dd.split", f => f <= Config.ChanceToSplitChain, OnEventSplitChain),
        new Event("dd.open-pipes", f => f <= Config.ChanceToLeavePipesOpen, OnEventOpenPipes),
        new Event("dd.brake-all", f => f <= Config.ChanceToBrakeAllCars, OnEventBrakeAll),
    ];

    public static void PreTryToGenerateJobs(StationController stationController)
    {
        NumberOfJobsTemp = stationController.ProceduralJobsController.jobChainControllers.Count;
        Plugin.Logger.Log(
            $"Station {stationController.name} contains {stationController.ProceduralJobsController.jobChainControllers.Count} " +
            $"job{(stationController.ProceduralJobsController.jobChainControllers.Count == 1 ? "" : "s")} before " +
            $"'{nameof(StationProceduralJobsController)}::{nameof(StationProceduralJobsController.TryToGenerateJobs)}'");
    }
    private static int NumberOfJobsTemp { get; set; }
    public static IEnumerator RandomizeStationJobs(StationController stationController)
    {
        yield return new WaitUntil(() =>
            stationController.ProceduralJobsController.generationCoro == null);

        var isJobFirstSpawn = stationController.ProceduralJobsController.jobChainControllers.Count != NumberOfJobsTemp;

        if (Config.RandomizeOnSpawnOnly && !isJobFirstSpawn) yield break;
        
        Plugin.Logger.Log(isJobFirstSpawn ? "Randomizing on station first entry." : "Randomizing on station re-entry");

        foreach (var jobChainController in stationController.ProceduralJobsController.jobChainControllers)
            RandomizeTrainCars(jobChainController.trainCarsForJobChain, jobChainController);
    }

    private static void RandomizeTrainCars(List<TrainCar> trainCarsForJobChain, JobChainController chainController)
    {
        for (var i = 0; i < Config.EventsPerJob; i++)
        {
            var randomEvent = ActiveEvents[Random.Range(0, ActiveEvents.Count - 1)];
            var saveKey = chainController.currentJobInChain.ID + "." + randomEvent.Id;
            if (SaveDataManager.RandomizedJobIDs.Contains(saveKey))
                continue;

            if (randomEvent.OnRandomChance(Random.Range(0f, 1f)) && randomEvent.OnTriggered(trainCarsForJobChain))
                SaveDataManager.RandomizedJobIDs.Add(saveKey);

        }
    }
}