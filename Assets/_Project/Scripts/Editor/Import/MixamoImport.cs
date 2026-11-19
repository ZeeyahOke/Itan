using UnityEditor;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Import rules for the third-party characters (Mixamo and Polygonal Mind): humanoid rig,
    /// no camera/light clutter, textures capped for Quest.
    /// </summary>
    public class MixamoImport : AssetPostprocessor
    {
        static readonly string[] Roots = { "Assets/_ThirdParty/Mixamo/", "Assets/_ThirdParty/PolygonalMind/" };

        static bool IsMixamo(string path)
        {
            path = path.Replace('\\', '/');
            foreach (var r in Roots) if (path.StartsWith(r)) return true;
            return false;
        }

        void OnPreprocessModel()
        {
            if (!IsMixamo(assetPath)) return;
            var m = (ModelImporter)assetImporter;
            if (m.importSettingsMissing == false && m.userData == "itan") return; // keep manual tweaks
            m.userData = "itan";
            m.animationType = ModelImporterAnimationType.Human;
            m.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            m.importCameras = false;
            m.importLights = false;
            m.importBlendShapes = false;
            m.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            m.meshCompression = ModelImporterMeshCompression.Medium;
            m.optimizeGameObjects = false;
            bool isAnim = assetPath.Contains("/Animations/");
            m.importAnimation = isAnim;
            if (isAnim) m.materialImportMode = ModelImporterMaterialImportMode.None;
        }

        void OnPreprocessTexture()
        {
            if (!IsMixamo(assetPath)) return;
            var t = (TextureImporter)assetImporter;
            t.maxTextureSize = 1024;
            if (assetPath.Contains("Normal")) t.textureType = TextureImporterType.NormalMap;
        }
    }
}
