/*
 * Original code by Jasper Flick. Catlike Coding. 22 February, 2020. https://catlikecoding.com/unity/tutorials/movement
 * Original code licensed under the MIT-0 license:      https://catlikecoding.com/unity/tutorials/license/
 * BitBucket repos at:
 *     Part 5: Custom Gravity        - https://bitbucket.org/catlikecodingunitytutorials/movement-05-custom-gravity/src/master/
 *     Part 6: Complex Gravity       - https://bitbucket.org/catlikecodingunitytutorials/movement-06-complex-gravity/src/master/
 * 
 * Adapted and modified by Christian Holm Christensen. October 19, 2022. 
*/

using CHCEditorTools;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Renderer))]
public class CustomGravityRigidbody : MonoBehaviour
{
	[Header("SETTINGS")]
	[SerializeField] bool canSleepIfFloating;
	[CHCReadOnly] public float floatDelay;
	[CHCReadOnly] public Rigidbody body;
	[CHCReadOnly] public Renderer objectRenderer;
	
	static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

	bool ObjectIsAsleepAndGray => body.IsSleeping() && ObjectMaterial.color == Color.gray;
	bool ObjectIsAwakeAndGreen => !body.IsSleeping() && ObjectMaterial.color == Color.green;
	bool VelocityIsLessThanMinimumThreshold => body.velocity.sqrMagnitude < .0001f;
	Material ObjectMaterial => objectRenderer.material;
	Color GetRigidbodyStateColor => body.IsSleeping() ? Color.gray : Color.green;

	
	void Awake()
	{
		Initialize();
	}

	void Initialize()
	{
		GetRequiredComponents();
		GetComponentValues();
	}

	void Update()
	{
		UpdateMaterialColor();
	}

	void UpdateMaterialColor()
	{
		if(ObjectIsAsleepAndGray || ObjectIsAwakeAndGreen) return;
		
		ObjectMaterial.SetColor(BaseColor, GetRigidbodyStateColor);
	}

	void FixedUpdate()
	{
		UpdateRigidbody();
	}

	void UpdateRigidbody()
	{
		if (SleepWhileFloating()) return;

		body.AddForce(CustomGravity.GetGravity(body.position), ForceMode.Acceleration);
	}

	bool SleepWhileFloating()
	{
		if (!canSleepIfFloating) return false;
		
		if (body.IsSleeping())
		{
			ResetFloatDelay();
			return true;
		}

		if (VelocityIsLessThanMinimumThreshold)
		{
			floatDelay += Time.deltaTime;
			if (floatDelay >= 1f) return true;
		}
		else
			ResetFloatDelay();

		return false;
	}

	void GetRequiredComponents()
	{
		objectRenderer = GetComponent<Renderer>();
		body = GetComponent<Rigidbody>();
	}

	void GetComponentValues()
	{
		body.useGravity = false;
	}

	void ResetFloatDelay() => floatDelay = 0f;
}