using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splash : MonoBehaviour
{
    public float waveLength = 0.15f;
    public float waveSpeed = 0.06f; //how many meters per second does the start of each ripple move?
    public float dampingCoefficient = 0.65f;
    public float waveAmplitude = 0.5f; //the height of the splash before any damping occurs

    private float splashPropogationRange = 0f; //how far from the splash point has the splash travelled?

    private float currentWaveHeight; //current height of the splash after the dampening that has occurred after time passed
    private float negligibleWaveHeight = 0.01f; //when current wave height falls under this value, the wave is destroyed
   
    public Vector3 splashPoint;
    public GameObject water;

    public GameObject obj;

    /*public float initialWaveHeight = 1f;
    public float fadeOutTime = 3f;
    public float waveSpreadSpeed = 0.04f; //how many meters per second does the end of each ripple wave move?*/

    /*private float currentWaveHeight;
    private float currentWaveStartDist;
    private float currentWaveEndDist;*/

    private float startTime;
    private float timePassed;


    // Start is called before the first frame update
    void Start()
    {
        startTime = Time.time;
        currentWaveHeight = negligibleWaveHeight * 2f;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTime();
        UpdateWaveHeight();
        UpdateSplashPropogationRange();
        AdjustMesh();

        if (timePassed > 20f) Destroy(gameObject);
        if (timePassed > 10f && currentWaveHeight < negligibleWaveHeight) Destroy(gameObject);
    }

    //location of the collision on the surface
    public void SetSplashPoint(Vector3 sp)
    {
        splashPoint = sp;
    }

    //between 0 and 1; higher values represent firmer surfaces --> higher viscosity
    public void SetDampeningCoef(float d)
    {
        dampingCoefficient = d;
    }

    //should be approximately the width of the object that collides with the surface
    public void SetWavelength(float wl)
    {
        waveLength = wl;
    }

    //should be approximately a function of the velocity of the object as it collides with the surface
    public void SetAmplitude(float a)
    {
        waveAmplitude = a;
    }

    void UpdateTime()
    {
        timePassed = Time.time - startTime;
    }

    void UpdateWaveHeight()
    {
        currentWaveHeight = waveAmplitude * (Mathf.Exp(-1f * dampingCoefficient * timePassed)) * Mathf.Cos(timePassed * 2f * Mathf.PI); //cosine function with dampening for wave height
    }

    void UpdateSplashPropogationRange()
    {
        splashPropogationRange += (waveSpeed * Time.deltaTime);
    }

    float GetSplashHeightAtMeshPoint(Vector3 meshPoint)
    {
        float distFromSplashPoint = Mathf.Abs(Vector2.Distance(new Vector2(meshPoint.x, meshPoint.z), new Vector2(splashPoint.x, splashPoint.z)));
        if (distFromSplashPoint > splashPropogationRange)
        {
            return 0f;
        }
        else
        {
            float proportionIntoSinWave = (((splashPropogationRange - distFromSplashPoint) % waveLength) / waveLength);
            float sinInput = proportionIntoSinWave * Mathf.PI * 2f;
            return Mathf.Sin(sinInput) * currentWaveHeight;
        }
    }


    void AdjustMesh()
    {
        Vector3[] verts = water.GetComponent<MeshFilter>().mesh.vertices;
        for (int i = 0; i < verts.Length; i += 1)
        {
            verts[i].y += GetSplashHeightAtMeshPoint(verts[i]);
        }
        water.GetComponent<MeshFilter>().mesh.vertices = verts;

    }

}
