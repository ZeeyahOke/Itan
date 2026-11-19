using UnityEditor;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>Shared helper: get or create a flat-colour URP greybox material.</summary>
    public static class GreyboxMaterials
    {
        const string Folder = "Assets/_Project/Art/Materials";

        public static Material Get(string name, Color color, bool unlit = false, float smoothness = 0.1f)
        {
            string path = Folder + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            mat = new Material(Shader.Find(unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", color);
            if (!unlit) mat.SetFloat("_Smoothness", smoothness);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        /// <summary>Primitive with a material, parented, no collider unless asked.</summary>
        public static Transform Part(string name, PrimitiveType type, Transform parent, Vector3 localPos,
                                     Vector3 localScale, Material mat, Vector3 localEuler = default, bool keepCollider = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localEulerAngles = localEuler;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go.transform;
        }

        public static Transform Group(string name, Transform parent, Vector3 localPos)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = localPos;
            return t;
        }
    }
}
