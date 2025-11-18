using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    Transform playerTransform;


    void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        

    }

    void LateUpdate()
    {
        transform.position = playerTransform.position;
       
    }
}
