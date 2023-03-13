using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script that handles the movement of the sun across the sky. Is attached to the directional light in the scene.
public class SunMovement : MonoBehaviour
{
    
    //Speed of rotation
    public float speed = 20f;
    //Transform to rotate around
    public Transform rotatePosition;
    
    // Update is called once per frame
    void Update()
    {
        //Rotate the sun around the rotate position
        transform.RotateAround(rotatePosition.position, Vector3.right, speed * Time.deltaTime);
        //Make the directional lights rotation point at the rotate position
        transform.LookAt(rotatePosition);
    }
}
