// Original code by Jasper Flick. Catlike Coding. 2019. https://catlikecoding.com/unity/tutorials/movement
// The code is licensed under the MIT-0 license.
// BitBucket repo at: https://bitbucket.org/catlikecodingunitytutorials/movement-01-sliding-a-sphere/src/master/
// Adapted and modified by Christian Holm Christensen. October 14th, 2022. 

using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovingSphere : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField, Range(0f, 1f)] float bounciness = .5f;
    [SerializeField, Range(0f, 100f)] float maxAcceleration = 10f;
    [SerializeField, Range(0f, 100f)] float maxSpeed = 10f;
    [SerializeField] Rect allowedArea = new (-4.5f, -4.5f, 9f, 9f);
    
    [Space, Header("Scriptable Objects")]    
    [SerializeField] InputReader inputReader;
    
    Vector2 moveInputVector;
    Vector3 velocity;
    
    void OnEnable()
    {
        inputReader.MoveInput += vector => moveInputVector = vector;
    }

    void Update()
    {
        MoveSphere();
    }

    void MoveSphere()
    {
        moveInputVector = Vector2.ClampMagnitude(moveInputVector, maxLength: 1f);
        
        Vector3 desiredVelocity = new Vector3(moveInputVector.x, 0f, moveInputVector.y) * maxSpeed;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

        Vector3 displacement = velocity * Time.deltaTime;
        Vector3 new3DPosition = transform.localPosition + displacement;
        
        if (new3DPosition.x < allowedArea.xMin)
        {
            new3DPosition.x = allowedArea.xMin;
            velocity.x = -velocity.x * bounciness;
        }
        else if (new3DPosition.x > allowedArea.xMax)
        {
            new3DPosition.x = allowedArea.xMax;
            velocity.x = -velocity.x * bounciness;
        }
        
        if (new3DPosition.z < allowedArea.yMin)
        {
            new3DPosition.z = allowedArea.yMin;
            velocity.z = -velocity.z * bounciness;
        }
        else if (new3DPosition.z > allowedArea.yMax)
        {
            new3DPosition.z = allowedArea.yMax;
            velocity.z = -velocity.z * bounciness;
        }
        
        transform.localPosition = new3DPosition;
    }
}
