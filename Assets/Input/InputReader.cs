using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "Project/Input Reader")]
public class InputReader : ScriptableObject, PlayerControls.IPlayerActions
{
	PlayerControls playerControls;
	
	public event Action<Vector2> MoveInput = delegate { };
	
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
		var vector2Input = context.performed 
			? context.ReadValue<Vector2>() 
			: Vector2.zero;
		MoveInput?.Invoke(vector2Input);
	}

	public void OnLook(InputAction.CallbackContext context)
	{
	}

	public void OnFire(InputAction.CallbackContext context)
	{
	}
}