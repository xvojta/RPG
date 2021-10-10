using BLINK.Controller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSInputController : Controller
{
	public FPSController controller;

	private PlayerInput _playerInput;

	public override void Init()
	{
		_playerInput = FindObjectOfType<PlayerInput>();
	}

	public override void OnCharacterUpdate()
	{
		_playerInput.UpdateInput();
		controller.MovementUpdate(_playerInput);
	}

	public override void OnCharacterFixedUpdate()
	{
	}
}
