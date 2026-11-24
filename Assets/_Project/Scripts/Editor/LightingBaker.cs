using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Itan.EditorTools
{
    /// <summary>
    /// Bakes light for Quest. The sun is a Mixed light in Subtractive mode: the scenery gets
    /// sun, shadows and bounce light from lightmaps, and only the moving characters are lit
    /// in real time. The fire stays a real-time light (it flickers) with no shadows.
    ///
    /// The hub and the story world sit in the same space, so each scene is baked on its own.
    /// The story world is baked with a stand-in copy of the hub's sun.
    ///
    /// Menu: Itan > Lighting > Bake All (or one scene at a time). Re-bake after re-dressing.
    /// </summary>
    public static class LightingBaker
    {
        const string SettingsPath = "Assets/_Project/Settings/Lighting/QuestLighting.lighting";
        const float SmallObject = 0.6f;   // anything smaller than this (grass, pebbles, cups) uses light probes

        [MenuItem("Itan/Lighting/Bake All")]
        static void BakeAll()
        {
            Bake(StoryScenes.HubPath, false);
            Bake(StoryScenes.StoryPath, true);
            Reopen();
        }

        [MenuItem("Itan/Lighting/Bake Hub")]
        static void BakeHub() { Bake(StoryScenes.HubPath, false); Reopen(); }

        [MenuItem("Itan/Lighting/Bake Story World")]
        static void BakeStory() { Bake(StoryScenes.StoryPath, true); Reopen(); }

        [MenuItem("Itan/Lighting/Clear Baked Lighting")]
        static void ClearAll()
        {
            foreach (var path in new[] { StoryScenes.HubPath, StoryScenes.StoryPath })
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                Lightmapping.Clear();
                Lightmapping.lightingDataAsset = null;
                EditorSceneManager.SaveScene(scene);
            }
            Reopen();
        }

        static void Reopen()
        {
            EditorSceneManager.OpenScene(StoryScenes.HubPath, OpenSceneMode.Single);
            StoryScenes.OpenStory();
            var hub = SceneManager.GetSceneByPath(StoryScenes.HubPath);
            SceneManager.SetActiveScene(hub);
        }

        static void Bake(string path, bool isStory)
        {
            // Read the hub's sun before leaving it, so the story world can borrow it.
            var hub = EditorSceneManager.OpenScene(StoryScenes.HubPath, OpenSceneMode.Single);
            var sun = FindSun(hub);
            if (sun == null) { Debug.LogError("[Itan] No directional light in the hub."); return; }
            MakeSunMixed(sun);
            EditorSceneManager.SaveScene(hub);
            var sunRotation = sun.transform.rotation;
            var sunColor = sun.color;
            float sunIntensity = sun.intensity, sunBounce = sun.bounceIntensity;

            var scene = isStory ? EditorSceneManager.OpenScene(path, OpenSceneMode.Single) : hub;
            GameObject standIn = null;
            if (isStory)
            {
                standIn = new GameObject("BakeSun (temporary)");
                SceneManager.MoveGameObjectToScene(standIn, scene);
                standIn.transform.rotation = sunRotation;
                var l = standIn.AddComponent<Light>();
                l.type = LightType.Directional;
                l.color = sunColor;
                l.intensity = sunIntensity;
                l.bounceIntensity = sunBounce;
                MakeSunMixed(l);
            }

            // The story world is kept switched off until it is shown; the baker only sees active objects.
            var hidden = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                .Select(t => t.gameObject).Where(g => !g.activeSelf && g.name != "CloudSea").ToList();
            foreach (var g in hidden) g.SetActive(true);

            Lightmapping.lightingSettings = GetSettings();
            PrepareRenderers(scene);
            if (isStory) AddProbes(scene);
            foreach (var l in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Light>(true)))
                if (l.type == LightType.Point) { l.lightmapBakeType = LightmapBakeType.Realtime; l.shadows = LightShadows.None; }

            var started = System.DateTime.Now;
            bool ok = Lightmapping.Bake();
            if (standIn != null) Object.DestroyImmediate(standIn);
            foreach (var g in hidden) g.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Itan] Baked {scene.name}: {(ok ? "ok" : "FAILED")} in {(System.DateTime.Now - started).TotalSeconds:F0}s, {LightmapSettings.lightmaps.Length} lightmap(s).");
        }

        static Light FindSun(Scene scene) =>
            scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Light>(true)).FirstOrDefault(l => l.type == LightType.Directional);

        static void MakeSunMixed(Light sun)
        {
            sun.lightmapBakeType = LightmapBakeType.Mixed;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.8f;
            EditorUtility.SetDirty(sun);
        }

        static LightingSettings GetSettings()
        {
            var s = AssetDatabase.LoadAssetAtPath<LightingSettings>(SettingsPath);
            if (s == null) { s = new LightingSettings(); AssetDatabase.CreateAsset(s, SettingsPath); }
            s.name = "QuestLighting";
            s.bakedGI = true;
            s.realtimeGI = false;
            s.mixedBakeMode = MixedLightingMode.Subtractive;
            s.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
            s.lightmapResolution = 5f;          // texels per metre: soft, and small on the headset
            s.lightmapPadding = 2;
            s.lightmapMaxSize = 1024;
            s.lightmapCompression = LightmapCompression.NormalQuality;
            s.directionalityMode = LightmapsMode.NonDirectional;
            s.ao = true;
            s.aoMaxDistance = 0.6f;
            s.directSampleCount = 32;
            s.indirectSampleCount = 256;
            s.environmentSampleCount = 128;
            s.maxBounces = 2;
            s.indirectScale = 1.4f;             // dusk scenes: lift the bounce light a little
            s.filteringMode = LightingSettings.FilterMode.Auto;
            s.denoiserTypeDirect = LightingSettings.DenoiserType.OpenImage;
            s.denoiserTypeIndirect = LightingSettings.DenoiserType.OpenImage;
            s.denoiserTypeAO = LightingSettings.DenoiserType.OpenImage;
            EditorUtility.SetDirty(s);
            AssetDatabase.SaveAssets();
            return s;
        }

        /// <summary>
        /// Big static scenery gets lightmaps (and a lightmap UV set if it lacks one); small or
        /// see-through bits use light probes so the lightmaps stay small.
        /// </summary>
        static void PrepareRenderers(Scene scene)
        {
            var reimport = new HashSet<string>();
            int mapped = 0, probed = 0;
            foreach (var r in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<MeshRenderer>(true)))
            {
                var go = r.gameObject;
                bool isStatic = GameObjectUtility.GetStaticEditorFlags(go).HasFlag(StaticEditorFlags.ContributeGI);
                if (!isStatic) continue;
                var mf = r.GetComponent<MeshFilter>();
                var mesh = mf != null ? mf.sharedMesh : null;
                bool cutout = r.sharedMaterials.Any(m => m != null && (m.IsKeywordEnabled("_ALPHATEST_ON") || (m.HasProperty("_AlphaClip") && m.GetFloat("_AlphaClip") > 0.5f) || m.GetTag("RenderType", false) == "TransparentCutout" || m.renderQueue >= 2450));
                // Grass, reeds and bare branches are thin cards: lightmaps make them dark, probes keep them lit by the sun.
                bool thin = go.name.Contains("Grass") || go.name.Contains("Reed") || go.name.Contains("Bush") || go.name.Contains("Plant") || go.name.Contains("Fern");
                bool small = r.bounds.size.magnitude < SmallObject;
                if (mesh == null || cutout || thin || small || go.name == "CloudSea")
                {
                    r.receiveGI = ReceiveGI.LightProbes;
                    probed++;
                    continue;
                }
                r.receiveGI = ReceiveGI.Lightmaps;
                r.scaleInLightmap = r.bounds.size.magnitude > 20f ? 0.5f : 1f;
                mapped++;
                if (mesh.uv2 != null && mesh.uv2.Length > 0) continue;
                string asset = AssetDatabase.GetAssetPath(mesh);
                if (AssetImporter.GetAtPath(asset) is ModelImporter mi)
                {
                    if (!mi.generateSecondaryUV) { mi.generateSecondaryUV = true; reimport.Add(asset); }
                }
                else if (asset.EndsWith(".asset"))
                {
                    Unwrapping.GenerateSecondaryUVSet(mesh);
                    EditorUtility.SetDirty(mesh);
                }
            }
            foreach (var a in reimport) AssetDatabase.ImportAsset(a, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Itan] {scene.name}: {mapped} lightmapped, {probed} on probes, {reimport.Count} models given lightmap UVs.");
        }

        /// <summary>A grid of light probes over the ground where the characters walk, plus the sky island.</summary>
        static void AddProbes(Scene scene)
        {
            var old = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<LightProbeGroup>(true)).ToList();
            foreach (var g in old) Object.DestroyImmediate(g.gameObject);

            var root = scene.GetRootGameObjects()[0].transform;
            var group = new GameObject("LightProbes").AddComponent<LightProbeGroup>();
            group.transform.SetParent(root, false);
            var points = new List<Vector3>();
            for (float x = -7f; x <= 7.01f; x += 1.75f)
                for (float z = -2f; z <= 12.01f; z += 1.75f)
                    foreach (float y in new[] { 0.3f, 1.3f, 2.8f })
                        points.Add(new Vector3(x, y, z));
            float top = StoryWorldDresser.CloudTop;
            for (float x = -4f; x <= 4.01f; x += 2f)
                for (float z = 4f; z <= 11.01f; z += 1.75f)
                    foreach (float y in new[] { top + 0.3f, top + 1.4f })
                        points.Add(new Vector3(x, y, z));
            group.probePositions = points.Select(p => group.transform.InverseTransformPoint(p)).ToArray();
        }
    }
}
