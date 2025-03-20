using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [Range(0, 4)] [SerializeField] private int LOD;
    [SerializeField] private NoiseGenerator noiseGenerator;
    [SerializeField] private ComputeShader marchingShader;
    private ComputeBuffer trianglesBuffer;
    private ComputeBuffer trianglesCountBuffer;
    private ComputeBuffer weightsBuffer;
    private Mesh mesh;

    public MeshFilter meshFilter;
    public MeshCollider meshCollider;
    
    private float[] weights;
    private float[] initialWeights;

    struct Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;
        
        public static int SizeOf => sizeof(float) *3*3;
    }

    private void OnValidate()
    {
        Create();
    }

    private void Start()
    {
        noiseGenerator = ChunkGenerator.Instance.noiseGenerator;
        noiseGenerator.chunks.Add(this);
        Create();
    }

    public void Create()
    {
        CreateBuffers();
        mesh = new Mesh();
        if (weights == null)
        {
            weights = noiseGenerator.GetNoise(GridMetrics.LastLOD, transform.position);
            
        }
        initialWeights = new float[weights.Length];
        Array.Copy(weights,initialWeights,weights.Length);
        UpdateMesh();
        ReleaseBuffers();
    }

    private void SetStartingArea()
    {
        int kernelID = marchingShader.FindKernel("SetStart");
        
        weightsBuffer.SetData(weights);
        marchingShader.SetBuffer(kernelID,"_Weights",weightsBuffer);
        
        marchingShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk(LOD));
        marchingShader.SetVector("_StartPosition", new Vector3(32,5,32));
        marchingShader.SetFloat("_Radius", 4);
        
        marchingShader.Dispatch(kernelID, GridMetrics.ThreadGroups(LOD),GridMetrics.ThreadGroups(LOD),GridMetrics.ThreadGroups(LOD));
        weightsBuffer.GetData(weights);
        
        UpdateMesh();
    }

    public void ResetTerrain()
    {
        Array.Copy(initialWeights,weights,weights.Length);
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        CreateMesh();
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public void EditWeights(Vector3 hitPosition, float brushSize, bool add, float terraformingStrength)
    {
        CreateBuffers();
        int kernelID = marchingShader.FindKernel("UpdateWeights");
        
        weightsBuffer.SetData(weights);
        marchingShader.SetBuffer(kernelID,"_Weights",weightsBuffer);
        
        marchingShader.SetInt("_ChunkSize", GridMetrics.PointsPerChunk(GridMetrics.LastLOD));
        marchingShader.SetVector("_HitPosition", hitPosition);
        marchingShader.SetVector("_ChunkGlobalPosition", transform.position);
        marchingShader.SetFloat("_BrushSize", brushSize);
        marchingShader.SetFloat("_TerraformStrength", add? terraformingStrength : -terraformingStrength);
        marchingShader.SetInt("_Scale",GridMetrics.Scale);
        
        marchingShader.Dispatch(kernelID, GridMetrics.ThreadGroups(GridMetrics.LastLOD),GridMetrics.ThreadGroups(GridMetrics.LastLOD),GridMetrics.ThreadGroups(GridMetrics.LastLOD));
        weightsBuffer.GetData(weights);
        
        UpdateMesh();
        ReleaseBuffers();
    }

    private void CreateBuffers()
    {
        trianglesBuffer = new ComputeBuffer(
            5 * (GridMetrics.PointsPerChunk(LOD) *
                 GridMetrics.PointsPerChunk(LOD) *
                 GridMetrics.PointsPerChunk(LOD)),
            Triangle.SizeOf, ComputeBufferType.Append);
        trianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        weightsBuffer = new ComputeBuffer(
            GridMetrics.PointsPerChunk(GridMetrics.LastLOD) *
            GridMetrics.PointsPerChunk(GridMetrics.LastLOD) *
            GridMetrics.PointsPerChunk(GridMetrics.LastLOD),
            sizeof(float));
    }


    private void ReleaseBuffers()
    {
        trianglesBuffer.Release();
        trianglesCountBuffer.Release();
        weightsBuffer.Release();
    }

    private Mesh CreateMesh()
    {
        int gridSizeLastLOD = GridMetrics.PointsPerChunk(GridMetrics.LastLOD);
        int gridSizeLOD = GridMetrics.PointsPerChunk(LOD);


        int kernelID = marchingShader.FindKernel("March");
        
        marchingShader.SetBuffer(kernelID,"_Triangles",trianglesBuffer);
        marchingShader.SetBuffer(kernelID,"_Weights",weightsBuffer);


        marchingShader.SetInt("_ChunkSize", gridSizeLastLOD);
        marchingShader.SetInt("_LODSize", gridSizeLOD);
        marchingShader.SetInt("_Scale", GridMetrics.Scale );
        marchingShader.SetFloat("_IsoLevel", 0.5f);

        float loadScaleFactor = ((float)GridMetrics.PointsPerChunk(GridMetrics.LastLOD) +
                                 1 )/ (float)GridMetrics.PointsPerChunk(LOD);
        marchingShader.SetFloat("_LodScaleFactor",loadScaleFactor);
        
        weightsBuffer.SetData(weights);
        trianglesBuffer.SetCounterValue(0);
        marchingShader.Dispatch(0, GridMetrics.ThreadGroups(LOD), GridMetrics.ThreadGroups(LOD), GridMetrics.ThreadGroups(LOD));

        Triangle[] triangles = new Triangle[ReadTriangleCount()];
        trianglesBuffer.GetData(triangles);
        
        return CreateMeshFromTriangles(triangles);
    }

    private Mesh CreateMeshFromTriangles(Triangle[] triangles)
    {
        Vector3[] vertices = new Vector3[triangles.Length * 3];
        int[] tris = new int[triangles.Length * 3];

        for (int i = 0; i < triangles.Length; i++)
        {
            int startIndex = i * 3;

            vertices[startIndex] = triangles[i].a;
            vertices[startIndex+1] = triangles[i].b;
            vertices[startIndex+2] = triangles[i].c;
            tris[startIndex] = startIndex;
            tris[startIndex+1] = startIndex+1;
            tris[startIndex+2] = startIndex+2;
        }
        
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    private int ReadTriangleCount()
    {
        int[] triCount = { 0 };
        ComputeBuffer.CopyCount(trianglesBuffer,trianglesCountBuffer,0);
        trianglesCountBuffer.GetData(triCount);
        return triCount[0];
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + new Vector3(GridMetrics.PointsPerChunk(3)/2,GridMetrics.PointsPerChunk(3)/2,GridMetrics.PointsPerChunk(3)/2),Vector3.one * GridMetrics.PointsPerChunk(3));
        return;
        if (weights == null || weights.Length == 0)
            return;

        for (int x = 0; x < GridMetrics.PointsPerChunk(LOD); x++)
        {
            for (int y = 0; y < GridMetrics.PointsPerChunk(LOD); y++)
            {
                for (int z = 0; z < GridMetrics.PointsPerChunk(LOD); z++)
                {
                    int index = x + GridMetrics.PointsPerChunk(LOD) * (y + GridMetrics.PointsPerChunk(LOD) * z);
                    float value = weights[index];
                    Gizmos.color = Color.Lerp(Color.black, Color.white, value);
                    Gizmos.DrawSphere(new Vector3(x + transform.position.x,y + transform.position.y,z + transform.position.z), 0.1f);
                }
            }
        }
    }
}



