using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform rotatePosition;
    public float rotationSpeed = 20f;
    public float moveSpeed = 25f;
    public float minDistance = 10f;
    public float maxDistance = 250f;

    public bool isAutoRotateEnabled = false;
    
    private float currentMoveSpeed;
    private float currentRotationSpeed;

    void Update()
    {
        currentMoveSpeed = moveSpeed;
        currentRotationSpeed = rotationSpeed;
        
        // Check if Shift key is pressed to double the move speed
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentMoveSpeed *= 2f;
        }

        if (isAutoRotateEnabled)
        {
            // Rotate the camera around the rotate position
            transform.RotateAround(rotatePosition.position, Vector3.up, currentRotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.RotateAround(rotatePosition.position, Vector3.up, currentRotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.RotateAround(rotatePosition.position, Vector3.down, currentRotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            Vector3 direction = transform.position - rotatePosition.position;
            float distance = direction.magnitude;
            if (distance > minDistance)
            {
                transform.Translate(Vector3.forward * currentMoveSpeed * Time.deltaTime, Space.Self);
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            Vector3 direction = transform.position - rotatePosition.position;
            float distance = direction.magnitude;
            if (distance < maxDistance)
            {
                transform.Translate(Vector3.back * currentMoveSpeed * Time.deltaTime, Space.Self);
            }
        }
    }
    
    public void ToggleAutoRotate()
    {
        isAutoRotateEnabled = !isAutoRotateEnabled;
    }
    
}