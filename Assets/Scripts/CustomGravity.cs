/*
 * Original code by Jasper Flick. Catlike Coding. 22 February, 2020. https://catlikecoding.com/unity/tutorials/movement
 * Original code licensed under the MIT-0 license:      https://catlikecoding.com/unity/tutorials/license/
 * BitBucket repos at:
 *     Part 5: Custom Gravity        - https://bitbucket.org/catlikecodingunitytutorials/movement-05-custom-gravity/src/master/
 *     Part 6: Complex Gravity       - https://bitbucket.org/catlikecodingunitytutorials/movement-06-complex-gravity/src/master/
 * 
 * Adapted and modified by Christian Holm Christensen. October 19, 2022. 
*/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CustomGravity
{

	static List<GravitySource> gravitySources = Enumerable.Empty<GravitySource>().ToList();
	
	static float PhysicsGravityY => Physics.gravity.y;

	public static void Register(GravitySource source)
	{
		Debug.Assert(!gravitySources.Contains(source), "Tried to register an already registered gravity source!", source);
		gravitySources.Add(source);
	}

	public static void Unregister(GravitySource source)
	{
		Debug.Assert(!gravitySources.Contains(source), "Tried to unregister an unknown gravity source!", source);
		gravitySources.Remove(source);
	}
	
	public static Vector3 GetUpAxis(Vector3 position)
	{
		return -GetGravity(position).normalized;
	}
	
	public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
	{
		upAxis = GetUpAxis(position);
		return GetGravity(position);
	}
	
	public static Vector3 GetGravity(Vector3 position)
	{
		return gravitySources.Aggregate(
			seed: Vector3.zero,
			func: (vector, source) => vector + source.GetGravity(position));
	}
}
