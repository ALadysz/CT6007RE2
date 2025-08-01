using UnityEngine;

//leaf mesh generator
public static class LeafGen
{
    //makes the leaf! all code is done within the function to be able to easily call from the thing
    public static Mesh MakeLeaf(LeafInfo info, float size = 1f)
    {
        var mesh = new Mesh { name = "Procedural Leaf" };

        //apply size stuff
        float finalLength = info.length * size;
        float finalWidth = info.width * size;
        float halfWidth = Mathf.Max(finalWidth, 0.001f) / 2f;
        float yOffset = finalLength / 2f;

        //define vertices
        var basePositions = new Vector3[] {
            new Vector3(0f, -yOffset, 0f),
            new Vector3(-Mathf.Lerp(halfWidth, 0f, info.baseSharpness), (finalLength * info.baseHeight) - yOffset, 0f),
            new Vector3(-Mathf.Lerp(halfWidth, 0f, info.tipSharpness), (finalLength * info.tipHeight) - yOffset, 0f),
            new Vector3(0f, finalLength - yOffset, 0f),
            new Vector3(Mathf.Lerp(halfWidth, 0f, info.tipSharpness), (finalLength * info.tipHeight) - yOffset, 0f),
            new Vector3(Mathf.Lerp(halfWidth, 0f, info.baseSharpness), (finalLength * info.baseHeight) - yOffset, 0f)
        };

        var verts = new Vector3[12];
        var norms = new Vector3[12];
        var uvs = new Vector2[12];

        //process vertices
        for (int i = 0; i < 6; i++)
        {
            Vector3 pos = basePositions[i];

            //calculate bend and fold angles
            float yPercent = (pos.y + yOffset) / finalLength;
            float bendAngle = Mathf.Sin(yPercent * Mathf.PI) * info.lengthBend;
            float foldAngle = info.widthFold * Mathf.Abs(pos.x / halfWidth);

            //make rotations based off angles
            Quaternion bend = Quaternion.Euler(bendAngle, 0f, 0f);
            Quaternion fold = Quaternion.Euler(0f, 0f, pos.x < 0 ? -foldAngle : foldAngle);
            Quaternion rot = fold * bend; //combine

            //front side
            verts[i] = rot * pos;
            norms[i] = rot * Vector3.back;

            //back side
            verts[i + 6] = verts[i];
            norms[i + 6] = rot * Vector3.forward;
        }

        //define triangles
        int[] triangles = {
            0, 1, 2,   0, 2, 3,   0, 3, 4,   0, 4, 5,
            6, 8, 7,   6, 9, 8,   6, 10, 9,  6, 11, 10
        };

        //define uvs
        var baseUVs = new Vector2[] {
            new Vector2(0.5f, 0f),
            new Vector2(Mathf.Lerp(0.5f, 0f, info.baseSharpness), info.baseHeight),
            new Vector2(Mathf.Lerp(0.5f, 0f, info.tipSharpness), info.tipHeight),
            new Vector2(0.5f, 1f),
            new Vector2(Mathf.Lerp(0.5f, 1f, info.tipSharpness), info.tipHeight),
            new Vector2(Mathf.Lerp(0.5f, 1f, info.baseSharpness), info.baseHeight)
        };

        for (int i = 0; i < 6; i++)
        {
            uvs[i] = baseUVs[i];
            uvs[i + 6] = baseUVs[i];
        }

        //assign everything we calculated to mesh
        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateBounds();

        return mesh;
    }
}