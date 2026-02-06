using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class circler : MonoBehaviour
{
    public float w = 1f;
    public float r = 1f;
    public float z = 0f;
    public float t1;
    private float t0 = 0;
    // Start is called before the first frame update
    void Start()
    {
        t0 = Time.time;
        transform.position = new Vector3(r, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        float t = Time.time-t0+t1;
        transform.position = new Vector3(r*Mathf.Cos(t * w), z, r * Mathf.Sin(t * w));

    }
}
