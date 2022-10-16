/*
 * Original code by Jasper Flick. Catlike Coding. 2019. https://catlikecoding.com/unity/tutorials/movement
 * Original code licensed under the MIT-0 license:      https://catlikecoding.com/unity/tutorials/license/
 * BitBucket repo at:                                   https://bitbucket.org/catlikecodingunitytutorials/movement-01-sliding-a-sphere/src/master/
 * Adapted and modified by Christian Holm Christensen. October 16th, 2022. 
*/

using EditorTools;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingSphere : MonoBehaviour
{
    [Header("SETTINGS")]
    [Header("> Locomotion")]
    [Header(">> On Ground")]
    [SerializeField, Range(0f, 90f)] float maxGroundAngle = 25f;
    [SerializeField, Range(0f, 100f)] float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] float maxGroundAcceleration = 10f;
    [Space]
    [Header(">> In Air")]
    [SerializeField, Range(0, 10)] int maxAirJumps;
    [SerializeField, Range(0f, 10f)] float jumpHeight = 2f;
    [SerializeField, Range(0f, 100f)] float maxAirAcceleration = 1f;
    [Space]
    [Header("SCRIPTABLE OBJECTS")]    
    [SerializeField] InputReader inputReader;
    [Space]
    [Header("DEBUG")]
    [CHCReadOnly] public int jumpPhase;
    [CHCReadOnly] public int groundContactCount;
    [CHCReadOnly] public bool hasJumpInput;
    [CHCReadOnly] public float minGroundDotProduct;
    [CHCReadOnly] public Vector2 moveInputVector;
    [CHCReadOnly] public Vector3 desiredVelocity;
    [CHCReadOnly] public Vector3 velocity;
    [CHCReadOnly] public Vector3 contactNormal;
        
    Rigidbody body;

    bool OnGround => groundContactCount > 0;

    void OnEnable()
    {
        inputReader.MoveInputEvent += vector => moveInputVector = vector;
        inputReader.JumpInputEvent += OnJumpInput;
        inputReader.JumpInputCancelledEvent += OnJumpInputCancelled;
    }

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    void Update()
    {
        moveInputVector = Vector2.ClampMagnitude(moveInputVector, maxLength: 1f);
        desiredVelocity = new Vector3(moveInputVector.x, 0f, moveInputVector.y) * maxSpeed;
    }

    void FixedUpdate()
    {
        UpdateState();
        AdjustVelocity();
        HandleJumping();
        
        body.velocity = velocity;
        
        ClearState();
    }

    void ClearState()
    {
        groundContactCount = 0;
        contactNormal = Vector3.zero;
    }

    void UpdateState()
    {
        velocity = body.velocity;
        
        if(OnGround)
        {
            jumpPhase = 0;
            if(groundContactCount > 1)
                contactNormal.Normalize();
        }
        else
            contactNormal = Vector3.up;
    }
    
    void HandleJumping()
    {
        if (!hasJumpInput) return;
        hasJumpInput = false;
        Jump();
    }

    void Jump()
    {
        if (!OnGround && jumpPhase >= maxAirJumps) return;
        
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        float alignedSpeed = Vector3.Dot(velocity, contactNormal);

        if(alignedSpeed > 0f)
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        
        velocity += contactNormal * jumpSpeed;
    }

    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;
        
        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);
        
        float acceleration = OnGround ? maxGroundAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    Vector3 ProjectOnContactPlane(Vector3 vector) => vector - contactNormal * Vector3.Dot(vector, contactNormal);
    
    void OnJumpInput() => hasJumpInput = true;
    void OnJumpInputCancelled() => hasJumpInput = false;

    void OnCollisionEnter(Collision collision) => EvaluateCollision(collision);
    void OnCollisionStay(Collision collision) => EvaluateCollision(collision);
    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y < minGroundDotProduct) continue;
            
            groundContactCount += 1;
            contactNormal += normal;
        }
    }

    void OnDisable()
    {
        inputReader.JumpInputEvent -= OnJumpInput;
        inputReader.JumpInputCancelledEvent -= OnJumpInputCancelled;
    }
}
