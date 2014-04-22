using UnityEngine;
using System.Collections;

public sealed class CameraFollow : MonoBehaviour
{
    public GameObject target;

    float _startZ;
    void Start()
    {
        _startZ = camera.transform.localPosition.z;
    }

	// Update is called once per frame
	void Update()
    {
        camera.transform.localPosition = new Vector3(target.transform.localPosition.x, target.transform.localPosition.y, _startZ);
	}
}
