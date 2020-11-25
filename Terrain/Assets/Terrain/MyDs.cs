using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

using Random = UnityEngine.Random;

public class MyDs : MonoBehaviour {
    private float[,] square;
    [SerializeField] private int num_vertices;
    [SerializeField] private int total_num_of_vertices;
    [SerializeField] private int total_num_of_triangles;
    //n for diamond square
    [Range(1,20)] public int n = 1;
    //Size of grid for diamond square
    private int size;
    //Top range for terrain generation
    public float range=80;
    //Number of triangles along each axis for terrain
    private int triangle_no=125;
    //Parameters for intial roughness and how much it reduces by each time
    [Range(0,2)] public float roughness = 0.7f;
    [Range(0,1)] public float reduction= 0.7f;
    private float max_height;
    //Length of terrain
    [Range(0,1000)] public float terrain_size = 100f;

    private MeshFilter terrain;
    //Random used to generate height values
    private Random seed = new Random();

    //introduce lighting parameters
    public PointLight pointLight;
    public Shader shader;
    
    // Start is called before the first frame update
    void Start(){
        //Determine size
        size=(int)Mathf.Pow(2,n)+1;
        //Set max_height for randomness
        max_height=roughness;
        terrain = GetComponent<MeshFilter>();
        //Generate heights
        square=CreateDiamondSquare();
        terrain.mesh = createMesh(square);

        MeshRenderer terrain_renderer = GetComponent<MeshRenderer>();
        //Set mesh for collisions
        GetComponent<MeshCollider>().sharedMesh = null;
        GetComponent<MeshCollider>().sharedMesh = terrain.mesh;


    
    }

    // Update is called once per frame
    void Update() {
        //Reset terrain when space is pressed
        if (Input.GetKey("space")){
            max_height = roughness;
			square=CreateDiamondSquare();
            terrain.mesh = createMesh(square);
            //Set mesh for collisions
            GetComponent<MeshCollider>().sharedMesh = null;
            GetComponent<MeshCollider>().sharedMesh = terrain.mesh;

            
		}
        MeshRenderer terrain_renderer = GetComponent<MeshRenderer>();

        terrain_renderer.material.shader = shader;

        // Pass updated light positions to shader
        terrain_renderer.material.SetColor("_PointLightColor", this.pointLight.color);
        terrain_renderer.material.SetVector("_PointLightPosition", this.pointLight.GetWorldPosition());
    }

    //Intialise square for first diamond step.
    float[,] initSquare(){
        square = new float[size,size];    
        //Set values to zero
        for (int z = 0; z < size; z++){  
            for (int x = 0; x < size; x++){
                square[x,z] = 0f;
            }
        }

        //setting random corner values
        square[0,0] = getPRandomHeight(max_height);
        square[0, size-1] = getPRandomHeight(max_height);
        square[size-1, 0] = getPRandomHeight(max_height);
        square[size-1, size-1] = getPRandomHeight(max_height);

        //Reduce each iteration
        max_height *= reduction;
        return square;
    }


    //Generate the mesh using heightmap and set parameters
    //WARNING:Setting num_triangles too high will results in bad display due to limits on number of vertices
    Mesh createMesh(float[,] square){
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
 
        //Create triangles
        for (int i=0;i<triangle_no;i++){
            for (int j=0;j<triangle_no;j++){
                create_triangles(i,j,square,vertices,triangles);
            }   
        }
        // Creating the mesh
        Mesh terrain = new Mesh();
        terrain.vertices = vertices.ToArray();
        terrain.triangles = triangles.ToArray();

        
        //Calculate vertex normals
        terrain.RecalculateBounds();
        terrain.RecalculateNormals();

        terrain.name = "DiamondSquare Terrain";

        total_num_of_vertices = terrain.vertices.Length;
        total_num_of_triangles = terrain.triangles.Length/3;


        return terrain;
    }

    //Add triangles and vertices to respective arrays
    void create_triangles(int i,int j,float[,] square,List<Vector3> vertices,List<int> triangles){
        float x1=j*(terrain_size/triangle_no);
        float x2=(j+1)*(terrain_size/triangle_no);
        float z1=i*(terrain_size/triangle_no);
        float z2=(i+1)*(terrain_size/triangle_no);
        float y1=getY(x1,z1,square);
        float y2=getY(x2,z1,square);
        float y3=getY(x1,z2,square);
        float y4=getY(x2,z2,square);
        vertices.Add(new Vector3(x1,y1,z1));
        vertices.Add(new Vector3(x2,y2,z1));
        vertices.Add(new Vector3(x1,y3,z2));
        vertices.Add(new Vector3(x2,y4,z2));
        triangles.Add((4*i*triangle_no)+(4*j)+2);
        triangles.Add((4*i*triangle_no)+(4*j)+1);
        triangles.Add((4*i*triangle_no)+(4*j));
        triangles.Add((4*i*triangle_no)+(4*j)+2);
        triangles.Add((4*i*triangle_no)+(4*j)+3);
        triangles.Add((4*i*triangle_no)+(4*j)+1);
    }


    //Get y co-ordinate by taking the average of the heights around the point
    float getY(float x, float z, float[,] square){
        float relx=((x/terrain_size)*(size-1));
        float relz=((z/terrain_size)*(size-1));
        float percentx=relx-(float)Math.Truncate(relx);
        float percenty=relz-(float)Math.Truncate(relz);
        int upperx=(int)Mathf.Ceil(relx);
        int lowerx=(int)Mathf.Floor(relx);
        int upperz=(int)Mathf.Ceil(relz);
        int lowerz=(int)Mathf.Floor(relz);
        float topleft=square[lowerx,lowerz];
        float topright=square[upperx,lowerz];
        float bottomleft=square[lowerx,upperz];
        float bottomright=square[upperx,upperz];
        float y=(topleft+topright+bottomleft+bottomright)/4;
        if (y>0){
            return y*range;
        }
        else{
            return 0;
        }
    }

    //Produce a random height with mean 0
	float getRandomHeight(float max_height){
		return Random.Range(-max_height, max_height);
	}

    //Produce a random height that's slightly skewed positive to encourage terrain formation
    float getPRandomHeight(float max_height){
		return Random.Range(-max_height*0.5f, max_height);
	}

    //Diamond Step for diamond square
    List<List<int>> diamondStep (List<List<int>> squarePoints,float[,] square){
        List<List<int>> diamondPoints=new List<List<int>>();
        for (int i=0;i<squarePoints.Count;i++){
            //Add new diamond point
            diamondPoints.Add(
                new List<int>(){((squarePoints[i][0]+squarePoints[i][2])/2)
                ,((squarePoints[i][1] + squarePoints[i][5])/2)});

            //Set value for point
            square[diamondPoints[diamondPoints.Count-1][0],diamondPoints[diamondPoints.Count-1][1]]
            =(float)((
            square[squarePoints[i][0],squarePoints[i][1]] +
            square[squarePoints[i][2],squarePoints[i][3]] +
            square[squarePoints[i][4],squarePoints[i][5]] +
            square[squarePoints[i][6],squarePoints[i][7]]) / 4.0f) + getRandomHeight(max_height);
        }
        return diamondPoints;
    }

    //Square Step for diamond square
    List<List<int>> squareStep(List<List<int>> diamondPoints, float[,] square,int dist){
        var toCalcPoints=new List<List<int>>();
        var newSquarePoints=new List<List<int>>();
        int added=0;
        //Calculate square points
        for (int i=0;i<diamondPoints.Count;i++){
            toCalcPoints=getPoints(diamondPoints[i],dist);
            added+=calcPoints(toCalcPoints,square,dist);
        }
        
        //Calculate number of points for diamond step
        int numSquares=(size-1)/dist;
        //New squares for diamond step
        for (int j=0;j<numSquares;j++){
            for (int i=0;i<numSquares;i++){
                newSquarePoints.Add(new List<int>(){
                    i*dist,j*dist,
                    (i+1)*dist,(j)*dist,
                    (i)*dist,(j+1)*dist,
                    (i+1)*dist,(j+1)*dist});
            }
        }
        return newSquarePoints;
    }

    //Get the points around a diamond point
    List<List<int>> getPoints(List<int> diamondPoint,int dist){
        List<List<int>> points=new List<List<int>>();
        points.Add(new List<int>(){diamondPoint[0],diamondPoint[1]+dist});
        points.Add(new List<int>(){diamondPoint[0]-dist,diamondPoint[1]});
        points.Add(new List<int>(){diamondPoint[0]+dist,diamondPoint[1]});
        points.Add(new List<int>(){diamondPoint[0],diamondPoint[1]-dist});
        return points;
    }

    //Determine the value for each point 
    int calcPoints(List<List<int>> points,float[,] square,int dist){
        int numCalc=0;
        for (int i=0;i<points.Count;i++){
            calcPoint(points[i],square,dist);
        }
        return numCalc;
    }

    //Point value is the average of the points around it
    void calcPoint (List<int> point,float[,] square,int dist){
        int numadded=0;
        float total=0;
        //If statements to avoid null points
        if (point[0]+dist<size){
            total+=square[point[0]+dist,point[1]];
            numadded++;
        }
        if (point[0]-dist>=0){
            total+=square[point[0]-dist,point[1]];
            numadded++;
        }
        if (point[1]+dist<size){
            total+=square[point[0],point[1]+dist];
            numadded++;
        }
        if (point[1]-dist>=0){
            total+=square[point[0],point[1]-dist];
            numadded++;
        }
        square[point[0],point[1]]=(total/(float)numadded)+getRandomHeight(max_height);

    }


    //Driver function for algorithm
    float[,] CreateDiamondSquare() {
        square=initSquare();      // initialize four corners with random height
        List<List<int>> squarePoints=new List<List<int>>();
        //Need initial square points
        squarePoints=initializeList();
        List<List<int>> diamondPoints=new List<List<int>>();
        int num_steps=2*n;
        bool diamond=true;
        for(int i=0;i<num_steps;i++){
            if (diamond){
                diamondPoints=diamondStep(squarePoints,square);
                diamond=false;
            }
            else{
                //Distance is equal to the distance the first diamond point is from the edge
                int dist=diamondPoints[0][0];
                squarePoints=squareStep(diamondPoints,square,dist);
                diamond=true;
            }
            //Reduce max_height after each iteration
            max_height *= reduction;
        }
        /*for (int i=0;i<size;i++){
            for(int j=0;j<size;j++){
                Debug.Log("SquareXY:"+i+" "+j+" "+square[i,j]);
            }
        }*/
        return square;
    }

    List<List<int>> initializeList(){
        var square=new List<List<int>>(){ 
            new List<int>(){0,0,size-1,0,0,size-1,size-1,size-1}
            };
        return square;
    }
}