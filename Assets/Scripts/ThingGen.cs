using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

//main generator for things :)
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ThingGen : MonoBehaviour
{
    //public variables
    //types of things
    public enum Thing { Oak, Leaves, Coral, Flower }

    [Header("Thing Type and Material")]
    public Thing thingType = Thing.Oak; //current setting
    public Material branchMat;
    public Material leafMat;

    [Header("Thing's Settings")]
    public LSInfo lsInfo;

    //private variables
    private Thing lastThing; //tracks thing type to make sure OnValidate doesn't change sliders 
    private MeshFilter branchMF;
    private MeshRenderer branchMR;
    private Dictionary<Thing, LSInfo> settings;
    private Stack<tortleState> tortleMemory;
    private Mesh leafMesh;
    private tortleState tortle;

    private struct tortleState { public Vector3 pos; public Quaternion rot; public float length; public float radius; }
    private class MeshData
    {
        public List<Vector3> verts = new List<Vector3>();
        public List<int> tris = new List<int>();
        public List<Vector2> uvs = new List<Vector2>();
    }

    //reloads preset if chosen thing changes
    private void OnValidate()
    {
        //check if settings loaded
        if (settings == null)
        {
            settings = CreateSettings();
        }

        //only reload settings if missing data or new thing chosen
        if (lsInfo == null || thingType != lastThing)
        {
            LoadSettings();
        }
    }

    //load setting
    public void LoadSettings()
    {
        settings ??= CreateSettings();
        if (settings.ContainsKey(thingType))
        {
            lsInfo = settings[thingType].Clone();
            lastThing = thingType; //update tracker
        }
    }

    //generator of things!
    public void Generate()
    {
        branchMF ??= GetComponent<MeshFilter>();
        branchMR ??= GetComponent<MeshRenderer>();
        settings ??= CreateSettings();

        //setup
        MeshData branchData = new MeshData();
        MeshData leafData = new MeshData();
        tortleMemory = new Stack<tortleState>();
        tortle = new tortleState
        {
            pos = Vector3.zero,
            rot = Quaternion.identity,
            length = lsInfo.initialLength,
            radius = lsInfo.initialRadius
        };

        if (lsInfo.generateLeaves && lsInfo.leafInfo != null)
        {
            leafMesh = LeafGen.MakeLeaf(lsInfo.leafInfo, lsInfo.leafSize);
        }

        //actual generation
        string sequence = LS.Sequencifier(lsInfo);
        var tortleActions = SetupTortleActions(branchData, leafData);

        AddRing(tortle.pos, tortle.rot, tortle.radius, 0, branchData);
        foreach (char command in sequence)
        {
            if (tortleActions.TryGetValue(command, out var handler))
            {
                handler.Invoke();
            }
        }

        //final steps
        MakeMesh(branchMF, branchMR, "Branches", branchData, branchMat);
        GameObject leafObj = AddLeafObject("Leaves");
        MakeMesh(leafObj.GetComponent<MeshFilter>(), leafObj.GetComponent<MeshRenderer>(), "Leaves", leafData, leafMat);
    }

    //sets up all the different actions of the tortle
    private Dictionary<char, System.Action> SetupTortleActions(MeshData branchData, MeshData leafData)
    {
        return new Dictionary<char, System.Action>
        {
            {'F', () => {
                int lastRingStart = branchData.verts.Count - (lsInfo.sides + 1);
                tortle.pos += tortle.rot * Vector3.up * tortle.length;
                AddRing(tortle.pos, tortle.rot, tortle.radius, tortle.pos.y, branchData);
                ConnectRings(lastRingStart, lsInfo.sides, branchData);
                tortle.length *= lsInfo.lengthModifier;
                tortle.radius = Mathf.Max(tortle.radius * lsInfo.radiusModifier, lsInfo.minRadius);
            }},
            {'f', () => tortle.pos += tortle.rot * Vector3.up * tortle.length },
            {'L', () => { if(lsInfo.generateLeaves && leafMesh != null) AddMesh(leafData, leafMesh, tortle.pos, tortle.rot); }},
            {'P', () => {
                if (!lsInfo.generateLeaves || leafMesh == null) return;
                float separationAngle = 360f / lsInfo.petalCount;
                for (int i = 0; i < lsInfo.petalCount; i++) {
                    tortleMemory.Push(tortle);
                    tortle.rot *= Quaternion.Euler(0, i * separationAngle, 0);
                    tortle.rot *= Quaternion.Euler(lsInfo.petalSplayAngle, 0, 0);
                    tortle.rot *= Quaternion.Euler(0, 0, 45f);
                    tortle.pos += tortle.rot * Vector3.up * (lsInfo.initialLength * 0.5f);
                    AddMesh(leafData, leafMesh, tortle.pos, tortle.rot * Quaternion.Euler(180, 0, 0));
                    tortle = tortleMemory.Pop();
                }
            }},
            {'[', () => {
                tortleMemory.Push(tortle);
                AddRing(tortle.pos, tortle.rot, tortle.radius, tortle.pos.y, branchData);
                tortle.rot *= Quaternion.Euler(
                    Random.Range(-lsInfo.branchDirectionRandomness, lsInfo.branchDirectionRandomness),
                    Random.Range(-lsInfo.branchDirectionRandomness, lsInfo.branchDirectionRandomness),
                    Random.Range(-lsInfo.branchDirectionRandomness, lsInfo.branchDirectionRandomness)
                );
            }},
            {']', () => { if (tortleMemory.Count > 0) tortle = tortleMemory.Pop(); }},
            {'+', () => tortle.rot *= Quaternion.Euler(0, lsInfo.angle, 0) },
            {'-', () => tortle.rot *= Quaternion.Euler(0, -lsInfo.angle, 0) },
            {'&', () => tortle.rot *= Quaternion.Euler(lsInfo.angle, 0, 0) },
            {'^', () => tortle.rot *= Quaternion.Euler(-lsInfo.angle, 0, 0) },
            {'\\',() => tortle.rot *= Quaternion.Euler(0, 0, lsInfo.angle) },
            {'/', () => tortle.rot *= Quaternion.Euler(0, 0, -lsInfo.angle) },
            {'X', () => {}}, { '!', () => {}}
        };
    }

    private void AddRing(Vector3 pos, Quaternion rot, float rad, float vCoord, MeshData data)
    {
        for (int i = 0; i <= lsInfo.sides; i++)
        {
            float angle = i * (360f / lsInfo.sides);
            Quaternion ringRot = Quaternion.AngleAxis(angle, Vector3.up);
            data.verts.Add(pos + rot * ringRot * (Vector3.forward * rad));
            data.uvs.Add(new Vector2((float)i / lsInfo.sides, vCoord));
        }
    }

    private void ConnectRings(int lastRingStart, int sides, MeshData data)
    {
        int currentRingStart = lastRingStart + sides + 1;
        for (int i = 0; i < sides; i++)
        {
            data.tris.AddRange(new int[] {
                lastRingStart + i, lastRingStart + i + 1, currentRingStart + i,
                lastRingStart + i + 1, currentRingStart + i + 1, currentRingStart + i
            });
        }
    }

    private void AddMesh(MeshData data, Mesh meshToAdd, Vector3 pos, Quaternion rot)
    {
        int vertOffset = data.verts.Count;
        foreach (var vert in meshToAdd.vertices) { data.verts.Add(pos + rot * vert); }
        foreach (var tri in meshToAdd.triangles) { data.tris.Add(tri + vertOffset); }
        data.uvs.AddRange(meshToAdd.uv);
    }

    private void MakeMesh(MeshFilter filter, MeshRenderer renderer, string name, MeshData data, Material mat)
    {
        if (filter == null || renderer == null) return;
        if (filter.sharedMesh != null) DestroyImmediate(filter.sharedMesh);

        if (data.verts.Count < 3)
        {
            filter.sharedMesh = null;
            renderer.enabled = false;
            return;
        }

        renderer.enabled = true;
        var mesh = new Mesh { name = name };
        if (data.verts.Count > 65534)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        }

        mesh.SetVertices(data.verts);
        mesh.SetTriangles(data.tris, 0);
        mesh.SetUVs(0, data.uvs);
        mesh.RecalculateNormals();

        filter.sharedMesh = mesh;
        renderer.material = mat;
    }


    //adds object for leaves mesh and its dependencies
    private GameObject AddLeafObject(string name)
    {
        Transform child = transform.Find(name);
        if (child != null) return child.gameObject;

        var newChild = new GameObject(name);
        newChild.transform.SetParent(transform, false);
        newChild.AddComponent<MeshFilter>();
        newChild.AddComponent<MeshRenderer>();
        return newChild;
    }

    //sets settings for each thing
    private Dictionary<Thing, LSInfo> CreateSettings()
    {
        return new Dictionary<Thing, LSInfo>
        {
            [Thing.Oak] = new LSInfo
            {
                axiom = "X",
                rules = new[] { new LSInfo.Rule { character = 'X', result = "F[+XL]F[-XL][^XL]F[&XL]FX" } },
                iterations = 4,
                angle = 30f,
                initialLength = 0.8f,
                lengthModifier = 0.8f,
                initialRadius = 0.15f,
                radiusModifier = 0.8f,
                minRadius = 0.02f,
                branchDirectionRandomness = 8f,
                leafSize = 1.2f,
                leafInfo = new LeafInfo { length = 0.4f, width = 0.3f, lengthBend = 10f }
            },
            [Thing.Leaves] = new LSInfo
            {
                axiom = "X",
                rules = new[] { new LSInfo.Rule { character = 'X', result = "f[+^L]X" } },
                iterations = 15,
                angle = 20f,
                initialLength = 0.2f,
                lengthModifier = 0.98f,
                initialRadius = 0.04f,
                radiusModifier = 0.98f,
                branchDirectionRandomness = 15f,
                leafSize = 0.8f,
                flowerPositionOffset = new Vector3(0.0f, -3.07f, 0.0f),
                leafInfo = new LeafInfo { length = 0.8f, width = 0.2f, lengthBend = 25f }
            },
            [Thing.Coral] = new LSInfo
            {
                axiom = "F",
                rules = new[] { new LSInfo.Rule { character = 'F', result = "F[+F][-F][&F]" } },
                iterations = 4,
                angle = 20f,
                initialLength = 0.4f,
                lengthModifier = 0.9f,
                initialRadius = 0.1f,
                radiusModifier = 0.9f,
                branchDirectionRandomness = 25f,
                generateLeaves = false,
                leafInfo = new LeafInfo()
            },
            [Thing.Flower] = new LSInfo
            {
                axiom = "S",
                rules = new[] { new LSInfo.Rule { character = 'S', result = "FFFF![H]" }, new LSInfo.Rule { character = 'H', result = "[P]" } },
                iterations = 3,
                angle = 30f,
                initialLength = 0.2f,
                lengthModifier = 0.8f,
                initialRadius = 0.05f,
                radiusModifier = 0.9f,
                leafSize = 1.5f,
                leafInfo = new LeafInfo { length = 0.2f, width = 0.2f, lengthBend = 30f, widthFold = 20f },
                petalCount = 8,
                petalSplayAngle = 60f
            }
        };
    }
}


//editor script for thing generator :)
[CustomEditor(typeof(ThingGen))]
public class ThingGenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var generator = (ThingGen)target;
        base.OnInspectorGUI();
        EditorGUI.BeginChangeCheck();
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            generator.LoadSettings();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Reload Settings", GUILayout.Height(30)))
        {
            generator.LoadSettings();
        }

        if (GUILayout.Button("Generate", GUILayout.Height(40)))
        {
            generator.Generate();
        }
    }
}