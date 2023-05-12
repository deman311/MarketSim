using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    [SerializeField] GameObject spawnArea;
    [SerializeField] int SPEED = 100;
    [SerializeField] int DISPLAY_CAM_SPEED = 20;

    // Define a Vector3 to store the initial position of the camera
    Vector3 initialPosition, targetPosition;
    GameObject target;

    bool isFollowing, isDisplayCam = false;

    void Start()
    {
        if (spawnArea == null)
            return;

        // Store the initial position of the camera in the initialPosition Vector3
        initialPosition = new Vector3(spawnArea.GetComponent<Renderer>().bounds.max.x + 20, 20, 0);
        isFollowing = false;
        ResetCamera();
    }

    void Update()
    {
        // If the user presses the X key, pick a random game object from the children of the "Customers" game object
        // and set it as the new target position for the camera
        if (Input.GetKeyDown(KeyCode.X))
        {
            isFollowing = !isFollowing;
            if (isFollowing)
            {
                GameObject customers = GameObject.Find("Customers");
                int childIndex = Random.Range(0, customers.transform.childCount);
                target = customers.transform.GetChild(childIndex).gameObject;
                targetPosition = target.transform.position;
            }
            else
                ResetCamera();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            isFollowing = !isFollowing;
        }
        if (isFollowing)
        {
            Camera.main.transform.RotateAround(targetPosition, Vector3.up, Time.deltaTime * DISPLAY_CAM_SPEED);
        }

        // Rotate the camera around the target position
        if (Input.GetKey(KeyCode.D))
        {
            Camera.main.transform.RotateAround(targetPosition, Vector3.up, Time.deltaTime * SPEED);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Camera.main.transform.RotateAround(targetPosition, Vector3.up, -Time.deltaTime * SPEED);
        }

        // Move the camera towards or away from the target position
        if (Input.GetKey(KeyCode.S))
        {
            Camera.main.transform.position -= Camera.main.transform.forward * Time.deltaTime * SPEED;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            Camera.main.transform.position += Camera.main.transform.forward * Time.deltaTime * SPEED;
        }

        // Make the camera look at the target position
        if (isFollowing && target != null)
            targetPosition = target.transform.position;
        else
            targetPosition = spawnArea.transform.position;

        Camera.main.transform.LookAt(targetPosition);
    }

    private void ResetCamera()
    {
        Camera.main.transform.position = initialPosition;
        Camera.main.transform.LookAt(spawnArea.transform.position);
    }
}
