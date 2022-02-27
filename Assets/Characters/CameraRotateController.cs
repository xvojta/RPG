using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotateController : MonoBehaviour
{
    public float mouseSensitivity = 100f;

    Transform camera;
    public Transform head;
    public FPSController FpsController;
    float xRotation = 0f;

    public void Init()
    {
        FpsController = GetComponent<FPSController>();
        camera = Camera.main.transform;
        camera.transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }

    internal void UpdateRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 40f);

        transform.Rotate(Vector3.up * mouseX);
        FpsController.SetControlRotation(Vector3.up * mouseX);
        camera.transform.rotation = Quaternion.Euler(xRotation, camera.rotation.eulerAngles.y + mouseX, camera.rotation.eulerAngles.z);
    }

    private void Update()
    {
        camera.position = head.position;
    }
}
