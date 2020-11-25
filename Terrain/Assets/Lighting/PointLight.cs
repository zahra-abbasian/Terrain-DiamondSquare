using UnityEngine;
using System.Collections;

public class PointLight : MonoBehaviour {

    public Color color;
    public GameObject terrain;
    public float SunSpeed;

    

    public Vector3 GetWorldPosition()
    {
        return this.transform.position;
    } 

     void Start()
    {
		//Receive terrain size
		float size = terrain.GetComponent<MyDs>().terrain_size;
       //Define the initial position of point light
        this.transform.position = new Vector3(0f, size, size/2);
        //Set a scale for point light
        this.transform.localScale = new Vector3(size/30, size/30, size/30);
        
    }

    // Update is called once per frame
    void Update()
    {
        
        float size = terrain.GetComponent<MyDs>().terrain_size;
        //Rotate around z axis
        this.transform.RotateAround(new Vector3(size/2, 0f, 0f), Vector3.forward, SunSpeed * Time.deltaTime);

    }
}