using System;
using EditorTools;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Project/Input Reader")]
public class InputReader : ScriptableObject, PlayerControls.IPlayerActions
{
	[Header("DEBUG")]
	[CHCReadOnly, SerializeField] Vector2 moveInput;
	[CHCReadOnly, SerializeField] Vector2 mouseInput;

	PlayerControls playerControls;
	
	public event Action JumpInputEvent = delegate { };
	public event Action JumpInputCancelledEvent = delegate { };
	public event Action<Vector2> MoveInputEvent = delegate { };
	public event Action<Vector2> MouseInputEvent = delegate { };
	
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
		moveInput = context.performed 
			? context.ReadValue<Vector2>() 
			: Vector2.zero;
		MoveInputEvent?.Invoke(moveInput);
	}

	public void OnLook(InputAction.CallbackContext context)
	{
		mouseInput = context.performed
			? context.ReadValue<Vector2>()
			: Vector2.zero;
		MouseInputEvent?.Invoke(mouseInput);
	}

	public void OnFire(InputAction.CallbackContext context)
	{
		Cursor.visible = !Cursor.visible;
		Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
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