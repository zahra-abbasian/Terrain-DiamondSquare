using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPosition : MonoBehaviour
{
    public Vector3 originalPosition=Vector3.zero;
    public Quaternion originalRotation=Quaternion.identity;
    // Start is called before the first frame update
    void Start()
    {
        GameObject terrain = GameObject.Find("Terrain");
		var cs = terrain.GetComponent<MyDs>();
		float length=cs.terrain_size;
        float height=cs.range;
        //Set transform based on terrain
        originalPosition.x=length/2;
        originalPosition.z=length/2;
        originalPosition.y=height+100;
        this.transform.position=originalPosition;
        //Set rotation to original
        originalRotation = this.transform.rotation;
    }  

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey (KeyCode.Space)){
            this.transform.position=originalPosition;
            this.transform.rotation=originalRotation;
            
        }
    }
}
