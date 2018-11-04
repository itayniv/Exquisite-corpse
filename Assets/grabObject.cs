using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grabObject : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        Debug.Log("starting");
    }


    void Update()
    {
        //Debug.Log(transform.position);
    }



    void OnCollisionEnter(Collision col)
    {
        Debug.Log("boooom");
    }

}
