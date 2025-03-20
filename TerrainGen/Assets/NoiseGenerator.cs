using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    private ComputeBuffer weightBuffer;
    public ComputeShader noiseShader;
    
    [SerializeField] float noiseScale = 1f;
    [SerializeField] float amplitude = 5f;
    [SerializeField] float frequency = 0.005f;
    [SerializeField] int octaves = 8;
    [SerializeField, Range(0f, 1f)] float groundPercent = 0.2f;
    //[SerializeField] private Vector3 offset;

    public List<Chunk> chunks;

    private void Awake()
    {
        chunks.Clear();
    }

    private void CreateBuffers(int LOD)
    {
        weightBuffer = new ComputeBuffer(GridMetrics.PointsPerChunk(LOD)*GridMetrics.PointsPerChunk(LOD)*GridMetrics.PointsPerChunk(LOD), sizeof(float));
    }

    private void ReleaseBuffers()
    {
        weightBuffer.Release();
    }

    public float[] GetNoise(int LOD, Vector3 chunkPosition)
    {
        int gridSize = GridMetrics.PointsPerChunk(LOD); 
        // Create the compute buffer with the new grid size
        CreateBuffers(LOD, gridSize);

        // Allocate an array to receive noise values: gridSize^3 elements
        float[] noiseValues = new float[gridSize * gridSize * gridSize];

        int kernelID = noiseShader.FindKernel("GenerateNoise");
        noiseShader.SetBuffer(kernelID, "_Weights", weightBuffer);

        // Pass the updated grid size to the compute shader
        noiseShader.SetInt("_ChunkSize", gridSize);
        noiseShader.SetFloat("_NoiseScale", noiseScale);
        noiseShader.SetFloat("_Amplitude", amplitude);
        noiseShader.SetFloat("_Frequency", frequency);
        noiseShader.SetInt("_Octaves", octaves);
        noiseShader.SetFloat("_GroundPercent", groundPercent);
        noiseShader.SetInt("_Scale", GridMetrics.Scale);
        noiseShader.SetInt("_GroundLevel", GridMetrics.GroundLevel);
        noiseShader.SetVector("_ChunkPosition", chunkPosition);

        // The offset remains similar; adjust if needed for global noise coordinates.
        Vector3 offset = -chunkPosition / gridSize;
        noiseShader.SetVector("_Offset", offset);

        // Adjust thread groups so that they cover gridSize instead of baseSize.
        int groups = Mathf.CeilToInt((float)gridSize / GridMetrics.NumThreads);
        noiseShader.Dispatch(kernelID, groups, groups, groups);

        // Now GetData should match the number of elements in the compute buffer.
        weightBuffer.GetData(noiseValues);

        ReleaseBuffers();
        return noiseValues;
    }

    private void CreateBuffers(int LOD, int gridSize)
    {
        // Allocate the compute buffer to match the new grid resolution.
        weightBuffer = new ComputeBuffer(gridSize * gridSize * gridSize, sizeof(float));
    }



    private void OnValidate()
    {
        foreach (Chunk chunk in chunks)
        {
            if (chunk != null)
            {
                chunk.Create();
            }
        }
    }
}
