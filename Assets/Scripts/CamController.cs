using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    [SerializeField] GameObject spawnArea;
    [SerializeField] int SPEED = 100;

    float currentAngle;

    void Start()
    {
        currentAngle = Camera.main.transform.rotation.eulerAngles.y;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            Camera.main.transform.RotateAround(spawnArea.transform.position, Vector3.up, Time.deltaTime * SPEED);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Camera.main.transform.RotateAround(spawnArea.transform.position, Vector3.up, -Time.deltaTime * SPEED);
        }

        if (Input.GetKey(KeyCode.S))
        {
            Camera.main.transform.position -= Camera.main.transform.forward * Time.deltaTime * SPEED;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            Camera.main.transform.position += Camera.main.transform.forward * Time.deltaTime * SPEED;
        }
    }
}
