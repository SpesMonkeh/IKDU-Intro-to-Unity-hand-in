/*
 * Original code by Jasper Flick. Catlike Coding. 2019. https://catlikecoding.com/unity/tutorials/movement
 * Original code licensed under the MIT-0 license:      https://catlikecoding.com/unity/tutorials/license/
 * BitBucket repos at:
 *     Part 5: Custom Gravity        - https://bitbucket.org/catlikecodingunitytutorials/movement-05-custom-gravity/src/master/
 *     Part 6: Complex Gravity       - https://bitbucket.org/catlikecodingunitytutorials/movement-06-complex-gravity/src/master/
 *     Part 7: Moving The Ground     - https://bitbucket.org/catlikecodingunitytutorials/movement-07-moving-the-ground/src/master/
 *     Part 8: Climbing              - https://bitbucket.org/catlikecodingunitytutorials/movement-08-climbing/src/master/
 *     Part 9: Swimming              - https://bitbucket.org/catlikecodingunitytutorials/movement-09-swimming/src/master/
 *    Part 10: Reactive Environment  - https://bitbucket.org/catlikecodingunitytutorials/movement-10-reactive-environment/src/master/
 *    Part 11: Rolling               - https://bitbucket.org/catlikecodingunitytutorials/movement-11-rolling/src/master/
 * 
 * Adapted and modified by Christian Holm Christensen. October 19th, 2022. 
*/

using UnityEngine;

public static class CustomGravity
{

	static float PhysicsGravityY => Physics.gravity.y;
	
	public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
	{
		upAxis = GetUpAxis(position);
		return GetGravity(position);
	}
	
	public static Vector3 GetUpAxis(Vector3 position)
	{
		Vector3 up = position.normalized;
		return PhysicsGravityY < 0f ? up : -up;
	}
	
	public static Vector3 GetGravity(Vector3 position)
	{
		return position.normalized * PhysicsGravityY;
	}
}
