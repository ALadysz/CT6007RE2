using UnityEngine;

//info for generating leaves
[System.Serializable]
public class LeafInfo
{
    [Header("Shape")]
    [Range(0.1f, 2f)] public float length = 0.5f;
    [Range(0.01f, 2f)] public float width = 0.3f;
    [Range(0.01f, 1f)] public float tipSharpness = 0.5f;
    [Range(0.01f, 1f)] public float baseSharpness = 0.5f;
    [Range(0.01f, 0.49f)] public float baseHeight = 0.2f;
    [Range(0.51f, 0.99f)] public float tipHeight = 0.8f;

    [Header("Bend")]
    [Range(0f, 45f)] public float lengthBend = 15f;
    [Range(0f, 45f)] public float widthFold = 10f;

    //makes a copy
    public LeafInfo Clone()
    {
        return (LeafInfo)this.MemberwiseClone();
    }
}