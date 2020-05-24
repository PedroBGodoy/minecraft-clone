using System.Collections.Generic;
using UnityEngine;

public class Chunk2 : MonoBehaviour
{

    [SerializeField] private MeshFilter meshFilter = null;

    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _triangles = new List<int>();
    private List<Vector2> _uvs = new List<Vector2>();
    private int _vertexIndex = 0;

    private byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    private World _world;

    private void Start()
    {
        _world = GameObject.Find("World").GetComponent<World>();

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }

    private void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (y < 1)
                        voxelMap[x, y, z] = 0;
                    else if (y == VoxelData.ChunkHeight - 1)
                        voxelMap[x, y, z] = 1;
                    else
                        voxelMap[x, y, z] = 2;
                }
            }
        }
    }

    private void CreateMeshData()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    private bool CheckVoxel(Vector3 voxelPosition)
    {
        int x = Mathf.FloorToInt(voxelPosition.x);
        int y = Mathf.FloorToInt(voxelPosition.y);
        int z = Mathf.FloorToInt(voxelPosition.z);

        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
            return false;

        return _world.BlockTypes[voxelMap[x, y, z]].IsSolid;
    }

    private void AddVoxelDataToChunk(Vector3 voxelPosition)
    {
        for (int i = 0; i < 6; i++)
        {
            if (!CheckVoxel(voxelPosition + VoxelData.faceCheck[i]))
            {
                byte blockId = voxelMap[(int)voxelPosition.x, (int)voxelPosition.y, (int)voxelPosition.z];

                _vertices.Add(VoxelData.voxelVertes[VoxelData.voxelTris[i, 0]] + voxelPosition);
                _vertices.Add(VoxelData.voxelVertes[VoxelData.voxelTris[i, 1]] + voxelPosition);
                _vertices.Add(VoxelData.voxelVertes[VoxelData.voxelTris[i, 2]] + voxelPosition);
                _vertices.Add(VoxelData.voxelVertes[VoxelData.voxelTris[i, 3]] + voxelPosition);

                AddTexture(_world.BlockTypes[blockId].GetTextureID(i));

                _triangles.Add(_vertexIndex);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 2);
                _triangles.Add(_vertexIndex + 1);
                _triangles.Add(_vertexIndex + 3);
                _vertexIndex += 4;

                // for (int j = 0; j < 6; j++)
                // {
                //     int triangleIndex = VoxelData.voxelTris[i, j];

                //     _vertices.Add(VoxelData.voxelVertes[triangleIndex] + voxelPosition);
                //     _triangles.Add(_vertexIndex++);
                //     _uvs.Add(VoxelData.voxelUvs[j]);
                // }
            }
        }
    }

    private void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = _vertices.ToArray();
        mesh.triangles = _triangles.ToArray();
        mesh.uv = _uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void AddTexture(int textureID)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlcks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlcks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        _uvs.Add(new Vector2(x, y));
        _uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        _uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        _uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }

}
