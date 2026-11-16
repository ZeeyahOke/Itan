using Itan.Session;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Small, repeatable steps that add one system at a time to the open Hub scene.
    /// Each item does nothing if the object is already there.
    /// </summary>
    public static class HubSceneSetup
    {
        [MenuItem("Itan/Scene/Add Session Manager")]
        public static void AddSessionManager()
        {
            if (Object.FindFirstObjectByType<SessionManager>() != null)
            {
                Debug.Log("[Itan] Scene already has a SessionManager.");
                return;
            }

            var go = new GameObject("SessionManager");
            go.AddComponent<SessionManager>();
            Undo.RegisterCreatedObjectUndo(go, "Add Session Manager");
            SaveScene();
        }

        static void SaveScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
