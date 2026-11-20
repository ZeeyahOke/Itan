using UnityEditor;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>Small procedural meshes for things no asset pack has: clay pots, calabashes, round huts.</summary>
    public static class ProcMesh
    {
        const string Dir = "Assets/_Project/Art/Meshes/Story";

        /// <summary>
        /// Spin a profile (x = radius, y = height) around the Y axis. U wraps around,
        /// V runs up the profile, so tiling textures stay straight.
        /// </summary>
        public static Mesh Lathe(string name, Vector2[] profile, int segments = 24, float uvScale = 1f)
        {
            var path = $"{Dir}/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;

            int rings = profile.Length;
            var verts = new Vector3[(segments + 1) * rings];
            var uvs = new Vector2[verts.Length];
            float length = 0f;
            var along = new float[rings];
            for (int i = 1; i < rings; i++) { length += Vector2.Distance(profile[i], profile[i - 1]); along[i] = length; }

            for (int s = 0; s <= segments; s++)
            {
                float a = s * Mathf.PI * 2f / segments;
                for (int i = 0; i < rings; i++)
                {
                    var p = profile[i];
                    verts[s * rings + i] = new Vector3(Mathf.Cos(a) * p.x, p.y, Mathf.Sin(a) * p.x);
                    uvs[s * rings + i] = new Vector2((float)s / segments * uvScale * Mathf.Max(1f, Mathf.Round(p.x * 4f)), along[i] * uvScale);
                }
            }
            var tris = new int[segments * (rings - 1) * 6];
            int t = 0;
            for (int s = 0; s < segments; s++)
                for (int i = 0; i < rings - 1; i++)
                {
                    int a = s * rings + i, b = (s + 1) * rings + i;
                    tris[t++] = a; tris[t++] = a + 1; tris[t++] = b;
                    tris[t++] = b; tris[t++] = a + 1; tris[t++] = b + 1;
                }
            var mesh = new Mesh { name = name, vertices = verts, uv = uvs, triangles = tris };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Art/Meshes")) AssetDatabase.CreateFolder("Assets/_Project/Art", "Meshes");
            if (!AssetDatabase.IsValidFolder(Dir)) AssetDatabase.CreateFolder("Assets/_Project/Art/Meshes", "Story");
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        /// <summary>A clay water pot, about 0.4 m tall, open at the top.</summary>
        public static Mesh ClayPot() => Lathe("ClayPot", new[]
        {
            new Vector2(0f, 0f), new Vector2(0.1f, 0.005f), new Vector2(0.19f, 0.06f), new Vector2(0.23f, 0.16f),
            new Vector2(0.21f, 0.26f), new Vector2(0.14f, 0.33f), new Vector2(0.11f, 0.36f), new Vector2(0.13f, 0.39f),
            new Vector2(0.12f, 0.4f), new Vector2(0.1f, 0.37f), new Vector2(0.1f, 0.3f),
        });

        /// <summary>Half a calabash gourd: a shallow serving bowl.</summary>
        public static Mesh CalabashBowl()
        {
            var p = new Vector2[10];
            for (int i = 0; i < 9; i++)
            {
                float a = Mathf.Lerp(-Mathf.PI / 2f, 0f, i / 8f);
                p[i] = new Vector2(Mathf.Cos(a) * 0.2f, 0.12f + Mathf.Sin(a) * 0.12f);
            }
            p[9] = new Vector2(0.185f, 0.115f); // thin lip turning inward
            return Lathe("CalabashBowl", p);
        }

        /// <summary>A carved round Yoruba stool, about 0.33 m tall: wide seat, narrow waist, round foot.</summary>
        public static Mesh CarvedStool() => Lathe("CarvedStool", new[]
        {
            new Vector2(0f, 0f), new Vector2(0.16f, 0f), new Vector2(0.16f, 0.03f), new Vector2(0.09f, 0.08f), new Vector2(0.065f, 0.16f),
            new Vector2(0.09f, 0.25f), new Vector2(0.18f, 0.29f), new Vector2(0.185f, 0.32f), new Vector2(0.16f, 0.335f), new Vector2(0f, 0.335f),
        }, 24, 1f);

        /// <summary>Round mud wall of a hut, 1 m radius and 1 m tall (scale to size).</summary>
        public static Mesh HutWall() => Lathe("HutWall", new[]
        {
            new Vector2(1.04f, 0f), new Vector2(1f, 0.08f), new Vector2(1f, 0.92f), new Vector2(1.02f, 1f), new Vector2(0.9f, 1f),
        }, 32, 2f);

        /// <summary>Conical thatched roof for a 1 m hut, with an overhang and a little topknot.</summary>
        public static Mesh HutRoof() => Lathe("HutRoof", new[]
        {
            new Vector2(1.38f, 0.82f), new Vector2(1.36f, 0.9f), new Vector2(0.8f, 1.45f), new Vector2(0.25f, 1.98f),
            new Vector2(0.12f, 2.1f), new Vector2(0.14f, 2.22f), new Vector2(0f, 2.3f),
        }, 32, 2f);
    }
}
