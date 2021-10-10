using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowObject : MonoBehaviour
{
    public Transform targetTransform;

    // Update is called once per frame
    void Update()
    {
        this.transform.position = targetTransform.position;
    }
}
