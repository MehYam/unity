using UnityEngine;
using System.Collections;

public class MuzzleFlash : MonoBehaviour
{
    void OnFlashComplete()
    {
        GetComponent<Animator>().enabled = false;
    }
}
