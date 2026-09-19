using FMODUnity;
using UnityEditor;
using UnityEngine;

namespace Gamelab.Editor.Sound
{
    // One-off configuration: points the FMOD Unity integration at the
    // existing FMOD Studio soundbanks under Src/Content/soundbanks instead
    // of rebuilding them inside the Unity project (the FmodProject/ and
    // .bank files carry over unchanged per the port spec's "Target"
    // section and this work order).
    public static class FmodSoundbankSetup
    {
        [MenuItem("Tools/FMOD/Configure Soundbank Path")]
        public static void ConfigureSoundbankPath()
        {
            Settings settings = Settings.Instance;
            settings.SourceBankPath = "../Src/Content/soundbanks";
            settings.ImportType = ImportType.StreamingAssets;

            // Note: BankLoadType lives directly on Settings in the vendored
            // 2.02 source (Settings.cs), not per-Platform as the plan
            // assumed. FMODUnity.Platform has no BankLoadType member.
            settings.BankLoadType = BankLoadType.All;

            // Src/Content/soundbanks holds the .bank files flat (no per-
            // platform subfolder like "Desktop") - HasPlatforms=true makes
            // EventManager look for SourceBankPath + "/" + BuildDirectory,
            // which doesn't exist here and throws DirectoryNotFoundException.
            settings.HasPlatforms = false;

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            // Populate Settings.MasterBanks/Banks by scanning SourceBankPath -
            // BankLoadType.All (RuntimeManager.BanksToLoad) reads those lists,
            // not the source folder directly, so without this the editor
            // never actually loads any bank even though the setting is "All".
            EventManager.RefreshBanks();

            Debug.Log($"FMOD SourceBankPath set to {settings.SourceBankPath}, ImportType={settings.ImportType}");
        }
    }
}
