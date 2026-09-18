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

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log($"FMOD SourceBankPath set to {settings.SourceBankPath}, ImportType={settings.ImportType}");
        }
    }
}
