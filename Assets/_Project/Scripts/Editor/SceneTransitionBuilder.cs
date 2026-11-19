using System.Collections.Generic;
using Itan.Session;
using Itan.World;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Adds the smoke particle effect at the fire, the fade quad in front of
    /// the camera and the SceneTransition that ties them together.
    /// Menu: Itan > Scene > Add Scene Transition
    /// </summary>
    public static class SceneTransitionBuilder
    {
        const string VfxFolder = "Assets/_Project/Art/VFX";
        static readonly Vector3 FirePosition = new Vector3(0f, 0.2f, 2f);

        [MenuItem("Itan/Scene/Add Scene Transition")]
        public static void Build()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            var origin = Object.FindFirstObjectByType<XROrigin>();
            if (session == null || origin == null) { Debug.LogError("[Itan] Need a SessionManager and an XR Origin."); return; }
            if (Object.FindFirstObjectByType<SceneTransition>() != null) { Debug.Log("[Itan] Scene transition already added."); return; }

            var root = new GameObject("SceneTransition");
            var smoke = BuildSmoke(root.transform);
            var fade = BuildFadeQuad(origin.Camera.transform);

            // Hub pieces to hide in the story: the compound, the calabash stand,
            // and the elder's body (his voice object stays on so narration still plays).
            var hubObjects = new List<GameObject> { Find("Hub"), Find("CalabashStand") };
            var elder = Find("Elder");
            if (elder != null)
                foreach (Transform child in elder.transform)
                    if (child.name != "Voice") hubObjects.Add(child.gameObject);

            var transition = root.AddComponent<SceneTransition>();
            var so = new SerializedObject(transition);
            var list = so.FindProperty("hubObjects");
            list.arraySize = hubObjects.Count;
            for (int i = 0; i < hubObjects.Count; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = hubObjects[i];
            so.FindProperty("smoke").objectReferenceValue = smoke;
            so.FindProperty("fadeQuad").objectReferenceValue = fade;
            so.ApplyModifiedPropertiesWithoutUndo();

            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("transition").objectReferenceValue = transition;
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Scene transition added.");
        }

        static ParticleSystem BuildSmoke(Transform parent)
        {
            var go = new GameObject("Smoke");
            go.transform.SetParent(parent);
            go.transform.position = FirePosition;
            go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // emit upward

            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.duration = 3f;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.8f, 4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.3f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.2f, 2.4f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.55f, 0.55f, 0.68f, 0.7f);
            main.gravityModifier = -0.05f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 160;

            var emission = ps.emission;
            emission.rateOverTime = 40f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 28f;
            shape.radius = 0.45f;

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.8f));

            var color = ps.colorOverLifetime;
            color.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.8f, 0.2f), new GradientAlphaKey(0f, 1f) });
            color.color = g;

            var rot = ps.rotationOverLifetime;
            rot.enabled = true;
            rot.z = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = SmokeMaterial();
            renderer.sortMode = ParticleSystemSortMode.Distance;
            return ps;
        }

        static Material SmokeMaterial()
        {
            string path = VfxFolder + "/Smoke.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            // Start from URP's own particle material so the transparent setup is right.
            var urpDefault = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat");
            mat = urpDefault != null ? new Material(urpDefault) : new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(VfxFolder + "/Smoke_Soft.png");
            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", Color.white);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        static Renderer BuildFadeQuad(Transform camera)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "FadeQuad";
            Object.DestroyImmediate(quad.GetComponent<Collider>());
            quad.transform.SetParent(camera, false);

            string path = VfxFolder + "/Fade.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                // Transparent, alpha blended, drawn after everything else.
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetFloat("_ZWrite", 0f);
                if (mat.HasProperty("_ZTest")) mat.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = 4000;
                mat.SetColor("_BaseColor", new Color(0.1f, 0.1f, 0.18f, 1f));
                AssetDatabase.CreateAsset(mat, path);
            }
            var r = quad.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.enabled = false;
            return r;
        }

        static GameObject Find(string name)
        {
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == name && t.parent == null) return t.gameObject;
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == name) return t.gameObject;
            return null;
        }
    }
}
