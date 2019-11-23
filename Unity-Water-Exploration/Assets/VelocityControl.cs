using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityControl : MonoBehaviour
{

    private float positionDelta = 0f; //per second in one chosen dimension

    private float velocityMoving = 0.08f; //constant
    private float direction = 1f; //1 represents forward, -1 represents backwards
    private float stoppingDuration = 1f; //how long does it take for the robot to stop from constant velocity?
    private float startingDuration = 1.5f; //how long does it take for the robot to reach constant velocity from rest?
    private float stoppingAcceleration;
    private float startingAcceleration;

    private enum movementState { moving, starting, stopping, stopped};
    private movementState state = movementState.stopped;


    // Start is called before the first frame update
    void Start()
    {
        stoppingAcceleration = velocityMoving / stoppingDuration;
        startingAcceleration = velocityMoving / startingDuration;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && state == movementState.moving)
        {
            state = movementState.stopping;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            state = movementState.starting;
        }



        switch (state)
        {
            case movementState.moving:
                positionDelta = velocityMoving * direction * Time.deltaTime;
                break;
            case movementState.starting:
                positionDelta += (startingAcceleration * direction * Time.deltaTime);
                if (Mathf.Abs(positionDelta) >= (velocityMoving) ) state = movementState.moving;
                break;
            case movementState.stopping:
                positionDelta -= (stoppingAcceleration * direction * Time.deltaTime);
                if (direction > 0f && positionDelta < 0f)
                {
                    state = movementState.starting;
                    direction = direction * -1f;
                }
                if (direction < 0f && positionDelta > 0f)
                {
                    state = movementState.starting;
                    direction = direction * -1f;
                }
                break;
        }



        transform.position = new Vector3(transform.position.x + positionDelta, transform.position.y, transform.position.z);

    }

    public void Push()
    {
        //GetComponent<Rigidbody>().velocity = new Vector3((0.5f - Random.value) * 25f, 0f, (0.5f - Random.value) * 25f);
        //GetComponent<Rigidbody>().AddForce(new Vector3((0.5f - Random.value) * 1000f, 0f, (0.5f - Random.value) * 1000f));
    }
}
