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

public class GravityPlane : GravitySource
{
	[Header("SETTINGS")]
	[SerializeField] bool drawGizmos = true;
	[SerializeField] float gravity = 9.81f;
	[SerializeField, Min(0f)] float gravitationalRange = 1f;
	[CHCReadOnly] public Color gravityPullLimitColor = Color.cyan;
	[CHCReadOnly] public Color gravityPlaneColor = Color.yellow;
	
	Transform ThisTransform => transform;
	
	public override Vector3 GetGravity(Vector3 position)
	{
		
		Vector3 up = ThisTransform.up;
		float distance = Vector3.Dot(up, position - ThisTransform.position);
		
		if(distance > gravitationalRange)
			return Vector3.zero;
		
		return -gravity * up;
	}


	void OnDrawGizmos()
	{
		if (!drawGizmos) return;

		Vector3 size = new Vector3(1f, 0f, 1f);
		Vector3 scale = ThisTransform.localScale;
		scale.y = gravitationalRange;

		Gizmos.matrix = Matrix4x4.TRS(ThisTransform.position, ThisTransform.rotation, scale);
		
		Gizmos.color = gravityPlaneColor;
		Gizmos.DrawWireCube(Vector3.zero, size);

		if (gravitationalRange == 0f) return;
		
		Gizmos.color = gravityPullLimitColor;
		Gizmos.DrawWireCube(Vector3.up, size);
	}
}