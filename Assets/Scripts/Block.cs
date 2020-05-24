using System.Collections.Generic;
using UnityEngine;

public class Block
{
    private int _blockType = 1;
    private Vector3 _position;

    public Vector3 Position
    {
        get
        {
            return _position;
        }
        private set
        {
            _position = value;
        }
    }

    public Block(int blockType, Vector3 position)
    {
        _blockType = blockType;
        _position = position;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();

        float length = 1f;
        float width = 1f;
        float height = 1f;

        Vector3[] c = new Vector3[8];

        c[0] = new Vector3(-length * .5f, -width * .5f, height * .5f);
        c[1] = new Vector3(length * .5f, -width * .5f, height * .5f);
        c[2] = new Vector3(length * .5f, -width * .5f, -height * .5f);
        c[3] = new Vector3(-length * .5f, -width * .5f, -height * .5f);

        c[4] = new Vector3(-length * .5f, width * .5f, height * .5f);
        c[5] = new Vector3(length * .5f, width * .5f, height * .5f);
        c[6] = new Vector3(length * .5f, width * .5f, -height * .5f);
        c[7] = new Vector3(-length * .5f, width * .5f, -height * .5f);

        List<Vector3> test = new List<Vector3>();

        foreach (Vector3 point in c)
        {
            test.Add(point + _position);
        }

        Vector3[] vertices = new Vector3[]
        {
            test[0], test[1], test[2], test[3], // Bottom
	        test[7], test[4], test[0], test[3], // Left
	        test[4], test[5], test[1], test[0], // Front
	        test[6], test[7], test[3], test[2], // Back
	        test[5], test[6], test[2], test[1], // Right
	        test[7], test[6], test[5], test[4]  // Top
        };

        Vector3 up = Vector3.up;
        Vector3 down = Vector3.down;
        Vector3 forward = Vector3.forward;
        Vector3 back = Vector3.back;
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;

        Vector3[] normals = new Vector3[]
        {
            down, down, down, down,             // Bottom
	        left, left, left, left,             // Left
	        forward, forward, forward, forward,	// Front
	        back, back, back, back,             // Back
	        right, right, right, right,         // Right
	        up, up, up, up	                    // Top
        };

        Vector2 uv00 = new Vector2(0f, 0f);
        Vector2 uv10 = new Vector2(1f, 0f);
        Vector2 uv01 = new Vector2(0f, 1f);
        Vector2 uv11 = new Vector2(1f, 1f);

        Vector2[] uvs = new Vector2[]
        {
            uv11, uv01, uv00, uv10, // Bottom
	        uv11, uv01, uv00, uv10, // Left
	        uv11, uv01, uv00, uv10, // Front
	        uv11, uv01, uv00, uv10, // Back
	        uv11, uv01, uv00, uv10, // Right
	        uv11, uv01, uv00, uv10  // Top
        };

        int[] triangles = new int[]
        {
            3, 1, 0,        3, 2, 1,        // Bottom
	        7, 5, 4,        7, 6, 5,        // Left
	        11, 9, 8,       11, 10, 9,      // Front
	        15, 13, 12,     15, 14, 13,     // Back
	        19, 17, 16,     19, 18, 17,	    // Right
	        23, 21, 20,     23, 22, 21,	    // Top
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;

        return mesh;
    }

}
