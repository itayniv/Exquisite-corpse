using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grabObject : MonoBehaviour
{


    public GameObject instantiateLeftArm;
    public GameObject instantiateRightArm;
    public GameObject instantiateLeftLeg;
    public GameObject instantiateRightLeg;
    public GameObject instantiateHead;
    public GameObject instantiateTorso;




    // Use this for initialization
    void Start()
    {
        Debug.Log("starting");
    }


    void Update()
    {
        //Debug.Log(transform.position);
    }



    void OnCollisionEnter(Collision collisionObject)
    {
        Debug.Log("boooom " + collisionObject.gameObject);

        //GameObject collisioned = collisionObject.gameObject;
        //Debug.Log(collisioned + "name");


        if (collisionObject.gameObject.CompareTag("RIG_SOFIA_HEAD"))
        {
            Debug.Log("Collision with Sofia Head");
            Vector3 collisionPosition = transform.position;
            collisionPosition = collisionObject.transform.position;
            Instantiate(instantiateHead, collisionPosition, Quaternion.identity);

        }
        else if (collisionObject.gameObject.CompareTag("RIG_SOFIA_RIGHTARM"))
        {
            Debug.Log("Collision with Sofia Head");
            Vector3 collisionPosition = transform.position;
            collisionPosition = collisionObject.transform.position;
            Instantiate(instantiateRightArm, collisionPosition, Quaternion.identity);

        }
        else if (collisionObject.gameObject.CompareTag("RIG_SOFIA_LEFTARM"))
        {
            Debug.Log("Collision with Sofia Head");
            Vector3 collisionPosition = transform.position;
            collisionPosition = collisionObject.transform.position;
            Instantiate(instantiateRightArm, collisionPosition, Quaternion.identity);

        }
        else if (collisionObject.gameObject.CompareTag("RIG_SOFIA_CORE"))
        {
            Debug.Log("Collision with Sofia Head");
            Vector3 collisionPosition = transform.position;
            collisionPosition = collisionObject.transform.position;
            Instantiate(instantiateTorso, collisionPosition, Quaternion.identity);

        }
        else if (collisionObject.gameObject.CompareTag("RIG_SOFIA_RIGHTLEG"))
        {
            Debug.Log("Collision with Sofia Head");
            Vector3 collisionPosition = transform.position;
            collisionPosition = collisionObject.transform.position;
            Instantiate(instantiateRightLeg, collisionPosition, Quaternion.identity);

        }
        else if (collisionObject.gameObject.CompareTag("RIG_SOFIA_LEFTLEG"))
        {
            Debug.Log("Collision with Sofia Head");
            Vector3 collisionPosition = transform.position;
            collisionPosition = collisionObject.transform.position;
            Instantiate(instantiateRightLeg, collisionPosition, Quaternion.identity);

        }

    }
}