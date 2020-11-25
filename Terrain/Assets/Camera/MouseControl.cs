using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Adapted from https://answers.unity.com/questions/29741/mouse-look-script.html
Credits to IJM*/

public class MouseControl : MonoBehaviour
{

    public float sensitivityX = 15F;
    public float sensitivityY = 15F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -900F;
    public float maximumY = 90F;
    float rotationX = 0F;
    float rotationY = 0F;
    Quaternion originalRotation;
    
    void Update ()
    {
            //Get mouse position
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            //Clamp angle to ensure within boundry
            rotationX = ClampAngle (rotationX, minimumX, maximumX);
            rotationY = ClampAngle (rotationY, minimumY, maximumY);
            //Calculate appropriate quaternion
            Quaternion xQuaternion = Quaternion.AngleAxis (rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis (rotationY, -Vector3.right);
            //Apply quaternion to local transform
            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
    }

    void Start ()
    {
        //Set original rotation and lock mouse
        originalRotation = transform.localRotation;
        Cursor.lockState = CursorLockMode.Locked;
    }

    //Ensures angle is within given min and max
    public static float ClampAngle (float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F)) {
            if (angle < -360F) {
                angle += 360F;
            }
            if (angle > 360F) {
                angle -= 360F;
            }
        }
        return Mathf.Clamp (angle, min, max);
    }
}
