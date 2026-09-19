using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Gamelab.Editor
{
    /// <summary>
    /// One-shot setup: creates Assets/Settings/DefaultPhysicsMaterial2D.asset (friction 0.2, bounciness 0,
    /// matching Aether.Physics2D/Box2D's fixture defaults - see docs/superpowers/plans/
    /// 2026-09-18-physics-movement-port-plan.md, "Design decisions") and assigns it as Unity's project-wide
    /// default 2D physics material, so every Collider2D without an explicit PhysicsMaterial2D matches the
    /// original's friction instead of Unity's own default of 0.4.
    ///
    /// There's no public C# API for the project-wide default (UnityEngine.Physics2D has no
    /// `defaultMaterial` member in this Unity version; it's a native-only field on the
    /// ProjectSettings/Physics2DSettings.asset object). This uses the same
    /// InternalEditorUtility.LoadSerializedFileAndForget / SaveToSerializedFileAndForget pattern Unity
    /// itself uses for other ProjectSettings assets (e.g. TagManager.asset), via SerializedObject/
    /// SerializedProperty against the loaded native object's "m_DefaultMaterial" field - confirmed present
    /// by reflection probing against this Unity version's UnityEditor.Physics2DSettings type.
    ///
    /// Run headless via:
    ///   Unity -batchmode -quit -projectPath Unity \
    ///     -executeMethod Gamelab.Editor.DefaultPhysicsMaterialSetup.Apply
    ///
    /// Idempotent: re-running updates the existing asset and re-assigns it in place.
    /// </summary>
    public static class DefaultPhysicsMaterialSetup
    {
        private const string AssetPath = "Assets/Settings/DefaultPhysicsMaterial2D.asset";
        private const string Physics2DSettingsPath = "ProjectSettings/Physics2DSettings.asset";

        [MenuItem("Gamelab/Physics/Apply Default Physics Material")]
        public static void Apply()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }

            PhysicsMaterial2D material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(AssetPath);
            if (material == null)
            {
                material = new PhysicsMaterial2D("DefaultPhysicsMaterial2D");
                AssetDatabase.CreateAsset(material, AssetPath);
            }

            material.friction = 0.2f;
            material.bounciness = 0f;
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();

            Object[] settingsObjects = InternalEditorUtility.LoadSerializedFileAndForget(Physics2DSettingsPath);
            if (settingsObjects.Length == 0)
            {
                Debug.LogError($"DefaultPhysicsMaterialSetup: could not load {Physics2DSettingsPath}");
                return;
            }

            SerializedObject settings = new SerializedObject(settingsObjects[0]);
            SerializedProperty defaultMaterialProp = settings.FindProperty("m_DefaultMaterial");
            if (defaultMaterialProp == null)
            {
                Debug.LogError("DefaultPhysicsMaterialSetup: m_DefaultMaterial property not found on Physics2DSettings");
                return;
            }

            defaultMaterialProp.objectReferenceValue = material;
            settings.ApplyModifiedProperties();

            InternalEditorUtility.SaveToSerializedFileAndForget(settingsObjects, Physics2DSettingsPath, true);
        }
    }
}
