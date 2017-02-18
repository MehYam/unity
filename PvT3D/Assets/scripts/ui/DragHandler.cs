using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Vector3 dragStart;
    Transform dragStartParent;


    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStart = transform.position;
        dragStartParent = transform.parent;

        transform.SetParent(transform.parent.parent.parent); //KAI:

        Debug.Log("OnBeginDrag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        //KAI: this right?  what about eventData?
        transform.position = Input.mousePosition;

        Debug.Log("OnDrag");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(dragStartParent);

        transform.position = dragStart;
    }
}
