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
        camera.localRotation = Quaternion.Euler(90, 0, 90);
    }

    internal void UpdateRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 40f);

        transform.Rotate(Vector3.up * mouseX);
        FpsController.SetControlRotation(Vector3.up * mouseX);
        camera.rotation = Quaternion.Euler(0, mouseX, 0f) * transform.rotation;
        camera.localRotation = camera.localRotation * Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void Update()
    {
        camera.position = head.position;
    }
}
