using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereGenerator : MonoBehaviour
{

    public GameObject SpherePrefab;
    public bool generatingSpheres; //does the simulation allow spheres to be generated with an x key

    private GameObject water;

    // Start is called before the first frame update
    void Start()
    {
        water = FindObjectOfType<WaveSimulator>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && generatingSpheres)
        {
            GameObject sphere  = Instantiate(SpherePrefab) as GameObject;
            sphere.transform.position = new Vector3(3f + Random.value * 10f, 10f + Random.value * 10f, 3f + Random.value * 10f); //generates a sphere at a random location over the surface
        }

    }



}
