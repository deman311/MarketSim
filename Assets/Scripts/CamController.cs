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
        Camera.main.transform.position = new Vector3
        (
            spawnArea.gameObject.GetComponent<Renderer>().bounds.max.x,
            20f,
            0
        );
        Camera.main.transform.LookAt(spawnArea.transform.position);
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
