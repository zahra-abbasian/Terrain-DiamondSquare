using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingVehicleMovement : MonoBehaviour {
    
    public Rigidbody rb;
    public float Speed = 5f; //regular speed
    float shiftAdd = 2.0f; //Add to speed
    Vector3 speed =new Vector3(0,0,0);

    void Start () {
        rb = GetComponent<Rigidbody>();
    }

    void Update () {
        //Get the keys being pressed
        Vector3 speed = GetBaseInput();
        
        //Add extra acceleration
        if (Input.GetKey (KeyCode.LeftShift)){
            speed = speed * shiftAdd * Speed;
        }
        else{
            speed = speed * Speed;
        }
        
        //Create vector that will change position
        Vector3 posChange=speed*Time.deltaTime;
        this.transform.localPosition+=posChange;
       
    }
     
     //Returns the vector of buttons being pressed with respect to rotation
    private Vector3 GetBaseInput() { 
        Vector3 p_Velocity = new Vector3(0,0,0);
        if (Input.GetKey (KeyCode.W)){
            p_Velocity += transform.forward;
        }
        if (Input.GetKey (KeyCode.S)){
            p_Velocity -= transform.forward;
        }
        if (Input.GetKey (KeyCode.A)){
            p_Velocity -= transform.right;
        }
        if (Input.GetKey (KeyCode.D)){
            p_Velocity += transform.right   ;
        }
        return p_Velocity;
    }

    //Set velocity to zero when you exit collision to avoid drifting, Only want movement from keys
    void OnCollisionExit(Collision other)
    {
        rb.velocity=Vector3.zero;
    }
}
