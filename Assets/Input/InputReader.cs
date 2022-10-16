using System;
using EditorTools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "Project/Input Reader")]
public class InputReader : ScriptableObject, PlayerControls.IPlayerActions
{
	[Header("DEBUG")]
	[CHCReadOnly, SerializeField] Vector2 moveInputVector2;


	PlayerControls playerControls;
	
	public event Action JumpInputEvent = delegate { };
	public event Action JumpInputCancelledEvent = delegate { };
	public event Action<Vector2> MoveInputEvent = delegate { };
	
	void OnEnable()
	{
		EnablePlayerInputActions();
	}

	void EnablePlayerInputActions()
	{
		playerControls ??= new PlayerControls();
		playerControls.Player.SetCallbacks(this);
		playerControls.Player.Enable();
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		moveInputVector2 = context.performed 
			? context.ReadValue<Vector2>() 
			: Vector2.zero;
		MoveInputEvent?.Invoke(moveInputVector2);
	}

	public void OnLook(InputAction.CallbackContext context)
	{
	}

	public void OnFire(InputAction.CallbackContext context)
	{
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		switch (context)
		{
			case { phase: InputActionPhase.Performed }:
				JumpInputEvent?.Invoke();
				break;
			case { phase: InputActionPhase.Canceled }:
				JumpInputCancelledEvent?.Invoke();
				break;
		}
	}
}