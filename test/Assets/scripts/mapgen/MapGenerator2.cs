using UnityEngine;
using System.Collections;

public sealed class MapGenerator2 : MonoBehaviour
{
    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;


}
