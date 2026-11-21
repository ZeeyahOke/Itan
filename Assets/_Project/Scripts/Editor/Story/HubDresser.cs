using System.Collections.Generic;
using Itan.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static Itan.EditorTools.StoryWorldDresser;

namespace Itan.EditorTools
{
    /// <summary>
    /// Turns the greybox fireside into a lived-in Yoruba compound: earth and clay
    /// textures, a real tree, a crackling fire with flames and embers, carved stools,
    /// and the everyday things of a family yard (pots, mortar, drum, baskets, firewood,
    /// adire cloth drying on a line). Everything new goes under Hub/Dressing.
    /// Menu: Itan > Hub > Dress Hub
    /// </summary>
    public static class HubDresser
    {
        const string VfxDir = "Assets/_Project/Art/VFX";

        [MenuItem("Itan/Hub/Dress Hub")]
        public static void Dress()
        {
            var hub = StoryScenes.Find("Hub");
            if (hub == null) { Debug.LogError("[Itan] Open Hub.unity first."); return; }
            Seed(5);
            var old = hub.transform.Find("Dressing");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var root = Group("Dressing", hub.transform, Vector3.zero);

            Surfaces(hub.transform);
            Tree(root);
            Fire(root);
            Seats(root);
            Props(root);
            Plants(root);

            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.GetComponent<ParticleSystem>() == null)
                    GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI | StaticEditorFlags.OccludeeStatic);
            foreach (var c in root.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);

            EditorSceneManager.MarkSceneDirty(hub.scene);
            EditorSceneManager.SaveScene(hub.scene);
            Debug.Log("[Itan] Hub dressed.");
        }

        // ---------------- Surfaces ----------------

        static void Surfaces(Transform hub)
        {
            Paint(hub.Find("Ground"), TexMat("Hub_Earth", "red_dirt_mud_01", 12f, new Color(0.9f, 0.74f, 0.64f)));
            Paint(hub.Find("Landmarks/Courtyard"), TexMat("Hub_Courtyard", "red_laterite_soil_stones", 5f, new Color(1f, 0.88f, 0.8f)));
            var walls = hub.Find("CompoundWalls");
            var clay = TexMat("Hub_ClayWall", "clay_plaster", 1f, new Color(0.96f, 0.72f, 0.54f), new Vector2(2.2f, 1.3f));
            foreach (Transform w in walls)
                Paint(w, w.name.StartsWith("Thatch") ? Thatch() : clay);
            var stand = StoryScenes.Find("CalabashStand");
            if (stand != null) Paint(stand.transform, TexMat("Story_CarvedWood", "palm_bark", 1f, new Color(0.62f, 0.4f, 0.24f), new Vector2(2f, 1f)));
        }

        static void Paint(Transform t, Material m)
        {
            if (t == null) return;
            var r = t.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = m;
        }

        // ---------------- The elder's tree ----------------

        static void Tree(Transform root)
        {
            var spot = StoryScenes.Find("ElderTree").transform;
            foreach (Transform c in spot) c.gameObject.SetActive(false); // the old cylinder and spheres
            var tree = Sized("CommonTree_3", root, spot.position, 200f, 10f);
            if (tree != null) tree.name = "ElderTree_Model";
        }

        // ---------------- Fire ----------------

        static void Fire(Transform root)
        {
            var pit = StoryScenes.Find("FirePit").transform;
            foreach (Transform c in pit)
                if (c.name.StartsWith("Log_") || c.name.StartsWith("Stone_")) c.gameObject.SetActive(false);

            var fire = Group("Fire", root, pit.position);
            // Three logs leaning into a cone over the embers.
            var bark = TexMat("Hub_Bark", "palm_bark", 1f, new Color(0.55f, 0.4f, 0.3f), new Vector2(1f, 2f));
            for (int i = 0; i < 4; i++)
            {
                var dir = Quaternion.Euler(0f, i * 90f + 20f, 0f) * Vector3.forward;
                var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
                log.name = "Log_" + i; log.SetParent(fire, false);
                log.position = pit.position + dir * 0.17f + Vector3.up * 0.12f;
                log.up = (-dir * 0.85f + Vector3.up * 0.55f).normalized; // leaning in, about 33 degrees off the ground
                log.localScale = new Vector3(0.09f, 0.26f, 0.09f);
                log.GetComponent<Renderer>().sharedMaterial = bark;
            }
            // Ring of stones.
            for (int i = 0; i < 11; i++)
            {
                float a = i * Mathf.PI * 2f / 11f;
                Sized(i % 2 == 0 ? "Pebble_Round_1" : "Pebble_Round_2", fire, pit.position + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.55f, Rand(0f, 360f), Rand(0.22f, 0.3f));
            }

            Flames(fire, pit.position);

            var light = pit.Find("FireLight");
            if (light != null && light.GetComponent<FireFlicker>() == null) light.gameObject.AddComponent<FireFlicker>();
        }

        static void Flames(Transform parent, Vector3 pos)
        {
            var add = AdditiveMat();

            var flames = NewParticles("Flames", parent, pos + Vector3.up * 0.12f, add);
            var main = flames.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.22f, 0.4f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.maxParticles = 60;
            var em = flames.emission; em.rateOverTime = 34f;
            var shape = flames.shape; shape.shapeType = ParticleSystemShapeType.Cone; shape.radius = 0.16f; shape.angle = 6f;
            var col = flames.colorOverLifetime; col.enabled = true;
            col.color = Gradient(new[] { new Color(1f, 0.85f, 0.4f), new Color(1f, 0.45f, 0.1f), new Color(0.6f, 0.12f, 0.04f) }, new[] { 0f, 0.9f, 0f });
            var size = flames.sizeOverLifetime; size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.7f), new Keyframe(0.3f, 1f), new Keyframe(1f, 0.2f)));

            var embers = NewParticles("Embers", parent, pos + Vector3.up * 0.2f, add);
            var em2 = embers.main;
            em2.startLifetime = new ParticleSystem.MinMaxCurve(1.4f, 2.6f);
            em2.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);
            em2.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.03f);
            em2.maxParticles = 40;
            var e2 = embers.emission; e2.rateOverTime = 7f;
            var s2 = embers.shape; s2.shapeType = ParticleSystemShapeType.Cone; s2.radius = 0.15f; s2.angle = 20f;
            var n2 = embers.noise; n2.enabled = true; n2.strength = 0.35f; n2.frequency = 0.8f;
            var c2 = embers.colorOverLifetime; c2.enabled = true;
            c2.color = Gradient(new[] { new Color(1f, 0.8f, 0.4f), new Color(1f, 0.4f, 0.1f), new Color(0.8f, 0.2f, 0.05f) }, new[] { 1f, 0.9f, 0f });
        }

        static ParticleSystem NewParticles(string name, Transform parent, Vector3 pos, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main; main.simulationSpace = ParticleSystemSimulationSpace.World; main.playOnAwake = true; main.loop = true;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return ps;
        }

        static Gradient Gradient(Color[] colours, float[] alphas)
        {
            var g = new Gradient();
            var ck = new GradientColorKey[colours.Length];
            var ak = new GradientAlphaKey[alphas.Length];
            for (int i = 0; i < colours.Length; i++) ck[i] = new GradientColorKey(colours[i], i / (float)(colours.Length - 1));
            for (int i = 0; i < alphas.Length; i++) ak[i] = new GradientAlphaKey(alphas[i], i / (float)(alphas.Length - 1));
            g.SetKeys(ck, ak);
            return g;
        }

        static Material AdditiveMat()
        {
            var path = VfxDir + "/Fire_Additive.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                var urp = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/ParticlesUnlit.mat");
                mat = urp != null ? new Material(urp) : new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(VfxDir + "/Smoke_Soft.png"));
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 2f); // additive
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_ZWrite", 0f);
            mat.renderQueue = 3000;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        // ---------------- Seats ----------------

        static void Seats(Transform root)
        {
            var wood = TexMat("Story_CarvedWood", "palm_bark", 1f, new Color(0.62f, 0.4f, 0.24f), new Vector2(2f, 1f));
            var mat = Flat("Hub_WovenMat", new Color(0.8f, 0.62f, 0.38f));
            var seating = StoryScenes.Find("Seating").transform;
            foreach (var t in seating.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "Stool")
                {
                    t.gameObject.SetActive(false);
                    // The visitor is sitting on their own seat, so that one is simply hidden; everyone else gets a carved stool.
                    if (t.parent != null && t.parent.name != "PlayerSpot")
                        MeshObject("CarvedStool", root, ProcMesh.CarvedStool(), wood, t.position - Vector3.up * t.position.y, Rand(0f, 360f), 1.15f);
                }
                else if (t.name == "Mat") Paint(t, mat);
            }
        }

        // ---------------- Everyday things ----------------

        static void Props(Transform root)
        {
            var props = Group("YardThings", root, Vector3.zero);
            var wood = TexMat("Story_CarvedWood", "palm_bark", 1f, new Color(0.62f, 0.4f, 0.24f), new Vector2(2f, 1f));
            var basket = TexMat("Hub_Basket", "thatch_roof_angled", 1f, new Color(1f, 0.85f, 0.6f), new Vector2(4f, 1f));
            var drumSkin = Flat("Hub_DrumSkin", new Color(0.86f, 0.74f, 0.56f));

            // Water pots by the left wall.
            foreach (var p in new[] { new Vector3(-5.6f, 0f, 1.6f), new Vector3(-5.9f, 0f, 2.2f), new Vector3(-5.3f, 0f, 2.5f), new Vector3(-6.0f, 0f, 1.0f) })
                MeshObject("ClayPot", props, ProcMesh.ClayPot(), Terracotta(), p, Rand(0f, 360f), Rand(1.2f, 1.8f));

            // Mortar and pestle for pounding yam, by the right wall.
            var mortar = MeshObject("Mortar", props, ProcMesh.Mortar(), wood, new Vector3(4.9f, 0f, 3.6f), 0f, 1.2f);
            var pestle = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            pestle.name = "Pestle"; pestle.SetParent(mortar, false);
            pestle.localPosition = new Vector3(0.05f, 0.75f, 0f);
            pestle.localRotation = Quaternion.Euler(0f, 0f, 12f);
            pestle.localScale = new Vector3(0.055f, 0.55f, 0.055f);
            pestle.GetComponent<Renderer>().sharedMaterial = wood;

            // A talking drum lying near the elder, and a second standing drum.
            var drum = MeshObject("TalkingDrum", props, ProcMesh.TalkingDrum(), wood, new Vector3(1.05f, 0.13f, 3.25f), 0f, 1f);
            drum.rotation = Quaternion.Euler(0f, 70f, 90f);
            MeshObject("StandingDrum", props, ProcMesh.TalkingDrum(), wood, new Vector3(-1.1f, 0f, 3.7f), 0f, 1.4f);

            // Baskets and calabash bowls with a little food: the compound has enough.
            foreach (var p in new[] { new Vector3(4.6f, 0f, 1.2f), new Vector3(5.2f, 0f, 0.7f) })
                MeshObject("Basket", props, ProcMesh.Basket(), basket, p, Rand(0f, 360f), Rand(1f, 1.3f));
            foreach (var p in new[] { new Vector3(4.6f, 0f, 1.2f), new Vector3(5.2f, 0f, 0.7f) })
                for (int i = 0; i < 3; i++)
                    Sized(i % 2 == 0 ? "Tomato" : "Pepper_Green", props, p + new Vector3(Rand(-0.08f, 0.08f), 0.2f + i * 0.04f, Rand(-0.08f, 0.08f)), Rand(0f, 360f), 0.09f);
            foreach (var p in new[] { new Vector3(-0.9f, 0f, 2.9f), new Vector3(0.75f, 0f, 2.95f) })
                MeshObject("CalabashBowl", props, ProcMesh.CalabashBowl(), CalabashMat(), p, Rand(0f, 360f), 1.1f);

            // Firewood stacked by the left wall.
            var barkWood = TexMat("Hub_Bark", "palm_bark", 1f, new Color(0.55f, 0.4f, 0.3f), new Vector2(1f, 2f));
            for (int i = 0; i < 7; i++)
            {
                int row = i < 4 ? 0 : 1;
                var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
                log.name = "Firewood_" + i; log.SetParent(props, false);
                log.position = new Vector3(-6.2f, 0.06f + row * 0.1f, -0.9f + (row == 0 ? i : i - 4 + 0.5f) * 0.11f);
                log.rotation = Quaternion.Euler(0f, 0f, 90f);
                log.localScale = new Vector3(0.1f, 0.45f, 0.1f);
                log.GetComponent<Renderer>().sharedMaterial = barkWood;
            }

            // Adire cloth drying on a line across the back right corner.
            AdireLine(props, new Vector3(2.6f, 1.75f, 7.9f), new Vector3(5.1f, 1.75f, 6.1f));
        }

        static void AdireLine(Transform parent, Vector3 a, Vector3 b)
        {
            var line = Group("AdireLine", parent, (a + b) * 0.5f);
            var rope = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            rope.name = "Line"; rope.SetParent(line, false);
            rope.position = (a + b) * 0.5f;
            rope.up = (b - a).normalized;
            rope.localScale = new Vector3(0.012f, (b - a).magnitude * 0.5f, 0.012f);
            rope.GetComponent<Renderer>().sharedMaterial = Flat("Hub_Line", new Color(0.55f, 0.45f, 0.3f));

            var adire = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            adire.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/UI/Adire_Tile.png"));
            adire.SetTextureScale("_BaseMap", new Vector2(2f, 3f));
            adire.SetFloat("_Cull", 0f); // both sides of the cloth
            adire.SetFloat("_Smoothness", 0.05f);
            var path = "Assets/_Project/Art/Materials/Story/Hub_AdireCloth.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing == null) AssetDatabase.CreateAsset(adire, path); else { existing.CopyPropertiesFromMaterial(adire); adire = existing; }

            for (int i = 0; i < 3; i++)
            {
                var cloth = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                cloth.name = "Cloth_" + i; cloth.SetParent(line, false);
                var p = Vector3.Lerp(a, b, 0.2f + i * 0.3f);
                cloth.position = p - Vector3.up * 0.45f;
                cloth.rotation = Quaternion.LookRotation(Vector3.Cross(b - a, Vector3.up)) * Quaternion.Euler(0f, 0f, Rand(-3f, 3f));
                cloth.localScale = new Vector3(0.7f, 0.9f, 1f);
                cloth.GetComponent<Renderer>().sharedMaterial = adire;
            }
        }

        static void Plants(Transform root)
        {
            var plants = Group("Plants", root, Vector3.zero);
            string[] kinds = { "Bush_Common", "Plant_1", "Fern_1", "Plant_7" };
            var spots = new List<Vector3>();
            for (int i = 0; i < 14; i++)
            {
                float a = Rand(-150f, 150f) * Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Sin(a) * 6.1f, 0f, 2f + Mathf.Cos(a) * 6.1f);
                if (p.z < -1.5f) continue;
                Sized(kinds[i % kinds.Length], plants, p, Rand(0f, 360f), Rand(0.45f, 0.9f));
            }
        }
    }
}
