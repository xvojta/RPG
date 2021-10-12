using BLINK.Controller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSInputController : Controller
{
	public float ControlRotationSensitivity = 3.0f;

	private PlayerInput _playerInput;
	private CameraRotateController _playerCamera;

	public override void Init()
	{
		_playerInput = FindObjectOfType<PlayerInput>();
		_playerCamera = FindObjectOfType<CameraRotateController>();

		_playerCamera.Init();
	}

	public override void OnCharacterUpdate()
	{
		if (!FPSController.ControllerEssentials.HasMovementRestrictions())
		{
			_playerInput.UpdateInput();
		}
		else
		{
			_playerInput.JumpInput = false;
			FPSController.SetJumpInput(false);
		}

		if (FPSController.cameraCanRotate &&
			!FPSController.ControllerEssentials.HasRotationRestrictions())
		{
			_playerCamera.UpdateRotation();
		}

		if (!FPSController.ControllerEssentials.HasMovementRestrictions())
		{
			FPSController.SetMovementInput(GetMovementInput());
			FPSController.SetJumpInput(_playerInput.JumpInput);
		}
	}

	public override void OnCharacterFixedUpdate()
	{
		if (!FPSController.cameraCanRotate) return;
		_playerCamera.UpdateRotation();
	}

	private Vector3 GetMovementInput()
	{
		// Calculate the move direction relative to the character's yaw rotation
		Quaternion yawRotation = new Quaternion();
		if (!FPSController.isFlying)
		{
			yawRotation = Quaternion.Euler(0.0f, FPSController.GetControlRotation().y, 0.0f);
		}
		else
		{
			yawRotation = Quaternion.Euler(0.0f, FPSController.GetControlRotation().y, 0.0f);
		}

		Vector3 forward = yawRotation * Vector3.forward;
		Vector3 right = yawRotation * Vector3.right;
		Vector3 movementInput = (forward * _playerInput.MoveInput.y + right * _playerInput.MoveInput.x);

		if (movementInput.sqrMagnitude > 1f)
		{
			movementInput.Normalize();
		}

		return movementInput;
	}
}
