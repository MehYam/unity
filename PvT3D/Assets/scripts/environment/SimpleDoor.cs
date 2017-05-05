using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SimpleDoor : MonoBehaviour
{
    [SerializeField] float time = 1;
    [SerializeField] GameObject left = null;
    [SerializeField] GameObject right = null;

    struct SavedPositions
    {
        public readonly float scaleX;
        public readonly float localX;
        public SavedPositions(Transform transform) { this.scaleX = transform.localScale.x; this.localX = transform.localPosition.x; }
    }
    SavedPositions savedLeft;
    SavedPositions savedRight;
    void Start()
    {
        savedLeft = new SavedPositions(left.transform);
        savedRight = new SavedPositions(right.transform);
    }
    void OnCollisionEnter(Collision col)
    {
        //var otherActor = col.collider.GetComponent<Actor>();
        Open();
    }

    bool open = false;
    public void Open()
    {
        if (!locked && !open)
        {
            LeanTween.scaleX(left, 0.1f, time).setEaseOutElastic();
            LeanTween.scaleX(right, 0.1f, time).setEaseOutElastic();
            LeanTween.moveLocalX(left, -9, time).setEaseOutElastic();
            LeanTween.moveLocalX(right, 9, time).setEaseOutElastic();

            open = true;
        }
    }
    public void Close()
    {
        if (!locked && open)
        {
            LeanTween.scaleX(left, savedLeft.scaleX, time);
            LeanTween.scaleX(right, savedRight.scaleX, time);
            LeanTween.moveLocalX(left, savedLeft.localX, time);
            LeanTween.moveLocalX(right, savedRight.localX, time);
        }
    }
    public bool locked { get; set; }
}
