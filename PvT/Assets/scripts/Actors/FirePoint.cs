using UnityEngine;
using System.Collections;

public sealed class FirePoint : MonoBehaviour
{
    public enum TYPE { LASER, ROCKET, SHIELD, BULLET };
    public TYPE type;
}
