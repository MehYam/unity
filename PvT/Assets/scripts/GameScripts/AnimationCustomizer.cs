using UnityEngine;
using System.Collections;

public sealed class AnimationCustomizer : MonoBehaviour
{
	// Use this for initialization
    public float speed = 1;
	void Start()
    {
        var animator = GetComponent<Animator>();

        animator.speed = speed;
        var start = Random.Range(0f, 1f);
        animator.Play("Hero", 0, start);
    }
}
