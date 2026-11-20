using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Sets up Quaternius' Universal Animation Libraries (CC0) as Unity's setup sheet asks:
    /// bake axis conversion, humanoid rig, loop the *_Loop clips. Our key moments move the
    /// characters themselves, so root motion is baked into the pose (clips play in place).
    /// Menu: Itan > Characters > Set Up Animation Libraries
    /// </summary>
    public static class AnimationLibrarySetup
    {
        public static readonly string[] Libraries =
        {
            "Assets/_ThirdParty/Quaternius/UniversalAnimationLibrary/Animation Library[Standard]/Unity/AnimationLibrary_Unity_Standard.fbx",
            "Assets/_ThirdParty/Quaternius/UniversalAnimationLibrary2/Universal Animation Library 2 [Standard]/Unity/UAL2_Standard.fbx",
        };

        [MenuItem("Itan/Characters/Set Up Animation Libraries")]
        public static void Setup()
        {
            var report = new StringBuilder();
            foreach (var path in Libraries)
            {
                var m = AssetImporter.GetAtPath(path) as ModelImporter;
                if (m == null) { report.AppendLine("missing " + path); continue; }
                m.bakeAxisConversion = true;
                m.animationType = ModelImporterAnimationType.Human;
                m.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                m.importAnimation = true;
                m.materialImportMode = ModelImporterMaterialImportMode.None;
                m.SaveAndReimport();

                var clips = m.defaultClipAnimations;
                foreach (var c in clips)
                {
                    c.loopTime = c.name.EndsWith("_Loop");
                    c.lockRootRotation = true;
                    c.keepOriginalOrientation = true;
                    c.lockRootHeightY = true;
                    c.keepOriginalPositionY = true;
                    c.lockRootPositionXZ = true;
                    c.keepOriginalPositionXZ = true;
                }
                m.clipAnimations = clips;
                m.SaveAndReimport();
                report.AppendLine($"== {Path.GetFileName(path)} ({clips.Length} clips)");
                report.AppendLine(string.Join(", ", clips.Select(c => c.name)));
            }
            File.WriteAllText("Logs/itan_anim_clips.txt", report.ToString());
            Debug.Log("[Itan] Animation libraries set up.");
        }

        /// <summary>Find a clip from the libraries by its exact name.</summary>
        public static AnimationClip Clip(string name)
        {
            foreach (var path in Libraries)
                foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (o is AnimationClip c && c.name == name) return c;
            Debug.LogWarning("[Itan] Animation clip not found: " + name);
            return null;
        }
    }
}
