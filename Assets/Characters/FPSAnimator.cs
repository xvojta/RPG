using BLINK.Controller;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSAnimator : MonoBehaviour
{
	private Animator _animator;
	private FPSController controller;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		controller = GetComponent<FPSController>();
	}

	public void UpdateState(bool hasMovementRestriction)
	{
		if (hasMovementRestriction)
		{
			_animator.SetFloat(CharacterAnimatorParamId.HorizontalSpeed, 0);
		}
		else
		{
			float normHorizontalSpeed = controller.HorizontalVelocity.magnitude /
										controller.MovementSettings.MaxHorizontalSpeed;
			_animator.SetFloat(CharacterAnimatorParamId.HorizontalSpeed, normHorizontalSpeed);
		}

		float jumpSpeed = controller.MovementSettings.JumpSpeed;
		float normVerticalSpeed =
			controller.VerticalVelocity.y.Remap(-jumpSpeed, jumpSpeed, -1.0f, 1.0f);
		_animator.SetFloat(CharacterAnimatorParamId.VerticalSpeed, normVerticalSpeed);
		_animator.SetBool(CharacterAnimatorParamId.IsGrounded, controller.IsGrounded);
	}
}
