using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RandomizeCarSetups;

public static partial class Randomizer
{
    public struct Event(string id, Event.ShouldRun onRandomChance, Event.Randomize onTriggered)
    {
        public readonly string Id = id;
        public readonly ShouldRun OnRandomChance = onRandomChance;
        public readonly Randomize OnTriggered = onTriggered;

        public delegate bool ShouldRun(float rand);

        public delegate bool Randomize(List<TrainCar> trainCars);
    }
    
    private static bool OnEventSplitChain(List<TrainCar> trainCars)
    {
        Plugin.Logger.Log($"[Event] Splitting cars for {trainCars.First()?.trainset.id}");
        var target = trainCars[Mathf.RoundToInt(trainCars.Count / 2f) - 1];
        var firstCouplerInRange = target.rearCoupler.GetFirstCouplerInRange();

        if (target.brakeSystem.hasHandbrake)
            target.brakeSystem.SetHandbrakePosition(Random.Range(0.5f, 1f));

        if (firstCouplerInRange && firstCouplerInRange.train.brakeSystem.hasHandbrake)
            firstCouplerInRange.train.brakeSystem.SetHandbrakePosition(Random.Range(0.5f, 1f));

        target.rearCoupler.Uncouple();
        if (firstCouplerInRange)
            firstCouplerInRange.Uncouple();

        return true;
    }
    private static bool OnEventBrakeOne(List<TrainCar> trainCars)
    {
        Plugin.Logger.Log($"[Event] Applying brake on single car for {trainCars.First()?.trainset.id}");
        var target = trainCars[Random.Range(0, trainCars.Count - 1)];
        if (target.brakeSystem.hasHandbrake)
            target.brakeSystem.SetHandbrakePosition(1f);
        return true;
    }
    private static bool OnEventBrakeAll(List<TrainCar> trainCars)
    {
        Plugin.Logger.Log($"[Event] Applying brake on all cars for {trainCars.First()?.trainset.id}");
        foreach (var trainCar in trainCars.Where(trainCar => trainCar.brakeSystem.hasHandbrake))
            trainCar.brakeSystem.SetHandbrakePosition(Random.Range(0.1f, 0.5f));
        return true;
    }
    private static bool OnEventOpenPipes(List<TrainCar> trainCars)
    {
        Plugin.Logger.Log($"[Event] Opening Angle-Cocks for {trainCars.First()?.trainset.id}");
        trainCars[0].frontCoupler.IsCockOpen = true;
        trainCars[trainCars.Count - 1].frontCoupler.IsCockOpen = true;
        trainCars[0].rearCoupler.IsCockOpen = true;
        trainCars[trainCars.Count - 1].rearCoupler.IsCockOpen = true;
        return true;
    }
}