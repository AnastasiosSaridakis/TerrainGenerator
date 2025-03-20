using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    private Vector3Int currentChunkCoords;

    public bool GenerateChunksOnStart = false;
    public int radius = 1;
    private void Start()
    {
        if (GenerateChunksOnStart)
        {
            ChunkGenerator.GenerateChunksAroundPlayer(ChunkGenerator.GetNormalizedVectorFromPosition(transform.position), radius);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3Int ChunkCoords = ChunkGenerator.GetNormalizedVectorFromPosition(transform.position);
        
        if (ChunkCoords != currentChunkCoords)
        {
            currentChunkCoords = ChunkCoords;
            
            ChunkGenerator.GenerateChunksAroundPlayer(ChunkCoords, radius);
        }
    }
}
