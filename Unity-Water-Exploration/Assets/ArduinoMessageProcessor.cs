using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArduinoMessageProcessor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnConnectionEvent(bool connected)
    {

    }

    public void OnMessageArrived(string message)
    {
        print("RECEIVED A MESSAGE: " + message);
    }
}
