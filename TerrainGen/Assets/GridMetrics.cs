
public static class GridMetrics
{
    public const int GroundLevel = Scale / 2;
    public const int NumThreads = 8;
    public const int Scale = 32;
    
    public static int[] LODs =
    {
        8,
        16,
        24,
        32,
        40
    };
    public static int LastLOD = LODs.Length - 1;

    public static int PointsPerChunk(int lodIndex)
    {
        return LODs[lodIndex];
    }

    public static int ThreadGroups(int lodIndex)
    {
        return LODs[lodIndex] / NumThreads;
    }
}