using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformTracker : MonoBehaviour {


    public Transform fromTarget;

    public int YRotationOffset;
    public int XRotationOffset;

    [Range(-1.0F, 1.0F)]
    public float ZPositionOffset;


    [Range(-1.0F, 1.0F)]
    public float XPositionOffset;

    private void Update()
    {
        if (fromTarget == null) return;

        transform.position = fromTarget.position + new Vector3(XPositionOffset, 0, ZPositionOffset);
        transform.rotation = fromTarget.rotation * Quaternion.Euler(Vector3.up * YRotationOffset) * Quaternion.Euler(Vector3.left * XRotationOffset);
    }

}
