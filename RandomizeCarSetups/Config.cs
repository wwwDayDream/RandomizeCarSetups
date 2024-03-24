using System.IO;

namespace RandomizeCarSetups;

internal static class Config
{
    internal static float ChanceToSplitChain { get; set; } = 0.05f;
    internal static float ChanceToBrakeSingleCar { get; set; } = 0.1f;
    internal static float ChanceToBrakeAllCars { get; set; } = 0.01f;
    internal static float ChanceToLeavePipesOpen { get; set; } = 0.15f;
    private static string ConfigFile(string dir) => Path.Combine(dir, "config.dat");
    
    internal static void Load(string path)
    {
        if (!File.Exists(ConfigFile(path))) return;
        using var reader = new BinaryReader(File.OpenRead(ConfigFile(path)));
        ChanceToSplitChain = reader.ReadSingle();
        ChanceToBrakeSingleCar = reader.ReadSingle();
        ChanceToBrakeAllCars = reader.ReadSingle();
        ChanceToLeavePipesOpen = reader.ReadSingle();
    }
    internal static void Save(string path)
    {
        using var writer = new BinaryWriter(File.OpenWrite(ConfigFile(path)));
        writer.Write(ChanceToSplitChain);
        writer.Write(ChanceToBrakeSingleCar);
        writer.Write(ChanceToBrakeAllCars);
        writer.Write(ChanceToLeavePipesOpen);
    }
}