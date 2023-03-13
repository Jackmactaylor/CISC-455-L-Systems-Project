using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Class is meant to slowly rotate the camera around a rotate position
public class CameraController : MonoBehaviour
{
    
    public Transform rotatePosition;
    //Speed of rotation
    public float speed = 20f;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate the camera around the rotate position
        transform.RotateAround(rotatePosition.position, Vector3.up, speed * Time.deltaTime);
    }
}
