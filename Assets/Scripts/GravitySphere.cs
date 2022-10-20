/*
 * Original code by Jasper Flick. Catlike Coding. 27 March, 2020. https://catlikecoding.com/unity/tutorials/movement
 * Original code licensed under the MIT-0 license:      https://catlikecoding.com/unity/tutorials/license/
 * BitBucket repos at:
 *     Part 6: Complex Gravity       - https://bitbucket.org/catlikecodingunitytutorials/movement-06-complex-gravity/src/master/
 * 
 * Adapted and modified by Christian Holm Christensen. October 20, 2022. 
*/

using CHCEditorTools;
using UnityEngine;

public class GravitySphere : GravitySource
{
	[Header("SETTINGS")]
	[SerializeField] bool drawGizmos = true;
	[SerializeField] float gravity = 9.81f;
	[SerializeField, Min(0f)] float outerRadius = 10f;
	[SerializeField, Min(0f)] float outerFalloffRadius = 15f;
	[CHCReadOnly] public float outerFalloffFactor;
	[CHCReadOnly] public Color gravityPullLimitColor = Color.cyan;
	[CHCReadOnly] public Color gravitySphereColor = Color.yellow;

	Transform ThisTransform => transform;

	void OnValidate()
	{
		outerFalloffRadius = Mathf.Max(outerFalloffRadius, outerRadius);
		outerFalloffFactor = 1f / (outerFalloffRadius - outerRadius);
	}

	void Awake()
	{
		OnValidate();
	}

	public override Vector3 GetGravity(Vector3 position)
	{
		Vector3 vector = ThisTransform.position - position;
		float distance = vector.magnitude;
		
		if(distance > outerFalloffRadius) return Vector3.zero;

		float g = gravity / distance;

		if (distance > outerRadius)
			g *= 1f - (distance - outerRadius) * outerFalloffFactor;
		
		return g * vector;
	}

	void OnDrawGizmos()
	{
		if (!drawGizmos) return;

		Vector3 p = ThisTransform.position;

		Gizmos.color = gravitySphereColor;
		Gizmos.DrawWireSphere(p, outerRadius);

		if (outerFalloffRadius <= outerRadius) return;

		Gizmos.color = gravityPullLimitColor;
		Gizmos.DrawWireSphere(p, outerFalloffRadius);
	}
}