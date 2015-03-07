using UnityEngine;
using System.Collections;

public class testcollider : MonoBehaviour
{
    void Awake()
    {
        //Debug.Log(GetComponent<Rigidbody2D>());
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D");
    }
    void OnTriggerStay2D(Collider2D other)
    {
        //Debug.Log("OnTriggerStay2D");

        other.GetComponent<Rigidbody2D>().AddForce(Vector2.right * 5);
    }
    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("OnTriggerExit2D");
    }
}
