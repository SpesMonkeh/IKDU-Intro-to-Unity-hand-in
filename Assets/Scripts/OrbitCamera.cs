/*
 * Original code by Jasper Flick. Catlike Coding. Published: 26 January, 2020. Updated: 16 July, 2020. https://catlikecoding.com/unity/tutorials/movement
 * Original code licensed under the MIT-0 license:      https://catlikecoding.com/unity/tutorials/license/
 * BitBucket repos at:
 *     Part 4: Orbit Camera          - https://bitbucket.org/catlikecodingunitytutorials/movement-04-orbit-camera/src/master/
 *     Part 5: Custom Gravity        - https://bitbucket.org/catlikecodingunitytutorials/movement-05-custom-gravity/src/master/
 *     Part 6: Complex Gravity       - https://bitbucket.org/catlikecodingunitytutorials/movement-06-complex-gravity/src/master/
 * 
 * Adapted and modified by Christian Holm Christensen. October 19, 2022. 
*/

using System;
using CHCEditorTools;
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
	[SerializeField, Min(0f)] float upAlignmentSpeed = 360f;
	[SerializeField, Range(0f, 90f)] float alignmentSmoothRange = 45f;
	[SerializeField, Range(1f, 360f)] float rotationSpeed = 90f;
	[SerializeField, Range(-89f, 89f)] float minVerticalAngle = -30f;
	[SerializeField, Range(-89f, 89f)] float maxVerticalAngle = 60f;
	[SerializeField] LayerMask obstructionMask = -1;
	[CHCReadOnly] public float lastManualRotationTime;
	[CHCReadOnly] public Vector2 mouseInput;
	[CHCReadOnly] public Quaternion gravityAlignment = Quaternion.identity;
	[CHCReadOnly] public Quaternion orbitAngleRotation;

	[Header("SCRIPTABLE OBJECTS")]    
	[SerializeField] InputReader inputReader;

	Camera regularCamera;
	
	bool ItIsTimeToRotateCamera => Time.unscaledTime - lastManualRotationTime >= alignmentDelay;
	Vector3 CameraHalfExtends
	{
		get
		{
			Vector3 halfExtends;
			halfExtends.y = regularCamera.nearClipPlane * Mathf.Tan(.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
			halfExtends.x = halfExtends.y * regularCamera.aspect;
			halfExtends.z = 0f;
			return halfExtends;
		}
	}
	
	void OnValidate()
	{
		if (maxVerticalAngle < minVerticalAngle)
			maxVerticalAngle = minVerticalAngle;
	}

	void OnEnable()
	{
		inputReader.MouseInputEvent += vector => mouseInput.Set(vector.y, vector.x);
	}

	void Awake()
	{
		regularCamera = GetComponent<Camera>();
		focusPoint = focusObject.position;
		
		transform.localRotation = orbitAngleRotation = Quaternion.Euler(orbitAngles);
	}

	void LateUpdate()
	{
		UpdateGravityAlignment();
		UpdateFocusPoint();
		UpdateLookRotation(out Quaternion lookRotation);
		CheckForOrbitObstruction(in lookRotation, out Vector3 lookPosition);
		
		transform.SetPositionAndRotation(lookPosition, lookRotation);
	}

	void UpdateLookRotation(out Quaternion lookRotation)
	{
		if (ManualRotation() || AutomaticRotation())
		{
			ConstrainOrbitAngles();
			orbitAngleRotation = Quaternion.Euler(orbitAngles);
		}
		
		lookRotation = gravityAlignment * orbitAngleRotation;
	}

	void UpdateGravityAlignment()
	{
		float AvoidNaNValueDotProduct(Vector3 lhs, Vector3 rhs) => Mathf.Clamp(Vector3.Dot(lhs, rhs), -1f, 1f);
		
		Vector3 fromUp = gravityAlignment * Vector3.up;
		Vector3 toUp = CustomGravity.GetUpAxis(focusPoint);

		float dot = AvoidNaNValueDotProduct(fromUp, toUp);
		float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
		float maxAngle = upAlignmentSpeed * Time.deltaTime;
		bool interpolationIsNotNeeded = angle <= maxAngle;
		
		Quaternion newAlignment = Quaternion.FromToRotation(fromUp, toUp) * gravityAlignment;
		
		gravityAlignment = interpolationIsNotNeeded 
			? newAlignment 
			: Quaternion.SlerpUnclamped(gravityAlignment, newAlignment, maxAngle / angle);
	}

	void CheckForOrbitObstruction(in Quaternion lookRotation, out Vector3 lookPosition)
	{
		Vector3 lookDirection = lookRotation * Vector3.forward;
		
		lookPosition = focusPoint - lookDirection * cameraDistance;

		Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
		Vector3 rectPosition = lookPosition + rectOffset;
		Vector3 castFrom = focusObject.position;
		Vector3 castLine = rectPosition - castFrom;
		float castDistance = castLine.magnitude;
		Vector3 castDirection = castLine / castDistance;
		
		bool orbitIsObstructed = Physics.BoxCast(
			center: castFrom,
			halfExtents: CameraHalfExtends,
			direction: castDirection,
			hitInfo: out RaycastHit hit,
			orientation: lookRotation,
			maxDistance: castDistance,
			layerMask: obstructionMask);

		if (!orbitIsObstructed) return;
		
		rectPosition = castFrom + castDirection * hit.distance;
		lookPosition = rectPosition - rectOffset;
	}

	void UpdateFocusPoint()
	{
		SetPreviousFocusPoint();
		
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
	
	void SetPreviousFocusPoint() => previousFocusPoint = focusPoint;

	void GetRotationChange(out float rotationChange, float movementDeltaSquared, float headingAngle)
	{
		float deltaAbsolute = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
		
		rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSquared);
		
		if (deltaAbsolute < alignmentSmoothRange)
			rotationChange *= deltaAbsolute / alignmentSmoothRange;
		else if (180f - deltaAbsolute < alignmentSmoothRange)
			rotationChange *= (180f - deltaAbsolute) / alignmentSmoothRange;
	}
	
	bool ManualRotation()
	{
		const float e = .001f;
		if (mouseInput.x is >= -e and <= e && mouseInput.y is >= -e and <= e) return false;
		
		orbitAngles += rotationSpeed * Time.unscaledDeltaTime * mouseInput;
		lastManualRotationTime = Time.unscaledTime;
		return true;
	}

	bool AutomaticRotation()
	{
		if (!ItIsTimeToRotateCamera) return false;

		float movementDeltaSquared = GetMovementDeltaSquared(out Vector2 movement);
		bool hasTinyMovementChange = movementDeltaSquared < .0001f;
		if (hasTinyMovementChange) return false;

		GetHeadingAngle(out float headingAngle, movementDeltaSquared, movement);
		GetRotationChange(out float rotationChange, movementDeltaSquared, headingAngle);

		orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
		return true;
	}
	
	float GetMovementDeltaSquared(out Vector2 movement)
	{
		Vector3 alignedDelta = Quaternion.Inverse(gravityAlignment) * (focusPoint - previousFocusPoint);
		movement = new Vector2(
			x: alignedDelta.x,
			y: alignedDelta.z);
		return movement.sqrMagnitude;
	}
	
	static void GetHeadingAngle(out float headingAngle, float movementDeltaSquared, Vector2 movement)
		=> headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSquared));

	static float GetAngle(Vector2 direction)
	{
		float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
		bool xIsCounterClockwise = direction.x < 0;
		
		return xIsCounterClockwise ? 360f - angle : angle;
	}
}