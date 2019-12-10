using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSimulator : MonoBehaviour
{

    public float cycleTime = 1f;
    public float waveLength = 0.15f;
    private float timer;

    public bool randomSplashes;
    public float averageRandomSplashesPerSecond;

    // Start is called before the first frame update
    void Start()
    {
        /*gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshFilter>();
        GetComponent<MeshFilter>().mesh = GenerateMesh();*/
        timer = Time.time;
        gameObject.AddComponent<MeshCollider>();
        GetComponent<MeshCollider>().convex = true;


        /*
        for(int i = 0; i < GetComponent<MeshFilter>().mesh.vertices.Length; i += 1)
        {
            //GetComponent<MeshFilter>().mesh.vertices[i].x += 0.5f;
            //GetComponent<MeshFilter>().mesh.vertices[i].z += 0.5f;
            print(GetComponent<MeshFilter>().mesh.vertices[i].ToString());
        }*/

    }

    // Update is called once per frame
    void Update()
    {
        float timePassed = Time.time % cycleTime;
        float sinOffset = (timePassed * Mathf.PI * 2f); //the width of the mesh that the sin passed through this frame

        AdjustMesh(sinOffset);

        if (randomSplashes){
            HandlePoissonSplashOccurances();
        }
        else{
            if (Input.anyKeyDown) MakeRandomSplash();
        }

        /*
        if (Input.GetKeyDown(KeyCode.S))
        {
            GameObject splash = new GameObject();
            splash.AddComponent<Splash>();
            splash.GetComponent<Splash>().water = gameObject;
        }*/

    }

    private void HandlePoissonSplashOccurances()
    {
        float expectedSplashesSinceLastFrame = averageRandomSplashesPerSecond * Time.deltaTime;
        float probabilityOfSplashThisFrame = expectedSplashesSinceLastFrame * Mathf.Exp(-1f * expectedSplashesSinceLastFrame); //poisson probability of one splash occuring
        if (Random.value < probabilityOfSplashThisFrame)
        {
            MakeRandomSplash();
        }
    }

    private void AdjustMesh(float offset)
    {
        Vector3[] verts = GetComponent<MeshFilter>().mesh.vertices;
        for(int i = 0; i < verts.Length; i += 1)
        {
            verts[i].y = 0;

            float xProportion = (verts[i].x / waveLength);
            float rawSinInput = xProportion * 2f * Mathf.PI;


            verts[i].y += (Mathf.Sin(rawSinInput + offset)) / (5f * (0.5f/waveLength));
           
        }
        GetComponent<MeshFilter>().mesh.vertices = verts;
    }

    public Mesh GenerateMesh()
    {
        float stepSize = 0.01f;

        int meshWidth = Mathf.CeilToInt(1f / (float)stepSize);
        int meshHeight = Mathf.CeilToInt(1f / (float)stepSize);

        List<Vector3> vertexList = new List<Vector3>();
        for (int i = 0; i <= meshHeight; i += 1)
        {
            for (int j = 0; j <= meshWidth; j += 1)
            {
                vertexList.Add(((new Vector3(1f * ((float)j / meshWidth), 0f, 1f * ((float)i / meshHeight)))));
            }
        }

        List<int> triangleIndices = new List<int>();
        for (int i = 0; i < meshHeight; i += 1)
        {
            for (int j = 0; j < meshWidth; j += 1)
            {
                int lowerLeft = (i * (meshWidth + 1)) + j;
                int lowerRight = (i * (meshWidth + 1)) + j + 1;
                int upperLeft = ((i + 1) * (meshWidth + 1)) + j;
                int upperRight = ((i + 1) * (meshWidth + 1)) + j + 1;

                //first triangle
                triangleIndices.Add(lowerLeft);
                triangleIndices.Add(upperLeft);
                triangleIndices.Add(upperRight);

                //second triangle
                triangleIndices.Add(lowerLeft);
                triangleIndices.Add(upperRight);
                triangleIndices.Add(lowerRight);
            }
        }

        /*
        //adjust y coordinates of each vertex based on the sin value
        for (int i = 0; i < vertexList.Count; i += 1)
        {
            Vector3 v = new Vector3(vertexList[i].x, vertexList[i].y, vertexList[i].z); //the untransformed point on the mesh in world space
                                                                                        //float x3figureVal = (a11 * v.x * v.x) + (a12 * v.x * v.z) + (a21 * v.x * v.z) + (a22 * v.z * v.z);
                                                                                        //float x3globalVal = FigureObject.convertYFigureToWorld(x3figureVal);
            float x3 = (a11 * v.x * v.x) + (a12 * v.x * v.z) + (a21 * v.x * v.z) + (a22 * v.z * v.z);
            vertexList[i] = new Vector3(v.x, x3, v.z);
        }*/

        Vector3[] meshVertices = vertexList.ToArray();
        int[] meshTriangles = triangleIndices.ToArray();
        Mesh mesh = new Mesh();
        mesh.vertices = meshVertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();

        return mesh;
    }


    //called when an object collides with the mesh
    public void MakeSplash(GameObject splashingObject)
    {
        float yVelocity = Mathf.Abs(splashingObject.GetComponent<Rigidbody>().velocity.y);
        print("Y VELOCITY AT COLLISION IS: " + yVelocity.ToString());

        float waveAmp = 0.1f * yVelocity;
        float dampingCoef =  0.60f - (Mathf.Clamp(yVelocity, 0f, 10f) * (2f/50f));  //y velocity mapped to 0.60-0.20
        float wavelength = splashingObject.GetComponent<Collider>().bounds.extents.x / 2f; //quarter the width of the colliding object
        Vector3 splashPoint = splashingObject.transform.position;

        splashPoint = new Vector3(splashPoint.x / 16f, 0f, splashPoint.z / 16f);

        print("splash point is: " + splashPoint.ToString());

        GameObject splash = new GameObject();
        splash.AddComponent<Splash>();
        splash.GetComponent<Splash>().water = gameObject;
        splash.GetComponent<Splash>().SetAmplitude(waveAmp);
        splash.GetComponent<Splash>().SetDampeningCoef(dampingCoef);
        splash.GetComponent<Splash>().SetWavelength(waveLength);
        splash.GetComponent<Splash>().SetSplashPoint(splashPoint);
    }


    public void MakeRandomSplash()
    {
        float yVelocity = (Random.value * 10f) + 5f; //random y velocity magnitude between 5 and 15

        float waveAmp = 0.065f * yVelocity;
        float dampingCoef = 0.60f - (Mathf.Clamp(yVelocity, 0f, 10f) * (2f / 50f));  //y velocity mapped to 0.60-0.20
        float wavelength = (Random.value * 0.75f) + 0.25f; //between 0.25f and 1f

        Vector3 splashPoint = new Vector3((Random.value * 0.8f) + 0.1f, 0f, (Random.value * 0.8f) + 0.1f); //random point on the mesh between (0.1, 0.1) and (0.9, 0.9)

        GameObject splash = new GameObject();
        splash.AddComponent<Splash>();
        splash.GetComponent<Splash>().water = gameObject;
        splash.GetComponent<Splash>().SetAmplitude(waveAmp);
        splash.GetComponent<Splash>().SetDampeningCoef(dampingCoef);
        splash.GetComponent<Splash>().SetWavelength(waveLength);
        splash.GetComponent<Splash>().SetSplashPoint(splashPoint);
    }

}
