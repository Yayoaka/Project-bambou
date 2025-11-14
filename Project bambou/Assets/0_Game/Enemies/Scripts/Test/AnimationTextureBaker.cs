using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class AnimationTextureBaker : MonoBehaviour
{
    public SkinnedMeshRenderer skinned;
    public AnimationClip clip;
    public int sampleRate = 30;

    [HideInInspector] public Texture2D animTex;
    [HideInInspector] public int boneCount;
    [HideInInspector] public int frameCount;

    [ContextMenu("Bake Animation")]
    public void Bake()
    {
        if (skinned == null || clip == null)
        {
            Debug.LogError("Missing skinned or clip");
            return;
        }

        boneCount = skinned.bones.Length;
        frameCount = Mathf.CeilToInt(clip.length * sampleRate);

        int width = boneCount * 4;
        int height = frameCount;

        animTex = new Texture2D(width, height, TextureFormat.RGBAHalf, false);
        animTex.filterMode = FilterMode.Point;
        animTex.wrapMode = TextureWrapMode.Clamp;

        // --- Playable Graph (NO Animator needed) ---
        var graph = PlayableGraph.Create("BakeGraph");
        var playable = AnimationClipPlayable.Create(graph, clip);
        playable.SetApplyFootIK(false);
        playable.SetApplyPlayableIK(false);

        graph.Play();

        // --- BAKE ---
        for (int f = 0; f < frameCount; f++)
        {
            float t = f / (float)sampleRate;
            playable.SetTime(t);
            graph.Evaluate(); // Force update bones

            for (int b = 0; b < boneCount; b++)
            {
                var bone = skinned.bones[b];

                Matrix4x4 m =
                    bone.localToWorldMatrix *
                    skinned.rootBone.worldToLocalMatrix;

                int x = b * 4;

                animTex.SetPixel(x + 0, f, new Color(m.m00, m.m01, m.m02, m.m03));
                animTex.SetPixel(x + 1, f, new Color(m.m10, m.m11, m.m12, m.m13));
                animTex.SetPixel(x + 2, f, new Color(m.m20, m.m21, m.m22, m.m23));
                animTex.SetPixel(x + 3, f, new Color(m.m30, m.m31, m.m32, m.m33));
            }
        }

        animTex.Apply();
        graph.Destroy();
        
#if UNITY_EDITOR
        string path = "Assets/AnimTex_" + clip.name + ".asset";
        AssetDatabase.CreateAsset(animTex, path);
        AssetDatabase.SaveAssets();
        Debug.Log("Saved anim texture at: " + path);
#endif

        Debug.Log("Bake Done: frames=" + frameCount + " bones=" + boneCount);
    }
}
