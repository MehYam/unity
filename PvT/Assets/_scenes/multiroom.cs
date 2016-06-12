using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public sealed class multiroom : MonoBehaviour
{
    public GameObject room;
    public GameObject wall;

	// Use this for initialization
	void Start () {

        var thisRoom = GameObject.Instantiate(room);

        // loop all the meshes in the level, take the bounds of the largest as our world bounds
        var meshes = thisRoom.GetComponentsInChildren<MeshRenderer>();
        MeshRenderer largestMesh = null;
        foreach (var mesh in meshes)
        {
            if (largestMesh == null || mesh.bounds.size.sqrMagnitude > largestMesh.bounds.size.sqrMagnitude)
            {
                largestMesh = mesh;
            }
        }

        Assert.AreNotEqual(largestMesh, null);

        // center the map in the world
        thisRoom.transform.position = -largestMesh.bounds.center;
    }
}
