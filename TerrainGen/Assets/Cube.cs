using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Cube
{
    public Vertex[] vertices = new Vertex[8];
    public Vector3 center;
    private int configuration;
    public List<Vector3> triangleCorners = new List<Vector3>();

    public Cube(float[] verticesValues)
    {
        for (int i = 0; i < verticesValues.Length; i++)
        {
            vertices[i] = new Vertex(verticesValues[i]);
        }

        SetConfiguration();
    }
    public void SetConfiguration()
    {
        triangleCorners.Clear();
        configuration = 0;
        if (vertices[0].value < MarchingCubesTables.isolevel) configuration |= 1;
        if (vertices[1].value < MarchingCubesTables.isolevel) configuration |= 2;
        if (vertices[2].value < MarchingCubesTables.isolevel) configuration |= 4;
        if (vertices[3].value < MarchingCubesTables.isolevel) configuration |= 8;
        if (vertices[4].value < MarchingCubesTables.isolevel) configuration |= 16;
        if (vertices[5].value < MarchingCubesTables.isolevel) configuration |= 32;
        if (vertices[6].value < MarchingCubesTables.isolevel) configuration |= 64;
        if (vertices[7].value < MarchingCubesTables.isolevel) configuration |= 128;
        Debug.Log(configuration);
        int[] edges = MarchingCubesTables.triTable[configuration];
        for (int i = 0; edges[i] != -1; i += 3)
        {
            // First edge lies between vertex e00 and vertex e01
            int e00 = MarchingCubesTables.edgeConnections[edges[i]][0];
            int e01 = MarchingCubesTables.edgeConnections[edges[i]][1];

            // Second edge lies between vertex e10 and vertex e11
            int e10 = MarchingCubesTables.edgeConnections[edges[i + 1]][0];
            int e11 = MarchingCubesTables.edgeConnections[edges[i + 1]][1];
        
            // Third edge lies between vertex e20 and vertex e21
            int e20 = MarchingCubesTables.edgeConnections[edges[i + 2]][0];
            int e21 = MarchingCubesTables.edgeConnections[edges[i + 2]][1];

            // We add our triangle here
            // triangleCorners.Add((vertices[e00].position + vertices[e01].position) / 2) ;
            // triangleCorners.Add((vertices[e10].position + vertices[e11].position) / 2) ;
            // triangleCorners.Add((vertices[e20].position + vertices[e21].position) / 2) ;
            triangleCorners.Add(InterpolateVertix(MarchingCubesTables.cubeCorners[e00],MarchingCubesTables.cubeCorners[e01],vertices[e00].value,vertices[e01].value));
            triangleCorners.Add(InterpolateVertix(MarchingCubesTables.cubeCorners[e10],MarchingCubesTables.cubeCorners[e11],vertices[e10].value,vertices[e11].value));
            triangleCorners.Add(InterpolateVertix(MarchingCubesTables.cubeCorners[e20],MarchingCubesTables.cubeCorners[e21],vertices[e20].value,vertices[e21].value));
        }
    }

    private Vector3 InterpolateVertix(Vector3 p1, Vector3 p2, float val1, float val2)
    {
        return (p1 + (0.5f - val1) * (p2 - p1)  / (val2 - val1));

        float iso = .5f;
        float mu;
        if (Mathf.Abs(iso - val1) < 0.001)
        {
            return p1;
        }
        if (Mathf.Abs(iso - val2) < 0.001)
        {
            return p2;
        }
        if (Mathf.Abs(val1 - val2) < 0.001)
        {
            return p1;
        }

        mu = (iso - val1) / (val2 - val1);
        float pX = p1.x + mu * (p2.x - p1.x);
        float pY = p1.y + mu * (p2.y - p1.y);
        float pZ = p1.z + mu * (p2.z - p1.z);
        Debug.Log(new Vector3(pX, pY, pZ).ToString());
        return new Vector3(pX, pY, pZ);
    }
}

public class Vertex
{
    //public Vector3 position;
    public float value;

    public Vertex(float value)
    {
        this.value = Mathf.Clamp01(value);
    }
}
