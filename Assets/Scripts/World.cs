using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes biome;

    public Transform player;
    public Vector3 spawnPosition;

    public Material Material;
    public BlockType[] BlockTypes;

    private Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    private List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    private ChunkCoord playerLastChunkCoord;
    private ChunkCoord playerCurrentChunkCoord;

    private List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    private bool isCreatingChunks = false;

    private void Start()
    {
        Random.InitState(seed);

        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight + 2, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();

        player.position = spawnPosition;
        playerCurrentChunkCoord = playerLastChunkCoord = GetChunkCoodFromVector3(spawnPosition);
    }

    private void Update()
    {
        playerCurrentChunkCoord = GetChunkCoodFromVector3(player.position);

        if (!playerCurrentChunkCoord.Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
            playerLastChunkCoord = playerCurrentChunkCoord;
        }

        if (chunksToCreate.Count > 0 && !isCreatingChunks)
            StartCoroutine("CreateChunks");
    }

    private void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true); ;
                chunksToCreate.Add(new ChunkCoord(x, z));
            }
        }
    }

    IEnumerator CreateChunks()
    {
        isCreatingChunks = true;

        while (chunksToCreate.Count > 0)
        {
            chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
            chunksToCreate.RemoveAt(0);
            yield return null;
        }

        isCreatingChunks = false;
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
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    }
                    else if (!chunks[x, z].isActive)
                        chunks[x, z].isActive = true;
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
            chunks[chunk.x, chunk.z].isActive = false;
        }
    }

    public bool CheckForVoxel(Vector3 voxelPosition)
    {
        ChunkCoord thisChunkCoord = new ChunkCoord(voxelPosition);

        if (IsVoxelInWorld(voxelPosition) || voxelPosition.y < 0 || voxelPosition.y > VoxelData.ChunkHeight)
            return false;

        Chunk thisChunk = chunks[thisChunkCoord.x, thisChunkCoord.z];
        if (thisChunk != null && thisChunk.isVoxelMapPopulated)
            return BlockTypes[thisChunk.GetVoxelFromGlobalVector3(voxelPosition)].IsSolid;

        return BlockTypes[GetVoxel(voxelPosition)].IsSolid;
    }

    public byte GetVoxel(Vector3 voxelPosition)
    {
        int yPos = Mathf.FloorToInt(voxelPosition.y);

        if (!IsVoxelInWorld(voxelPosition))
            return 0; // AIR

        if (yPos == 0)
            return 3; // BEDROCK

        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(voxelPosition.x, voxelPosition.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = 2; // GRASS
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = 5; // DIRT
        else if (yPos > terrainHeight)
            return 0; // AIR
        else
            voxelValue = 1; // STONE

        if (voxelValue == 1)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (Noise.Get3DPerlin(voxelPosition, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
            }
        }
        return voxelValue;
    }

    private bool IsChunkInWorld(ChunkCoord chunkPosition) =>
        chunkPosition.x > 0 && chunkPosition.x < VoxelData.WorldSizeInChunks - 1 &&
        chunkPosition.z > 0 && chunkPosition.z < VoxelData.WorldSizeInChunks - 1;

    private bool IsVoxelInWorld(Vector3 voxelPosition) =>
        voxelPosition.x >= 0 && voxelPosition.x < VoxelData.WorldSizeInVoxels &&
        voxelPosition.y >= 0 && voxelPosition.y < VoxelData.ChunkHeight &&
        voxelPosition.z >= 0 && voxelPosition.z < VoxelData.WorldSizeInVoxels;

}
