using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickAndDragCam : MonoBehaviour
{
    public float rotateSpeed = 3.5f;
    private float X;
    private float Y;
    public float zoomSpeed = 0.05f;

    private Vector3 resetPosition;
    private Quaternion resetRotation;

    private void Start()
    {
        resetPosition = transform.position;
        resetRotation = transform.rotation;
    }

    void Update() {
        if(Input.GetMouseButton(0)) {
            transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * rotateSpeed, -Input.GetAxis("Mouse X") * rotateSpeed, 0));
            X = transform.rotation.eulerAngles.x;
            Y = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(X, Y, 0);
        }
        if(Input.GetKey(KeyCode.W)){
            transform.position = transform.position + (transform.forward * zoomSpeed);
            print("zooming");
            }
            if (Input.GetKey(KeyCode.S)){
            transform.position = transform.position - (transform.forward * zoomSpeed);
            print("zooming out");

       }

        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = resetPosition;
            transform.rotation = resetRotation;
        }

    }
}
