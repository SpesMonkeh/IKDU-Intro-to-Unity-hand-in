using System;
using EditorTools;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
	[Header("SETTINGS")]
	[SerializeField, Range(1f, 20f)] float cameraDistance = 5f;
	[Header(">> Focus")]
	[SerializeField] Transform focusObject;
	[SerializeField, Min(0f)] float focusRadius = 1f;
	[CHCReadOnly] public Vector3 focusPoint;
	
	Vector3 LocalPosition { get => transform.localPosition; set => transform.localPosition = value; }

	void Awake()
	{
		focusPoint = focusObject.position;
	}

	void LateUpdate()
	{
		UpdateFocusPoint();
		
		Vector3 lookDirection = transform.forward;
		LocalPosition = focusPoint - lookDirection * cameraDistance;
	}

	void UpdateFocusPoint()
	{
		Vector3 targetPoint = focusObject.position;

		if (focusRadius > 0f)
		{
			float distance = Vector3.Distance(targetPoint, focusPoint);

			if (distance > focusRadius)
			{
				focusPoint = Vector3.Lerp(targetPoint, focusPoint, focusRadius / distance);
			}
		}
		else
			focusPoint = targetPoint;
	}
}