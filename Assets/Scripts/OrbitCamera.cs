using EditorTools;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
	[Header("SETTINGS")]
	[SerializeField, Range(1f, 20f)] float cameraDistance = 5f;
	
	[Header("> Focus")]
	[SerializeField, Min(0f)] float focusRadius = 1f;
	[SerializeField, Range(0f, 1f)] float focusCentering = .5f;
	[SerializeField] Transform focusObject;
	[CHCReadOnly] public Vector2 orbitAngles = new (45f, 0f);
	[CHCReadOnly] public Vector3 focusPoint;
	[CHCReadOnly] public Vector3 previousFocusPoint;

	[Header("> Orbit")]
	[SerializeField, Min(0f)] float alignmentDelay = 5f;
	[SerializeField, Range(1f, 360f)] float rotationSpeed = 90f;
	[SerializeField, Range(-89f, 89f)] float minVerticalAngle = -30f;
	[SerializeField, Range(-89f, 89f)] float maxVerticalAngle = 60f;
	[CHCReadOnly] public float lastManualRotationTime;
	[CHCReadOnly] public Vector2 mouseInput;

	[Header("SCRIPTABLE OBJECTS")]    
	[SerializeField] InputReader inputReader;
	
	void OnValidate()
	{
		if (maxVerticalAngle < minVerticalAngle)
			maxVerticalAngle = minVerticalAngle;
	}

	void OnEnable()
	{
		inputReader.MouseInputEvent += vector => mouseInput = new Vector2(vector.y, vector.x);
	}

	void Awake()
	{
		focusPoint = focusObject.position;
		transform.localRotation = Quaternion.Euler(orbitAngles);
	}

	void LateUpdate()
	{
		UpdateFocusPoint();
		Quaternion lookRotation;

		if (ManualRotation() || AutomaticRotation())
		{
			ConstrainOrbitAngles();
			lookRotation = Quaternion.Euler(orbitAngles);
		}
		else
			lookRotation = transform.localRotation;

		Vector3 lookDirection = lookRotation * Vector3.forward;
		Vector3 lookPosition = focusPoint - lookDirection * cameraDistance;
		transform.SetPositionAndRotation(lookPosition, lookRotation);
	}

	void UpdateFocusPoint()
	{
		previousFocusPoint = focusPoint;
		
		Vector3 targetPoint = focusObject.position;

		if (focusRadius > 0f)
		{
			float distance = Vector3.Distance(targetPoint, focusPoint);
			float t = 1f;

			if (distance > .01f && focusCentering > 0f)
				t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
			
			if (distance > focusRadius)
				t = Mathf.Min(t, focusRadius / distance);

			focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
		}
		else
			focusPoint = targetPoint;
	}

	void ConstrainOrbitAngles()
	{
		orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

		switch (orbitAngles.y)
		{
			case < 0f:
				orbitAngles.y += 360f;
				break;
			case >= 360f:
				orbitAngles.y -= 360f;
				break;
		}
	}

	bool AutomaticRotation()
	{
		if (Time.unscaledTime - lastManualRotationTime < alignmentDelay) return false;

		Vector2 movement = new(
			focusPoint.x - previousFocusPoint.x,
			focusPoint.z - previousFocusPoint.z);
		float movementDeltaSquared = movement.sqrMagnitude;

		if (movementDeltaSquared < .0001f) return false;

		float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSquared));
		orbitAngles.y = headingAngle;
		
		return true;
	}
	
	bool ManualRotation()
	{
		const float e = .001f;
		if (mouseInput.x is >= -e and <= e && mouseInput.y is >= -e and <= e) return false;
		
		orbitAngles += rotationSpeed * Time.unscaledDeltaTime * mouseInput;
		lastManualRotationTime = Time.unscaledTime;
		return true;
	}

	static float GetAngle(Vector2 direction)
	{
		float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
		bool xIsCounterClockwise = direction.x < 0;
		
		return xIsCounterClockwise ? 360f - angle : angle;
	}
}