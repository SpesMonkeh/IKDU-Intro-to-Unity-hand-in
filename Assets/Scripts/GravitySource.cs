/*
 * Original code by Jasper Flick. Catlike Coding. 27 March, 2020. https://catlikecoding.com/unity/tutorials/movement
 * Original code licensed under the MIT-0 license:      https://catlikecoding.com/unity/tutorials/license/
 * BitBucket repos at:
 *     Part 6: Complex Gravity       - https://bitbucket.org/catlikecodingunitytutorials/movement-06-complex-gravity/src/master/
 * 
 * Adapted and modified by Christian Holm Christensen. October 19, 2022. 
*/

using System;
using UnityEngine;

public class GravitySource : MonoBehaviour
{
	void OnEnable()
	{
		CustomGravity.Register(this);
	}

	public virtual Vector3 GetGravity(Vector3 position)
	{
		return Physics.gravity;
	}

	void OnDisable()
	{
		CustomGravity.Unregister(this);
	}
}