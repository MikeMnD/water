using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform spawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            GameObject go = Instantiate(ballPrefab) as GameObject;
            go.transform.position = spawnPoint.position;
            go.GetComponent<Rigidbody>().AddForce(new Vector3((0.5f - Random.value) * 100f, -1f, (0.5f - Random.value) * 100f));
        }
    }


}
