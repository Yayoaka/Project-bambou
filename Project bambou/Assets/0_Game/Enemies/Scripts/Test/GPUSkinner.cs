using System.Runtime.InteropServices;
using UnityEngine;

public class GPU_Skinner_Anim : MonoBehaviour
{
    [System.Serializable]
    struct VertexData
    {
        public Vector3 pos;
        public Vector3 nrm;
        public Vector4 weights;
        public Vector4 indices;
    }

    public SkinnedMeshRenderer skinned;   // Le mesh original
    public Texture2D animTexture;         // AnimTex RGBAHalf
    public ComputeShader compute;         // Skinning.compute
    public Material material;             // Material Custom/GPUSkinnedLit

    public float clipLength = 1.2f;       // durée de ton animation
    public int boneCount;                 // auto set
    public int frameCount;                // auto set

    Mesh _mesh;
    ComputeBuffer _vbufIn;
    ComputeBuffer _vbufPos;
    ComputeBuffer _vbufNrm;

    int _kernel;
    int _vertexCount;
    float _t;

    void Start()
    {
        // --- 1) Préparation du mesh ---
        if (skinned == null)
        {
            Debug.LogError("Pas de SkinnedMeshRenderer assigné.");
            return;
        }

        Mesh src = skinned.sharedMesh;
        boneCount = src.bindposes.Length;

        _mesh = Instantiate(src);
        _vertexCount = _mesh.vertexCount;

        // --- 2) Construction du tableau d’entrée (positions, normales, weights, indices) ---
        var verts = src.vertices;
        var norms = src.normals;
        var bw = src.boneWeights;

        VertexData[] input = new VertexData[_vertexCount];

        for (int i = 0; i < _vertexCount; i++)
        {
            input[i] = new VertexData()
            {
                pos = verts[i],
                nrm = norms[i],
                weights = new Vector4(bw[i].weight0, bw[i].weight1, bw[i].weight2, bw[i].weight3),
                indices = new Vector4(bw[i].boneIndex0, bw[i].boneIndex1, bw[i].boneIndex2, bw[i].boneIndex3)
            };
        }

        // --- 3) Compute Buffers ---
        int stride = Marshal.SizeOf(typeof(VertexData));
        _vbufIn  = new ComputeBuffer(_vertexCount, stride);
        _vbufPos = new ComputeBuffer(_vertexCount, sizeof(float) * 3);
        _vbufNrm = new ComputeBuffer(_vertexCount, sizeof(float) * 3);

        _vbufIn.SetData(input);

        // --- 4) Initialisation compute shader ---
        _kernel = compute.FindKernel("Skin");

        compute.SetBuffer(_kernel, "_InVertices", _vbufIn);
        compute.SetBuffer(_kernel, "_OutPositions", _vbufPos);
        compute.SetBuffer(_kernel, "_OutNormals", _vbufNrm);

        compute.SetTexture(_kernel, "_AnimTex", animTexture);

        // Dimensions de l’AnimTex
        frameCount = animTexture.height;
        compute.SetInt("_FrameCount", frameCount);
        compute.SetInt("_BoneCount", boneCount);
        compute.SetInt("_Count", _vertexCount);

        // --- 5) Création MeshFilter + MeshRenderer ---
        MeshFilter mf = gameObject.AddComponent<MeshFilter>();
        mf.sharedMesh = _mesh;

        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.sharedMaterial = material;

        // --- 6) ⚠ IMPORTANT : assignation buffers AU MATERIAL ---
        material.SetBuffer("_SkinnedPositions", _vbufPos);
        material.SetBuffer("_SkinnedNormals", _vbufNrm);

        Debug.Log("Buffers assignés au Material");
    }

    void Update()
    {
        if (_vbufPos == null) return;

        // Temps d’anim
        _t += Time.deltaTime;
        float animT = (_t % clipLength) / clipLength;

        compute.SetFloat("_AnimTime", animT);

        // Dispatch du compute
        int groups = Mathf.CeilToInt(_vertexCount / 64f);
        compute.Dispatch(_kernel, groups, 1, 1);

        // Récup temp CPU (phase 1)
        Vector3[] pos = new Vector3[_vertexCount];
        Vector3[] nrm = new Vector3[_vertexCount];

        _vbufPos.GetData(pos);
        _vbufNrm.GetData(nrm);

        _mesh.SetVertices(pos);
        _mesh.SetNormals(nrm);
    }

    void OnDestroy()
    {
        _vbufIn?.Dispose();
        _vbufPos?.Dispose();
        _vbufNrm?.Dispose();
    }
}
