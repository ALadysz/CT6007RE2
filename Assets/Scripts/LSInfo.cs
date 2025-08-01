using UnityEngine;

//info for generation
[System.Serializable]
public class LSInfo
{
    [Header("L-System Rules")]
    public string axiom = "X";
    public Rule[] rules;

    [Header("Gen Settings")]
    [Range(1, 10)] public int iterations = 4;
    [Range(3, 16)] public int sides = 8;

    [Header("Branch Stuff")]
    [Range(0f, 90f)] public float angle = 30f;
    [Range(0.01f, 2f)] public float initialLength = 0.5f;
    [Range(0.5f, 1f)] public float lengthModifier = 0.9f;
    [Range(0.01f, 0.5f)] public float initialRadius = 0.1f;
    [Range(0.5f, 1f)] public float radiusModifier = 0.9f;
    [Range(0.001f, 0.1f)] public float minRadius = 0.01f;
    [Range(0f, 45f)] public float branchDirectionRandomness = 5f;

    [Header("Leaves")]
    public bool generateLeaves = true;
    [Range(0.1f, 5f)] public float leafSize = 1f;
    public LeafInfo leafInfo;

    [Header("Flower Specific")]
    public int petalCount = 8;
    public float petalSplayAngle = 60f;
    public Vector3 flowerPositionOffset = Vector3.zero; //offset was because of previous debugging - manually adjusting so the flower would spawn correctly

    [System.Serializable]
    public struct Rule
    {
        public char character;
        public string result;
    }

    //makes a copy of lsInfo - making sure original data isnt modified just the copy
    public LSInfo Clone()
    {
        return new LSInfo
        {
            axiom = this.axiom,
            rules = (Rule[])this.rules.Clone(),
            sides = this.sides,
            angle = this.angle,
            initialLength = this.initialLength,
            lengthModifier = this.lengthModifier,
            initialRadius = this.initialRadius,
            radiusModifier = this.radiusModifier,
            minRadius = this.minRadius,
            branchDirectionRandomness = this.branchDirectionRandomness,
            generateLeaves = this.generateLeaves,
            leafSize = this.leafSize,
            leafInfo = this.leafInfo?.Clone()
        };
    }
}