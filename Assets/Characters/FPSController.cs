using BLINK.Controller;
using BLINK.RPGBuilder.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{
    float speed;
    float speedInterpolating = 0;
    bool stunned;
    bool isSprinting;
    bool cameraCanRotate;
    public CharacterController charController;
    public FPSControllerEssentials ControllerEssentials;
    public CameraRotateController cameraController;
    public float interpolatingSpeed = 2f; //speed in which interpolating happens

    public float maxWalkSpeed = 8f;
    public float maxSprintSpeed = 12f;
    public float gravity = -9.81f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public Controller Controller;
    public Animator animator;
    Vector3 velocity;
    bool isGrounded;

    void Start()
    {
        charController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        ControllerEssentials = GetComponent<FPSControllerEssentials>();
        cameraController = GetComponent<CameraRotateController>();

        Controller.Init();
    }

    private void Update()
    {
        if (CombatManager.playerCombatNode == null) return;
        Controller.OnCharacterUpdate();
    }

    public void MovementUpdate(PlayerInput movementInput)
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < -2) velocity.y = -2f;
        
        if (!ControllerEssentials.HasMovementRestrictions() /*&& movementInput.HasMoveInput*/)
        {
            speedInterpolating += Time.deltaTime * interpolatingSpeed;

            if(isSprinting)
            {
                if (speed < maxSprintSpeed)
                {
                    speed = Mathf.Lerp(speed, maxSprintSpeed, speedInterpolating);
                }
                else
                {
                    speedInterpolating = 0;
                }
            }
            else
            {
                if (speed < maxWalkSpeed)
                {
                    speed = Mathf.Lerp(speed, maxWalkSpeed, speedInterpolating);
                }
                else if(speed == maxWalkSpeed)
                {
                    speedInterpolating = 0;
                }
                else
                {
                    speed = Mathf.Lerp(maxWalkSpeed, speed, speedInterpolating);
                }
            }
            Vector3 movement = (transform.forward * movementInput.MoveInput.y + transform.right * movementInput.MoveInput.x);
            charController.Move(movement * speed * Time.deltaTime);
        }
        else
        {
            if(speed == 0)
            {
                speedInterpolating = 0;
            }
            else
            {
                speed = Mathf.Lerp(speed, 0, speedInterpolating);
            }
        }
        animator.SetFloat("Speed", 1f);
        print(animator.GetFloat("Speed") + "s");
        animator.SetFloat("Speed", (speed/maxSprintSpeed)*0.75f + 0.25f);

        if(cameraCanRotate && !ControllerEssentials.HasRotationRestrictions())
        {
            cameraController.UpdateRotation();
        }

        velocity.y += gravity * Time.deltaTime;

        charController.Move(velocity * Time.deltaTime);
    }

    #region Get-methods
    public CharacterController getCharController()
    {
        return charController;
    }

    public bool getSprinting()
    {
        return isSprinting;
    }
    
    public bool getCameraCanRotate()
    {
        return cameraCanRotate;
    }
    #endregion

    #region  Set-methods
    public void setSprinting(bool isSprinting)
    {
        this.isSprinting = isSprinting;
    }

    public void SetStunned(bool stunned)
    {
        this.stunned = stunned;
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    public void SetCameraCanRotate(bool cameraCanRotate)
    {
        this.cameraCanRotate = cameraCanRotate;
    }
    #endregion
}
