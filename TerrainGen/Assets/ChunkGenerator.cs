using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    public static ChunkGenerator Instance;
    
    public GameObject chunk;
    public NoiseGenerator noiseGenerator;

    public static Dictionary<Vector3Int, Chunk> chunks;

    private static List<Vector3Int> positionsAroundPlayer = new List<Vector3Int>()
    {
        Vector3Int.zero,
        new Vector3Int(1,0,0),
        new Vector3Int(-1,0,0),
        new Vector3Int(0,0,1),
        new Vector3Int(0,0,-1),
        new Vector3Int(1,0,1),
        new Vector3Int(1,0,-1),
        new Vector3Int(-1,0,1),
        new Vector3Int(-1,0,-1),
    };

    private void Awake()
    {
        Instance = this;
        chunks = new Dictionary<Vector3Int, Chunk>();
    }

    public static Chunk GetChunkFromCoords(Vector3 coords)
    {
        Vector3Int normalizedVector = GetNormalizedVectorFromPosition(coords);
        if (chunks.ContainsKey(normalizedVector))
        {
            Debug.Log("Returning existing chunk");
            return chunks[normalizedVector];
        }
        else
        {
            GameObject newChunkObject = Instantiate(ChunkGenerator.Instance.chunk, normalizedVector, quaternion.identity);
            Chunk newChunk = newChunkObject.GetComponent<Chunk>();
            chunks.Add(normalizedVector,newChunk);
            Debug.Log($"Creating new chunk at {normalizedVector}");
            return newChunk;
        }
    }
    public static Vector3Int GetNormalizedVectorFromPosition(Vector3 position)
    {
        float Xpos = position.x >= 0 ? position.x : position.x - GridMetrics.Scale;
        float Ypos = position.y >= 0 ? position.y : position.y - GridMetrics.Scale;
        float Zpos = position. z>= 0 ? position.z : position.z - GridMetrics.Scale;
        return new Vector3Int(((int)Xpos / GridMetrics.Scale) *  GridMetrics.Scale, ((int)Ypos / GridMetrics.Scale) * GridMetrics.Scale, ((int)Zpos / GridMetrics.Scale) * GridMetrics.Scale);
    }
    
    public static Chunk GetChunkFromCoords(Vector3Int normalizedVector)
    {
        if (chunks.ContainsKey(normalizedVector))
        {
            return chunks[normalizedVector];
        }
        else
        {
            GameObject newChunkObject = Instantiate(ChunkGenerator.Instance.chunk, normalizedVector, quaternion.identity);
            Chunk newChunk = newChunkObject.GetComponent<Chunk>();
            chunks.Add(normalizedVector,newChunk);
            return newChunk;
        }
    }

    public static void GenerateChunksAroundPlayer(Vector3Int normalizedPosiiton, int radius)
    {
            foreach (var chunk in chunks.Values)
        {
            if (chunk.gameObject.activeSelf)
            {
                chunk.gameObject.SetActive(false);
            }
        }
        foreach (var position in positionsAroundPlayer)
        {
            Chunk chunk = GetChunkFromCoords(normalizedPosiiton + position * radius * GridMetrics.Scale);
            chunk.gameObject.SetActive(true);
        }
    }
}
