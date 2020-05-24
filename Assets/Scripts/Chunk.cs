using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private int _chunkIndex = 0;
    [SerializeField] private Vector3 _chunkSize = Vector3.zero;

    public int ChunkIndex => _chunkIndex;
    public Vector3 ChunkSize => _chunkSize;

    private List<Block> blocks = new List<Block>();

    private void Awake()
    {
        CreateChunk();
    }

    public void CreateChunk()
    {
        MeshFilter meshFilter = this.gameObject.AddComponent<MeshFilter>();
        Vector3 baseChunkPosition = new Vector3(_chunkIndex * _chunkSize.x, 0, 0);
        Mesh chunkMesh = new Mesh();

        List<Mesh> blocksMesh = new List<Mesh>();

        for (int x = 0; x < _chunkSize.x; x++)
        {
            for (int y = 0; y < _chunkSize.y; y++)
            {
                for (int z = 0; z < _chunkSize.z; z++)
                {
                    Block block = new Block(1, new Vector3(x, y, z) + baseChunkPosition);
                    blocks.Add(block);
                    blocksMesh.Add(block.CreateMesh());
                }
            }
        }

        CombineInstance[] combine = new CombineInstance[blocksMesh.Count];
        for (int i = 0; i < blocksMesh.Count; i++)
        {
            combine[i].mesh = blocksMesh[i];
        }

        chunkMesh.CombineMeshes(combine);

        meshFilter.mesh = chunkMesh;
    }

}
