using UnityEngine;

public class VoxelData : MonoBehaviour
{
    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;
    public static readonly int WorldSizeInChunks = 50;
    public static int WorldSizeInVoxels => WorldSizeInChunks * ChunkWidth;

    public static readonly int ViewDistanceInChunks = 5;

    public static readonly int TextureAtlasSizeInBlcks = 16;
    public static float NormalizedBlockTextureSize => 1f / TextureAtlasSizeInBlcks;

    public static readonly Vector3[] voxelVertes = new Vector3[8] {
        new Vector3(0,0,0),
        new Vector3(1,0,0),
        new Vector3(1,1,0),
        new Vector3(0,1,0),
        new Vector3(0,0,1),
        new Vector3(1,0,1),
        new Vector3(1,1,1),
        new Vector3(0,1,1),
    };

    public static readonly Vector3[] faceCheck = new Vector3[6]{
        new Vector3(0,0,-1), // BACK
        new Vector3(0,0,1) , // FRONT
        new Vector3(0,1,0) , // TOP
        new Vector3(0,-1,0), // BOTTOM
        new Vector3(-1,0,0), // LEFT
        new Vector3(1,0,0) , // RIGHT
    };

    public static readonly int[,] voxelTris = new int[6, 4] {
        {0,3,1,2}, // BACK
        {5,6,4,7}, // FRONT
        {3,7,2,6}, // TOP
        {1,5,0,4}, // BOTTOM
        {4,7,0,3}, // LEFT
        {1,2,5,6}, // RIGHT
    };

    public static readonly Vector2[] voxelUvs = new Vector2[4] {
        new Vector2(0,0),
        new Vector2(0,1),
        new Vector2(1,0),
        new Vector2(1,1),
    };

}
