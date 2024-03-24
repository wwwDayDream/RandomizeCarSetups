using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RandomizeCarSetups;

[HarmonyPatch(typeof(CarSpawner), nameof(CarSpawner.SpawnCars))]
[UsedImplicitly]
public class CarSpawnerSpawnCars
{
    [UsedImplicitly]
    public static void Postfix(CarSpawner __instance, List<TrainCar> __result)
    {
        var splitCarsValue = Random.Range(0f, 1f) <= Config.ChanceToSplitChain;
        var brakeSingleCar = Random.Range(0f, 1f) <= Config.ChanceToBrakeSingleCar;
        var chanceToBrakeAllCars = Random.Range(0f, 1f) <= Config.ChanceToBrakeAllCars;
        var chanceToLeavePipesOpen = Random.Range(0f, 1f) <= Config.ChanceToLeavePipesOpen;

        if (splitCarsValue && __result.Count > 3)
        {
            var target = __result[__result.Count / 2 - 1];
            target.rearCoupler.preventAutoCouple = true;
            if (target.brakeSystem.hasHandbrake) 
                target.brakeSystem.SetHandbrakePosition(Random.Range(0.5f, 1f));
            var firstCouplerInRange = target.rearCoupler.GetFirstCouplerInRange();
            if (firstCouplerInRange && firstCouplerInRange.train.brakeSystem.hasHandbrake) 
                firstCouplerInRange.train.brakeSystem.SetHandbrakePosition(Random.Range(0.5f, 1f));
            if (firstCouplerInRange)
                firstCouplerInRange.preventAutoCouple = true;
        }

        if (brakeSingleCar && __result.Count > 1)
        {
            var target = __result[Random.Range(0, __result.Count - 1)];
            if (target.brakeSystem.hasHandbrake)
                target.brakeSystem.SetHandbrakePosition(1f);
        } else if (chanceToBrakeAllCars)
        {
            foreach (var trainCar in __result)
                if (trainCar.brakeSystem.hasHandbrake)
                    trainCar.brakeSystem.SetHandbrakePosition(Random.Range(0f, 0.5f));
        }

        if (chanceToLeavePipesOpen && __result.Count > 1)
        {
            __result[0].frontCoupler.IsCockOpen = true;
            __result[__result.Count - 1].frontCoupler.IsCockOpen = true;
            __result[0].rearCoupler.IsCockOpen = true;
            __result[__result.Count - 1].rearCoupler.IsCockOpen = true;
        }
    }
}