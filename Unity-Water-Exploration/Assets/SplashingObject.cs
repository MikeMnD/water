using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashingObject : MonoBehaviour
{

    public int MaxNumSplashes = 10; //the maximum number of splashes this object can make on the surface
    private int numSplashes = 0;

    public float activeTime = 10f; //how many seconds is the object active until it is faded out and deleted
    public float fadeOutTime = 2f;

    private float timer;
    private float fadeTimerStart;
    private float timeFading;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().material = new Material(GetComponent<MeshRenderer>().material); //make a local copy of the material

        //GetComponent<MeshRenderer>().material.color = new Color(Random.value * 256f, Random.value * 256f, Random.value * 256f);

        timer = Time.time;
        fadeTimerStart = -1f;
        timeFading = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time - timer > activeTime && fadeTimerStart < 0f)
        {
            fadeTimerStart = Time.time;
        }
        if(fadeTimerStart > 0f) //fading out this object
        {
            timeFading += Time.deltaTime;
            float proportionFaded = timeFading / fadeOutTime;
            if (proportionFaded >= 1f) Destroy(gameObject);
            Color color = GetComponent<MeshRenderer>().material.color;
            color.a = 1f - proportionFaded;
            GetComponent<MeshRenderer>().material.color = color;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (numSplashes >= MaxNumSplashes) return;

        if(collision.collider.gameObject.tag == "waterSurface")
        {
            numSplashes += 1;
            collision.collider.gameObject.GetComponent<WaveSimulator>().MakeSplash(gameObject);
        }
    }
}
