using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Turns the greybox story world into a storybook place with real assets:
    /// a famine-struck Yoruba village of round mud huts under dead trees, the
    /// rope spot, and Ajá's mother's home on a cloud with a feast laid out.
    /// Everything it adds lives under StoryWorld/Dressing, so it can be rebuilt.
    /// Menu: Itan > Story > Dress Story World
    /// </summary>
    public static class StoryWorldDresser
    {
        const string MatDir = "Assets/_Project/Art/Materials/Story";
        const string PH = "Assets/_ThirdParty/PolyHaven";

        // Layout (the visitor sits at the origin, facing +Z).
        public static readonly Vector3 CloudCentre = new Vector3(0f, 4.62f, 9.6f);
        public const float CloudTop = 5.05f;
        /// <summary>Where the visitor ends up after climbing: at the top of the rope, facing the feast.</summary>
        public static readonly Vector3 SkyViewpoint = new Vector3(0f, CloudTop, 7.4f);
        public static readonly Vector3 FeastCentre = new Vector3(0f, CloudTop, 9.5f);
        public static readonly Vector3 MotherSeat = new Vector3(1.3f, CloudTop, 10.1f);
        public static readonly Vector3 AjaSeat = new Vector3(-1.3f, CloudTop, 9.9f);
        /// <summary>Where Ìjàpá crouches, hidden from Ajá but in view of the visitor.</summary>
        public static readonly Vector3 IjapaHide = new Vector3(-2.7f, 0f, 4.6f);

        static System.Random rng = new System.Random(11);

        public static void Seed(int seed) => rng = new System.Random(seed);

        [MenuItem("Itan/Story/Dress Story World")]
        public static void Dress()
        {
            StoryScenes.OpenStory();
            var world = StoryScenes.Find("StoryWorld");
            if (world == null) { Debug.LogError("[Itan] No StoryWorld found."); return; }
            rng = new System.Random(11);

            // Clear old greybox scenery and any earlier dressing.
            foreach (var name in new[] { "Ground", "FamineLand", "RopeSpot", "SkyHome", "Dressing" })
            {
                var old = world.transform.Find(name);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }

            var root = new GameObject("Dressing").transform;
            root.SetParent(world.transform, false);

            BuildGround(root);
            BuildVillage(root);
            BuildDeadLand(root);
            BuildRopeSpot(root);
            BuildSkyHome(root);
            RestyleRope(world.transform);

            // Scenery never moves: let Unity batch it.
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
            foreach (var c in root.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
            // The stones round the foot of the rope are solid, so Ìjàpá lands on them rather than in them.
            var spot = root.Find("RopeSpot");
            if (spot != null)
            {
                foreach (var mf in spot.GetComponentsInChildren<MeshFilter>(true))
                    mf.gameObject.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;
                // A flat box, not a primitive: a squashed cylinder keeps a capsule collider that balloons into a sphere.
                var floor = new GameObject("GroundCollider");
                floor.transform.SetParent(spot, false);
                floor.transform.position = new Vector3(spot.position.x, -0.05f, spot.position.z);
                floor.AddComponent<BoxCollider>().size = new Vector3(6f, 0.1f, 6f);
            }

            StoryScenes.SaveAll();
            // Moments point at markers inside the dressing (e.g. the sky viewpoint), so re-link them.
            if (Object.FindFirstObjectByType<Itan.World.FeastMoment>(FindObjectsInactive.Include) != null)
                StoryMomentsWiring.Wire();
            Debug.Log("[Itan] Story world dressed.");
        }

        // ---------------- Ground ----------------

        static void BuildGround(Transform root)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ground.name = "Ground";
            ground.transform.SetParent(root, false);
            ground.transform.localPosition = new Vector3(0f, -0.02f, 6f);
            ground.transform.localScale = new Vector3(70f, 0.02f, 70f);
            ground.GetComponent<Renderer>().sharedMaterial = TexMat("Story_CrackedEarth", "mud_cracked_dry_03", 22f, new Color(0.95f, 0.78f, 0.62f));

        }

        // ---------------- Village of round huts ----------------

        static void BuildVillage(Transform root)
        {
            var village = Group("Village", root, Vector3.zero);
            var huts = new (Vector3 pos, float r)[]
            {
                (new Vector3(-8.5f, 0f, 12.5f), 1.6f), (new Vector3(-4.6f, 0f, 16f), 1.9f), (new Vector3(2.2f, 0f, 17.5f), 2.1f),
                (new Vector3(7.4f, 0f, 14.2f), 1.7f), (new Vector3(10.6f, 0f, 9.4f), 1.5f), (new Vector3(-11.5f, 0f, 6.5f), 1.6f),
                (new Vector3(-9.8f, 0f, -4f), 1.8f), (new Vector3(9.5f, 0f, -3.2f), 1.6f),
            };
            for (int i = 0; i < huts.Length; i++)
            {
                var (pos, r) = huts[i];
                var hut = Hut("Hut_" + i, village, pos, r, FaceTowards(pos, new Vector3(0f, 0f, 6f)));
                // Empty clay pots, some knocked over: nothing to eat.
                for (int k = 0; k < 2; k++)
                {
                    var off = Quaternion.Euler(0f, Rand(-60f, 60f), 0f) * hut.forward * (r + 0.5f) + hut.right * Rand(-0.6f, 0.6f);
                    var pot = MeshObject("EmptyPot", village, ProcMesh.ClayPot(), Terracotta(), pos + off, Rand(0f, 360f), Rand(0.8f, 1.3f));
                    if (k == 1) pot.localRotation *= Quaternion.Euler(Rand(70f, 95f), 0f, 0f);
                    else pot.localPosition += Vector3.up * 0.0f;
                }
            }
        }

        public static Transform Hut(string name, Transform parent, Vector3 pos, float radius, float yaw)
        {
            var hut = Group(name, parent, pos);
            hut.localRotation = Quaternion.Euler(0f, yaw, 0f);
            float height = radius * 1.05f;
            var wall = MeshObject("Wall", hut, ProcMesh.HutWall(), Clay(), Vector3.zero, 0f, 1f);
            wall.localScale = new Vector3(radius, height, radius);
            var roof = MeshObject("Roof", hut, ProcMesh.HutRoof(), Thatch(), Vector3.zero, 0f, 1f);
            roof.localScale = new Vector3(radius, height * 0.95f, radius);
            // Dark doorway facing the open ground.
            var door = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
            door.name = "Doorway";
            door.SetParent(hut, false);
            door.localPosition = new Vector3(0f, height * 0.36f, radius * 0.99f);
            door.localScale = new Vector3(radius * 0.5f, height * 0.72f, 0.06f);
            door.GetComponent<Renderer>().sharedMaterial = Flat("Story_Doorway", new Color(0.09f, 0.06f, 0.05f));
            return hut;
        }

        // ---------------- Dead land ----------------

        static void BuildDeadLand(Transform root)
        {
            var land = Group("DeadLand", root, Vector3.zero);
            string[] trees = { "DeadTree_1", "DeadTree_2", "DeadTree_3", "DeadTree_4", "DeadTree_5" };
            // Kept out from under the cloud and off the line to the rope, so they frame the view.
            var spots = new[]
            {
                new Vector3(-5.6f, 0f, 8.4f), new Vector3(5.8f, 0f, 7.6f), new Vector3(-7.4f, 0f, 3.4f), new Vector3(7.2f, 0f, 2.6f),
                new Vector3(-12f, 0f, 15f), new Vector3(12.5f, 0f, 15.5f), new Vector3(-3.5f, 0f, 21f), new Vector3(4.5f, 0f, 22f),
                new Vector3(-15f, 0f, 2f), new Vector3(15f, 0f, 4f), new Vector3(-6f, 0f, -7f), new Vector3(6f, 0f, -8f),
            };
            for (int i = 0; i < spots.Length; i++)
                Place(trees[i % trees.Length], land, spots[i], Rand(0f, 360f), Rand(0.45f, 0.65f));

            // Rocks and pebbles.
            string[] rocks = { "Rock_Medium_1", "Rock_Medium_2", "Rock_Medium_3" };
            for (int i = 0; i < 16; i++)
            {
                var p = OnRing(Rand(4f, 16f), Rand(-110f, 110f));
                if (Mathf.Abs(p.x) < 2.5f && p.z > 4f && p.z < 13f) continue; // nothing in the middle of the stage
                Place(rocks[i % 3], land, p, Rand(0f, 360f), Rand(0.3f, 0.7f));
            }

            // Dry, wispy grass. A thick patch on the left is where Ìjàpá hides.
            var dry = DryGrass();
            for (int i = 0; i < 40; i++)
            {
                var p = OnRing(Rand(2.5f, 14f), Rand(-120f, 120f));
                if (Mathf.Abs(p.x) < 1.4f && p.z < 8f) continue; // keep the path and rope clear
                Place(i % 2 == 0 ? "Grass_Wispy_Short" : "Grass_Wispy_Tall", land, p, Rand(0f, 360f), Rand(0.9f, 1.4f), dry);
            }
            // Ìjàpá's long grass sits forward and to the left, well clear of Ajá's way to the rope.
            var hide = Group("LongGrass", land, IjapaHide + new Vector3(-0.25f, 0f, 0.5f));
            for (int i = 0; i < 16; i++)
                Place("Grass_Wispy_Tall", hide, hide.position + new Vector3(Rand(-0.9f, 0.9f), 0f, Rand(-0.25f, 0.45f)), Rand(0f, 360f), Rand(1.4f, 2.0f), dry);
        }

        // ---------------- Rope spot ----------------

        static void BuildRopeSpot(Transform root)
        {
            var spot = Group("RopeSpot", root, new Vector3(StoryWorldBuilder.RopeBottom.x, 0f, StoryWorldBuilder.RopeBottom.z));
            for (int i = 0; i < 9; i++)
            {
                float a = i * Mathf.PI * 2f / 9f;
                Place(i % 2 == 0 ? "Pebble_Round_1" : "Pebble_Round_3", spot, spot.position + new Vector3(Mathf.Cos(a) * 0.8f, 0f, Mathf.Sin(a) * 0.8f), Rand(0f, 360f), Rand(1.6f, 2.4f));
            }
        }

        // ---------------- Sky home ----------------

        static void BuildSkyHome(Transform root)
        {
            var home = Group("SkyHome", root, Vector3.zero);
            var cloud = CloudMat();

            // A wide, flat-topped island you can stand on, with puffs piled round the edge.
            Puff(home, "Floor", CloudCentre, new Vector3(9f, 0.86f, 7.2f), cloud);
            Puff(home, "Underside", CloudCentre + new Vector3(0f, -0.45f, 0.3f), new Vector3(7.2f, 1.2f, 5.6f), cloud);
            for (int i = 0; i < 22; i++)
            {
                float a = i * 360f / 22f + Rand(-6f, 6f);
                var dir = Quaternion.Euler(0f, a, 0f) * Vector3.forward;
                var p = CloudCentre + new Vector3(dir.x * 4.2f, Rand(-0.1f, 0.35f), dir.z * 3.4f);
                // Leave the near edge low so the visitor can see in when they arrive by the rope.
                bool front = dir.z < -0.55f && Mathf.Abs(dir.x) < 0.6f;
                var s = front ? new Vector3(2.2f, 0.6f, 1.6f) : new Vector3(Rand(2f, 3.4f), Rand(1.1f, 1.9f), Rand(1.8f, 2.8f));
                Puff(home, "Puff_" + i, p, s, cloud);
            }
            // The sea of cloud all around, only shown while the visitor is up here.
            var sea = MeshObject("CloudSea", home, ProcMesh.CloudSea(5.5f, CloudTop - 0.35f), cloud, new Vector3(CloudCentre.x, 0f, CloudCentre.z), 0f, 1f);
            sea.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            sea.gameObject.SetActive(false);

            // Puffs drifting below, seen from the ground.
            for (int i = 0; i < 9; i++)
                Puff(home, "Lower_" + i, CloudCentre + new Vector3(Rand(-4f, 4f), Rand(-1.1f, -0.6f), Rand(-2.5f, 3f)), new Vector3(Rand(1.6f, 2.8f), Rand(0.7f, 1.1f), Rand(1.4f, 2.2f)), cloud);

            // Ìyá's hut at the back of the cloud.
            var hut = Hut("MotherHut", home, CloudCentre + new Vector3(0.6f, CloudTop - CloudCentre.y, 2.3f), 1.25f, 180f);

            // The feast: a woven mat with pots, calabash bowls and food.
            var feast = Group("Feast", home, FeastCentre);
            var mat = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            mat.name = "Mat";
            mat.SetParent(feast, false);
            mat.localPosition = new Vector3(0f, 0.01f, 0f);
            mat.localScale = new Vector3(2.2f, 0.01f, 1.5f);
            mat.GetComponent<Renderer>().sharedMaterial = Flat("Story_WovenMat", new Color(0.86f, 0.7f, 0.44f));

            var stew = MeshObject("StewPot", feast, ProcMesh.ClayPot(), Terracotta(), new Vector3(0f, 0f, 0.15f), 0f, 1.25f);
            var stewTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            stewTop.name = "Stew";
            stewTop.SetParent(stew, false);
            stewTop.localPosition = new Vector3(0f, 0.355f, 0f);
            stewTop.localScale = new Vector3(0.2f, 0.005f, 0.2f);
            stewTop.GetComponent<Renderer>().sharedMaterial = Flat("Story_Stew", new Color(0.72f, 0.22f, 0.08f), 0.5f);

            var bowls = new[] { new Vector3(-0.55f, 0f, -0.05f), new Vector3(0.55f, 0f, -0.1f), new Vector3(-0.2f, 0f, -0.45f), new Vector3(0.3f, 0f, 0.5f) };
            foreach (var b in bowls) MeshObject("CalabashBowl", feast, ProcMesh.CalabashBowl(), CalabashMat(), b, Rand(0f, 360f), 1.2f);

            // Food in and around the bowls (Quaternius food, all CC0), sized to real life.
            FoodIn(feast, "Banana", bowls[0], 3, 0.2f);
            FoodIn(feast, "ChickenLeg", bowls[1], 3, 0.16f);
            FoodIn(feast, "Bread", bowls[2], 2, 0.2f);
            FoodIn(feast, "Tomato", bowls[3], 4, 0.08f);
            Sized("Pumpkin", feast, feast.position + new Vector3(0.85f, 0f, 0.35f), 30f, 0.32f);
            Sized("Eggplant", feast, feast.position + new Vector3(-0.85f, 0f, 0.4f), 70f, 0.22f);
            Sized("Pepper_Green", feast, feast.position + new Vector3(-0.7f, 0f, -0.5f), 10f, 0.1f);
            MeshObject("WaterPot", feast, ProcMesh.ClayPot(), Terracotta(), new Vector3(1.1f, 0f, -0.3f), 0f, 1.6f);

            // Carved stools for Ìyá and Ajá, set just behind where they sit.
            var wood = TexMat("Story_CarvedWood", "palm_bark", 1f, new Color(0.62f, 0.4f, 0.24f), new Vector2(2f, 1f));
            foreach (var seat in new[] { MotherSeat, AjaSeat })
            {
                var toFeast = FeastCentre - seat; toFeast.y = 0f;
                MeshObject("CarvedStool", home, ProcMesh.CarvedStool(), wood, seat - toFeast.normalized * 0.14f, 0f, 1f);
            }


            // Markers the moments use.
            var markers = Group("SkyMarkers", home, Vector3.zero);
            Marker("SkyViewpoint", markers, SkyViewpoint, FeastCentre);
            Marker("MotherSeat", markers, MotherSeat, FeastCentre);
            Marker("AjaSeat", markers, AjaSeat, FeastCentre);
        }

        static void FoodIn(Transform parent, string model, Vector3 bowl, int count, float size)
        {
            for (int i = 0; i < count; i++)
            {
                var off = new Vector3(Rand(-0.07f, 0.07f), 0.05f + i * 0.03f, Rand(-0.07f, 0.07f));
                Sized(model, parent, parent.position + bowl + off, Rand(0f, 360f), size);
            }
        }

        /// <summary>Place a model scaled so its longest side is <paramref name="size"/> metres, resting on its base.</summary>
        public static Transform Sized(string model, Transform parent, Vector3 worldPos, float yaw, float size)
        {
            var t = Place(model, parent, worldPos, yaw, 1f);
            if (t == null) return null;
            var b = Bounds(t);
            float longest = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
            if (longest > 0.0001f) t.localScale *= size / longest;
            b = Bounds(t);
            t.position += Vector3.up * (worldPos.y - b.min.y);
            return t;
        }

        public static Bounds Bounds(Transform t)
        {
            var rs = t.GetComponentsInChildren<Renderer>();
            if (rs.Length == 0) return new Bounds(t.position, Vector3.zero);
            var b = rs[0].bounds;
            foreach (var r in rs) b.Encapsulate(r.bounds);
            return b;
        }

        static void Puff(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            p.name = name;
            p.SetParent(parent, false);
            p.position = pos;
            p.localScale = scale;
            p.localRotation = Quaternion.Euler(0f, Rand(0f, 360f), 0f);
            var r = p.GetComponent<Renderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static void RestyleRope(Transform world)
        {
            var rope = world.Find("RopePivot/Rope");
            if (rope == null) return;
            rope.localScale = new Vector3(0.07f, rope.localScale.y, 0.07f);
            rope.GetComponent<Renderer>().sharedMaterial = TexMat("Story_Rope", "thatch_roof_angled", 1f, new Color(0.95f, 0.78f, 0.5f), new Vector2(0.4f, 14f));
        }

        // ---------------- Helpers ----------------

        public static float Rand(float a, float b) => a + (float)rng.NextDouble() * (b - a);

        static Vector3 OnRing(float radius, float degrees)
        {
            var d = Quaternion.Euler(0f, degrees, 0f) * Vector3.forward;
            return new Vector3(d.x * radius, 0f, 3f + d.z * radius);
        }

        static float FaceTowards(Vector3 from, Vector3 to)
        {
            var d = to - from;
            return Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
        }

        public static Transform Group(string name, Transform parent, Vector3 worldPos)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.position = worldPos;
            return t;
        }

        static void Marker(string name, Transform parent, Vector3 pos, Vector3 lookAt)
        {
            var t = Group(name, parent, pos);
            var d = lookAt - pos; d.y = 0f;
            t.rotation = Quaternion.LookRotation(d);
        }

        public static Transform MeshObject(string name, Transform parent, Mesh mesh, Material mat, Vector3 pos, float yaw, float scale)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale = Vector3.one * scale;
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go.transform;
        }

        static readonly Dictionary<string, GameObject> modelCache = new Dictionary<string, GameObject>();

        /// <summary>Find a model in Assets/_ThirdParty by its file name (without extension).</summary>
        public static GameObject Model(string fileName)
        {
            if (modelCache.TryGetValue(fileName, out var cached) && cached != null) return cached;
            var path = AssetDatabase.FindAssets(fileName + " t:Model", new[] { "Assets/_ThirdParty" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == fileName);
            var model = path == null ? null : AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null) Debug.LogWarning("[Itan] Model not found: " + fileName + " (run Itan > Assets > Download Third-Party Assets)");
            modelCache[fileName] = model;
            return model;
        }

        public static Transform Place(string model, Transform parent, Vector3 worldPos, float yaw, float scale, Material overrideMat = null)
        {
            var prefab = Model(model);
            if (prefab == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = worldPos;
            go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            go.transform.localScale *= scale;
            if (overrideMat != null)
                foreach (var r in go.GetComponentsInChildren<Renderer>())
                    r.sharedMaterials = Enumerable.Repeat(overrideMat, r.sharedMaterials.Length).ToArray();
            else FixCutouts(go);
            return go.transform;
        }

        // ---------------- Materials ----------------

        /// <summary>
        /// Leaves and grass come in with see-through textures but opaque materials (black blocks).
        /// Swap in a saved copy with alpha clipping for any material whose texture has alpha.
        /// </summary>
        static void FixCutouts(GameObject go)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    var m = mats[i];
                    if (m == null || !m.HasProperty("_BaseMap")) continue;
                    var tex = m.GetTexture("_BaseMap") as Texture2D;
                    if (tex == null || !GraphicsFormatUtilityHasAlpha(tex)) continue;
                    if (m.IsKeywordEnabled("_ALPHATEST_ON")) continue;
                    var path = $"{MatDir}/Cutout_{m.name.Replace("/", "_")}.mat";
                    var cut = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (cut == null)
                    {
                        cut = new Material(m);
                        cut.SetFloat("_AlphaClip", 1f);
                        cut.SetFloat("_Cutoff", 0.5f);
                        cut.EnableKeyword("_ALPHATEST_ON");
                        cut.SetFloat("_Cull", 0f); // leaves are single planes: show both sides
                        cut.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                        AssetDatabase.CreateAsset(cut, path);
                    }
                    mats[i] = cut; changed = true;
                }
                if (changed) r.sharedMaterials = mats;
            }
        }

        static bool GraphicsFormatUtilityHasAlpha(Texture2D tex)
        {
            var path = AssetDatabase.GetAssetPath(tex);
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            return imp != null ? imp.DoesSourceTextureHaveAlpha() : UnityEngine.Experimental.Rendering.GraphicsFormatUtility.HasAlphaChannel(tex.graphicsFormat);
        }

        static Material Save(string name, Material m)
        {
            var path = $"{MatDir}/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) { existing.CopyPropertiesFromMaterial(m); existing.shader = m.shader; EditorUtility.SetDirty(existing); return existing; }
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static Material Lit() => new Material(Shader.Find("Universal Render Pipeline/Lit"));

        public static Material Flat(string name, Color c, float smooth = 0.05f)
        {
            var m = Lit();
            m.SetColor("_BaseColor", c);
            m.SetFloat("_Smoothness", smooth);
            return Save(name, m);
        }

        public static Material TexMat(string name, string polyHavenId, float tiling, Color tint, Vector2? tilingXY = null)
        {
            var m = Lit();
            var diff = AssetDatabase.LoadAssetAtPath<Texture2D>($"{PH}/{polyHavenId}/{polyHavenId}_diff.jpg");
            var nor = AssetDatabase.LoadAssetAtPath<Texture2D>($"{PH}/{polyHavenId}/{polyHavenId}_nor.jpg");
            if (nor != null)
            {
                var imp = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(nor));
                if (imp.textureType != TextureImporterType.NormalMap) { imp.textureType = TextureImporterType.NormalMap; imp.SaveAndReimport(); }
                m.SetTexture("_BumpMap", nor);
                m.EnableKeyword("_NORMALMAP");
            }
            m.SetTexture("_BaseMap", diff);
            m.SetColor("_BaseColor", tint);
            m.SetFloat("_Smoothness", 0.05f);
            var tile = tilingXY ?? new Vector2(tiling, tiling);
            m.SetTextureScale("_BaseMap", tile);
            m.SetTextureScale("_BumpMap", tile);
            return Save(name, m);
        }

        public static Material Clay() => TexMat("Story_ClayWall", "clay_plaster", 1f, new Color(0.92f, 0.66f, 0.5f), new Vector2(3f, 1f));
        public static Material Thatch() => TexMat("Story_Thatch", "thatch_roof_angled", 1f, new Color(1f, 0.88f, 0.66f), new Vector2(3f, 1.2f));
        public static Material Terracotta() => TexMat("Story_Terracotta", "clay_plaster", 1f, new Color(0.86f, 0.46f, 0.3f), new Vector2(2f, 1f));
        public static Material CalabashMat() => Flat("Story_Calabash", new Color(0.78f, 0.56f, 0.28f), 0.25f);

        public static Material DryGrass()
        {
            var src = Model("Grass_Wispy_Tall");
            var m = Lit();
            var r = src != null ? src.GetComponentInChildren<Renderer>() : null;
            if (r != null && r.sharedMaterial != null) m.CopyPropertiesFromMaterial(r.sharedMaterial);
            m.SetColor("_BaseColor", new Color(0.86f, 0.68f, 0.4f));
            return Save("Story_DryGrass", m);
        }

        static Material CloudMat()
        {
            var m = Lit();
            m.SetColor("_BaseColor", new Color(0.98f, 0.94f, 0.95f));
            m.SetFloat("_Smoothness", 0f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", new Color(0.42f, 0.3f, 0.34f)); // glows softly in the sunset
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            return Save("Story_Cloud", m);
        }
    }
}
