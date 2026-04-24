using System;
using System.IO;
using Newtonsoft.Json;

namespace Gamelab.Serialization;

public static class SaveManager
{
    private static readonly string SaveDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gamelab");

    private static readonly string SaveFile = Path.Combine(SaveDirectory, "run_save.json");

    public static void SaveRun(RunSession session)
    {
        Directory.CreateDirectory(SaveDirectory);
        string json = JsonConvert.SerializeObject(session, Formatting.Indented);
        File.WriteAllText(SaveFile, json);
    }

    public static RunSession LoadRun()
    {
        if (!HasSave()) return null;

        try
        {
            string json = File.ReadAllText(SaveFile);
            return JsonConvert.DeserializeObject<RunSession>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load save file: {ex.Message}");
            return null;
        }
    }

    public static bool HasSave() => File.Exists(SaveFile);

    public static void DeleteSave()
    {
        if (HasSave())
        {
            File.Delete(SaveFile);
        }
    }
}