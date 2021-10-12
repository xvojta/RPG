using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotateController : MonoBehaviour
{
    public float mouseSensitivity = 100f;

    public Transform camera;
    public FPSController FpsController;
    public Quaternion initRotation;
    float xRotation = 0f;

    public void Init()
    {
        FpsController = GetComponent<FPSController>();
        initRotation = camera.rotation;
    }

    internal void UpdateRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 40f);

        camera.rotation = Quaternion.Euler(xRotation, 0f, 0f) * initRotation;
        transform.Rotate(Vector3.up * mouseX);
        FpsController.SetControlRotation(Vector3.up * mouseX);
    }
}
