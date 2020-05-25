using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord chunkPosition;

    private GameObject chunkObject;
    private MeshFilter meshFilter = null;
    private MeshRenderer meshRenderer = null;

    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _triangles = new List<int>();
    private List<Vector2> _uvs = new List<Vector2>();
    private int _vertexIndex = 0;

    private byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    private World _world;

    public bool isVoxelMapPopulated = false;

    private bool _isActive;
    public bool isActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
        }
    }

    public Vector3 ChunkUnityPosition => chunkObject.transform.position;

    private bool IsVoxelInChunk(Vector3 voxelPosition) =>
        voxelPosition.x >= 0 && voxelPosition.x < VoxelData.ChunkWidth &&
        voxelPosition.y >= 0 && voxelPosition.y < VoxelData.ChunkHeight &&
        voxelPosition.z >= 0 && voxelPosition.z < VoxelData.ChunkWidth;

    public Chunk(ChunkCoord _chunkPosition, World world, bool generateOnLoad)
    {
        chunkPosition = _chunkPosition;
        _world = world;
        isActive = true;

        if (generateOnLoad)
            Init();
    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = _world.Material;
        chunkObject.transform.SetParent(_world.transform);
        chunkObject.transform.position = new Vector3(chunkPosition.x * VoxelData.ChunkWidth, 0, chunkPosition.z * VoxelData.ChunkWidth);
        chunkObject.name = $"Chunk {chunkPosition.x},{chunkPosition.z}";

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
                    voxelMap[x, y, z] = _world.GetVoxel(new Vector3(x, y, z) + this.ChunkUnityPosition);
                }
            }
        }

        isVoxelMapPopulated = true;
    }

    private void CreateMeshData()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (_world.BlockTypes[voxelMap[x, y, z]].IsSolid)
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

        if (!IsVoxelInChunk(voxelPosition))
            return _world.CheckForVoxel(voxelPosition + ChunkUnityPosition);

        return _world.BlockTypes[voxelMap[x, y, z]].IsSolid;
    }

    public byte GetVoxelFromGlobalVector3(Vector3 voxelPosition)
    {
        int xCheck = Mathf.FloorToInt(voxelPosition.x);
        int yCheck = Mathf.FloorToInt(voxelPosition.y);
        int zCheck = Mathf.FloorToInt(voxelPosition.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelMap[xCheck, yCheck, zCheck];
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

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;
    }

    public bool Equals(ChunkCoord other) => other != null && x == other.x && z == other.z;
}
