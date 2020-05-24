using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public Transform player;
    public Vector3 spawnPosition;

    public Material Material;
    public BlockType[] BlockTypes;

    private Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    private List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    private ChunkCoord playerLastChunkCoord;
    private ChunkCoord playerCurrentChunkCoord;

    private void Start()
    {
        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight + 2, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();

        player.position = spawnPosition;
        playerLastChunkCoord = GetChunkCoodFromVector3(spawnPosition);
    }

    private void Update()
    {
        playerCurrentChunkCoord = GetChunkCoodFromVector3(player.position);

        if (!playerCurrentChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
            playerLastChunkCoord = playerCurrentChunkCoord;
        }
    }

    private void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                CreateChunk(x, z);
            }
        }
    }

    private ChunkCoord GetChunkCoodFromVector3(Vector3 position) => new ChunkCoord(Mathf.FloorToInt(position.x / VoxelData.ChunkWidth), Mathf.FloorToInt(position.z / VoxelData.ChunkWidth));

    private void CheckViewDistance()
    {
        ChunkCoord chunkPosition = GetChunkCoodFromVector3(player.position);
        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = chunkPosition.x - VoxelData.ViewDistanceInChunks; x < chunkPosition.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = chunkPosition.z - VoxelData.ViewDistanceInChunks; z < chunkPosition.z + VoxelData.ViewDistanceInChunks; z++)
            {
                if (IsChunkInWorld(new ChunkCoord(x, z)) && chunks[x, z] == null)
                    CreateChunk(x, z);
                else if (!chunks[x, z].IsActive)
                {
                    chunks[x, z].IsActive = true;
                    activeChunks.Add(new ChunkCoord(x, z));
                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                        previouslyActiveChunks.RemoveAt(i);
                }
            }
        }

        foreach (ChunkCoord chunk in previouslyActiveChunks)
        {
            chunks[chunk.x, chunk.z].IsActive = false;
        }
    }

    public byte GetVoxel(Vector3 voxelPosition)
    {
        if (!IsVoxelInWorld(voxelPosition))
            return 0; // AIR
        if (voxelPosition.y < 1)
            return 3; // BEDROCK
        else if (voxelPosition.y == VoxelData.ChunkHeight - 1)
            return 2; // STONE
        else
            return 1; // GRASS
    }

    private void CreateChunk(int x, int z)
    {
        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this); ;
        activeChunks.Add(new ChunkCoord(x, z));
    }

    private bool IsChunkInWorld(ChunkCoord chunkPosition) =>
        chunkPosition.x > 0 && chunkPosition.x < VoxelData.WorldSizeInChunks - 1 &&
        chunkPosition.z > 0 && chunkPosition.z < VoxelData.WorldSizeInChunks - 1;

    private bool IsVoxelInWorld(Vector3 voxelPosition) =>
        voxelPosition.x >= 0 && voxelPosition.x < VoxelData.WorldSizeInVoxels &&
        voxelPosition.y >= 0 && voxelPosition.y < VoxelData.ChunkHeight &&
        voxelPosition.z >= 0 && voxelPosition.z < VoxelData.WorldSizeInVoxels;

}
