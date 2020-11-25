using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water : MonoBehaviour {

	private MeshFilter water;
	private int triangle_no=125;
	public float level=15.0f;
    public float water_size;

    // Introduce lighting variables
    public PointLight pointLight;
    public Shader shader;
    public Texture texture;



    Mesh generateWaterMesh(float size){
		List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
		List<Vector2> uv = new List<Vector2>();
 
        //Create triangles
        for (int i=0;i<triangle_no;i++){
            for (int j=0;j<triangle_no;j++){
                create_triangles(i,j,size,vertices,triangles,uv);
            }   
        }
        // Creating the mesh
        Mesh water = new Mesh();
        water.vertices = vertices.ToArray();
        water.triangles = triangles.ToArray();
		water.uv = uv.ToArray();
		water.RecalculateNormals();
        water.name = "Water";

        return water;
	}

	void create_triangles(int i,int j,float size,List<Vector3> vertices,List<int> triangles, List<Vector2> uv){
        float x1=j*(size/triangle_no);
        float x2=(j+1)*(size/triangle_no);
        float z1=i*(size/triangle_no);
        float z2=(i+1)*(size/triangle_no);
        float y=level;
		uv.Add(new Vector2(x1,z1));
		uv.Add(new Vector2(x2,z1));
		uv.Add(new Vector2(x1,z2));
		uv.Add(new Vector2(x2,z2));
        vertices.Add(new Vector3(x1,y,z1));
        vertices.Add(new Vector3(x2,y,z1));
        vertices.Add(new Vector3(x1,y,z2));
        vertices.Add(new Vector3(x2,y,z2));
        triangles.Add((4*i*triangle_no)+(4*j)+2);
        triangles.Add((4*i*triangle_no)+(4*j)+1);
        triangles.Add((4*i*triangle_no)+(4*j));
        triangles.Add((4*i*triangle_no)+(4*j)+2);
        triangles.Add((4*i*triangle_no)+(4*j)+3);
        triangles.Add((4*i*triangle_no)+(4*j)+1);
    }

    void setBoundaries() {

        foreach (Transform boundary in this.transform) {
            
            boundary.localScale = new Vector3(100f, 1f, 100f);
            float y_displacement = 75f;

            if (boundary.name == "EastBoundary"){
                boundary.localPosition = new Vector3(water_size/2, y_displacement, water_size);
                boundary.localRotation = Quaternion.Euler(-90, 0, 0);
            }

            if (boundary.name == "WestBoundary"){
                boundary.localPosition = new Vector3(water_size/2, y_displacement, 0f);
                boundary.localRotation = Quaternion.Euler(90, 0, 0);
            }

            if (boundary.name == "NorthBoundary"){
                boundary.localPosition = new Vector3(0f, y_displacement, water_size/2);
                boundary.localRotation = Quaternion.Euler(0, 0, -90);
            }

            if (boundary.name == "SouthBoundary"){
                boundary.localPosition = new Vector3(water_size, y_displacement, water_size/2);
                boundary.localRotation = Quaternion.Euler(0, 0, 90);
            }

            if (boundary.name == "TopBoundary"){
                boundary.localPosition = new Vector3(water_size/2, water_size/2, water_size/2);
                boundary.localRotation = Quaternion.Euler(0, 0, 180);
            }

            Mesh boundary_mesh = boundary.GetComponent<MeshFilter>().mesh;
            boundary_mesh.RecalculateNormals();
            boundary.GetComponent<MeshCollider>().sharedMesh = boundary_mesh;

            // MeshRenderer terrain_renderer = GetComponent<MeshRenderer>();
        }        

    }

	// Use this for initialization
	void Start () {
		GameObject terrain = GameObject.Find("Terrain");
		var cs = terrain.GetComponent<MyDs>();

		water_size = cs.terrain_size;
		
        water = GetComponent<MeshFilter>();
		water.mesh=generateWaterMesh(water_size);

        this.transform.localPosition = new Vector3(0f, 0f, 0f);

        setBoundaries();

        MeshRenderer terrain_renderer = GetComponent<MeshRenderer>();
		//Add collision with water
        GetComponent<MeshCollider>().sharedMesh = water.mesh;
	}
	
	// Update is called once per frame
	void Update () {
        water.mesh.RecalculateNormals();

        MeshRenderer terrain_renderer = GetComponent<MeshRenderer>();

		terrain_renderer.material.shader = shader;
        terrain_renderer.material.mainTexture = texture;

        // Pass updated light positions to shader
        terrain_renderer.material.SetColor("_PointLightColor", this.pointLight.color);
        terrain_renderer.material.SetVector("_PointLightPosition", this.pointLight.GetWorldPosition());
		
	}
}