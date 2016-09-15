using UnityEngine;
using System.Collections;

public sealed class MouseControl : MonoBehaviour
{
    [Range(5, 150)]
    public float zoomSensitivity = 50;
    const float BASE_ZOOM_SENSITIVITY = 100;

    Vector3 startPosition;
    Vector3 origin;
    Vector3 offset;
    bool dragging = false;

    void Start()
    {
        startPosition = Camera.main.transform.position;
    }
    void LateUpdate()
    {
        // scrolling background with middle wheel
        if (Input.GetMouseButton(2))
        {
            offset = (Camera.main.ScreenToWorldPoint(Input.mousePosition)) - Camera.main.transform.position;
            if (!dragging)
            {
                dragging = true;
                origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }
        else
        {
            dragging = false;
        }
        if (dragging)
        {
            Camera.main.transform.position = origin - offset;
        }

        // reset to origin with right button
        //if (Input.GetMouseButton(1))
        //{
        //    Camera.main.transform.position = startPosition;
        //}

        // zoom with mousewheel
        Camera.main.orthographicSize -= Input.mouseScrollDelta.y * zoomSensitivity / BASE_ZOOM_SENSITIVITY;
    }
}
