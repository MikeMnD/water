using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Water;

public class ForceSimulator : MonoBehaviour
{

    public GameObject Water;
    public bool waterResistance;
    public bool airResistance;

    public float radius;

    public float waterResistanceFactor = 0.5f; //the opposite force applied to the object for each 1m/s of velocity while moving through water
    public float airResistanceFactor = 0.15f;

    private float bottomYPos;
    private float heightSubmerged;
    private float volumeSubmerged;
    private float totalVolume;
    private float density;
    private float gravityForce = 9.81f;
    private float buoyantForce;
    private float waterDensity = 1000f;
    private Vector3 resistancePoint;

    // Start is called before the first frame update
    void Start()
    {
        heightSubmerged = 0f;
        totalVolume = (4f / 3f) * Mathf.PI * Mathf.Pow(radius, 3);
        density = 1f / totalVolume;
        Water = FindObjectOfType<Water>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        //resistance always applied to the side of the sphere opposite movement
        if (GetComponent<Rigidbody>().velocity.y < 0f) {
            resistancePoint = new Vector3(0f, -0.5f, 0f);
        }
        else
        {
            resistancePoint = new Vector3(0f, 0.5f, 0f);
        }


        bottomYPos = (transform.position.y - radius);
        heightSubmerged = Mathf.Clamp(Water.transform.position.y - bottomYPos, 0f, 2*radius);
        if(heightSubmerged > 0.001f) //at least partially under water
        {
            print("submerged");
            volumeSubmerged = ((Mathf.PI * heightSubmerged * heightSubmerged) / 3f) * ((3 * radius) - heightSubmerged);
            buoyantForce = (volumeSubmerged * waterDensity * gravityForce)/400f;
            GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(0f, buoyantForce, 0f), new Vector3(0f, -0.5f, 0f));
            if(waterResistance)
            {
                //the object is moving into the water, so apply water resistance
                float waterResistanceForce = GetComponent<Rigidbody>().velocity.y * waterResistanceFactor * -1f;
                GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(0f, waterResistanceForce, 0f), resistancePoint);
           }
        }
        else //above water
        {
            if (airResistance)
            {
                float airResistanceForce = GetComponent<Rigidbody>().velocity.y * airResistanceFactor * -1f;
                GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(0f, airResistanceForce, 0f), resistancePoint);

            }
        }
    }


    public void ApplyLeftForce()
    {
        GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(-150f, 0f, 0f), Vector3.zero);
    }

    public void ApplyRightForce()
    {
        GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(150f, 0f, 0f), Vector3.zero);
    }
}
