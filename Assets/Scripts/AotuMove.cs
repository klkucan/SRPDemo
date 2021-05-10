using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AotuMove : MonoBehaviour
{
    Vector3 origin;
    // Start is called before the first frame update
    void Start()
    {
        origin = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.position = origin;
        gameObject.transform.position += Random.insideUnitSphere;
    }
}
