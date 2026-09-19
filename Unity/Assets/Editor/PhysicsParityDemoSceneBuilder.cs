using System.Collections.Generic;
using Gamelab.PhysicalEntities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gamelab.Editor
{
    /// <summary>
    /// Regenerates Assets/Scenes/PhysicsParityDemo.unity: a Player-parity dynamic circle body pushed into
    /// a static wall, for a human to open in the Editor and visually compare against the MonoGame
    /// reference build (Wave A verification gate). Run headless via:
    ///   Unity -batchmode -quit -projectPath Unity -executeMethod Gamelab.Editor.PhysicsParityDemoSceneBuilder.Build
    /// </summary>
    public static class PhysicsParityDemoSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/PhysicsParityDemo.unity";

        [MenuItem("Gamelab/Physics/Rebuild Parity Demo Scene")]
        public static void Build()
        {
            UnityEngine.SceneManagement.Scene scene =
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 3f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.tag = "MainCamera";

            GameObject entityObject = new GameObject("PlayerParityBody");
            PhysicalEntity entity = entityObject.AddComponent<PhysicalEntity>();
            entity.ConfigureAsDynamicCircle(0.1f, 3f, 20f, true);
            // Not entity.Position: Rigidbody2D.position only syncs back to the Transform while physics is
            // actually stepping, which doesn't happen outside Play mode (i.e. here, during -executeMethod).
            // Setting it that way silently leaves the saved Transform at the origin, on top of the wall.
            entityObject.transform.position = new Vector3(-2f, 0f, 0f);
            entityObject.AddComponent<PushTowardWallDemo>();

            // Visual sprite lives on a child object, not entityObject itself: scaling entityObject's own
            // transform would also scale its CircleCollider2D radius (Unity multiplies collider size by
            // lossyScale), silently breaking the physics parity this scene exists to demonstrate.
            AddVisualChild(entityObject, "UI/Skin/Knob.psd", new Vector2(0.2f, 0.2f));

            GameObject wallObject = new GameObject("Wall");
            wallObject.transform.position = Vector3.zero;
            BoxCollider2D wallCollider = wallObject.AddComponent<BoxCollider2D>();
            wallCollider.size = new Vector2(0.2f, 2f);
            Rigidbody2D wallBody = wallObject.AddComponent<Rigidbody2D>();
            wallBody.bodyType = RigidbodyType2D.Static;

            AddVisualChild(wallObject, "UI/Skin/Background.psd", new Vector2(0.2f, 2f));

            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterInBuildSettings();
        }

        /// <summary>
        /// Adds a child "Visual" GameObject with a SpriteRenderer using a builtin sprite, scaled to
        /// <paramref name="worldSizeMeters"/>. Kept off the parent so the parent's own transform scale
        /// (which colliders on the parent are sensitive to) stays at 1.
        /// </summary>
        private static void AddVisualChild(GameObject parent, string builtinSpritePath, Vector2 worldSizeMeters)
        {
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(parent.transform, false);

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(builtinSpritePath);
            renderer.sprite = sprite;

            // Builtin sprites import at 100 PPU by default, so a `sprite.rect` of e.g. 16x16px is 0.16m.
            // Scale the child so the rendered sprite matches the requested world size.
            Vector2 spriteSizeMeters = sprite.rect.size / sprite.pixelsPerUnit;
            visual.transform.localScale = new Vector3(
                worldSizeMeters.x / spriteSizeMeters.x,
                worldSizeMeters.y / spriteSizeMeters.y,
                1f);
        }

        private static void RegisterInBuildSettings()
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene s in existing)
            {
                if (s.path == ScenePath) return;
            }

            List<EditorBuildSettingsScene> updated = new List<EditorBuildSettingsScene>(existing)
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = updated.ToArray();
        }
    }
}
