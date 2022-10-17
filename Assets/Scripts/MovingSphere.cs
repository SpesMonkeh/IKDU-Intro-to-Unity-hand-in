/*
 * Original code by Jasper Flick. Catlike Coding. 2019. https://catlikecoding.com/unity/tutorials/movement
 * Original code licensed under the MIT-0 license:      https://catlikecoding.com/unity/tutorials/license/
 * BitBucket repos at:
 *     Part 1: Sliding a Sphere      - https://bitbucket.org/catlikecodingunitytutorials/movement-01-sliding-a-sphere/src/master/
 *     Part 2: Physics               - https://bitbucket.org/catlikecodingunitytutorials/movement-02-physics/src/master/
 *     Part 3: Surface Contact       - https://bitbucket.org/catlikecodingunitytutorials/movement-03-surface-contact/src/master/
 *     Part 4: Orbit Camera          - https://bitbucket.org/catlikecodingunitytutorials/movement-04-orbit-camera/src/master/
 *     Part 5: Custom Gravity        - https://bitbucket.org/catlikecodingunitytutorials/movement-05-custom-gravity/src/master/
 *     Part 6: Complex Gravity       - https://bitbucket.org/catlikecodingunitytutorials/movement-06-complex-gravity/src/master/
 *     Part 7: Moving The Ground     - https://bitbucket.org/catlikecodingunitytutorials/movement-07-moving-the-ground/src/master/
 *     Part 8: Climbing              - https://bitbucket.org/catlikecodingunitytutorials/movement-08-climbing/src/master/
 *     Part 9: Swimming              - https://bitbucket.org/catlikecodingunitytutorials/movement-09-swimming/src/master/
 *    Part 10: Reactive Environment  - https://bitbucket.org/catlikecodingunitytutorials/movement-10-reactive-environment/src/master/
 *    Part 11: Rolling               - https://bitbucket.org/catlikecodingunitytutorials/movement-11-rolling/src/master/
 * 
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
    [SerializeField, Range(0f, 100f)] float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)] float maxGroundAcceleration = 10f;
    [CHCReadOnly] public int groundContactCount;
    [CHCReadOnly] public Vector2 moveInputVector;
    [CHCReadOnly] public Vector3 desiredVelocity;
    [CHCReadOnly] public Vector3 velocity;
    [CHCReadOnly] public Vector3 groundContactNormal;
    [Header(">>> Movement Angles")]
    [SerializeField, Range(0f, 90f)] float maxGroundAngle = 25f;
    [SerializeField, Range(0f, 90f)] float maxStairAngle = 50f;
    [CHCReadOnly] public int steepContactCount;
    [CHCReadOnly] public float minGroundDotProduct;
    [CHCReadOnly] public float minStairDotProduct;
    [CHCReadOnly] public Vector3 steepContactNormal;

    [Header(">>> Snap To Ground")]
    [SerializeField, Range(0f, 100f)] float maxSnapToGroundSpeed = 100f;
    [SerializeField, Min(0f)] float snapToGroundProbeDistance = 1f;
    [SerializeField] LayerMask groundProbeMask = -1;
    [SerializeField] LayerMask stairProbeMask = -1;
    [CHCReadOnly] public int jumpingPhysicsStepsSinceLast;
    [CHCReadOnly] public int groundedPhysicsStepsSinceLast;
    [Space]
    [Header(">> In Air")]
    [SerializeField, Range(0, 10)] int maxAirJumps;
    [SerializeField, Range(0f, 10f)] float jumpHeight = 2f;
    [SerializeField, Range(0f, 100f)] float maxAirAcceleration = 1f;
    [CHCReadOnly] public int jumpPhase;
    [CHCReadOnly] public bool hasJumpInput;
    [Space]
    [Header("SCRIPTABLE OBJECTS")]    
    [SerializeField] InputReader inputReader;
    [CHCReadOnly] public Rigidbody body;

    bool OnGround => groundContactCount > 0;
    bool OnSteep => steepContactCount > 0;
    bool AirJumpingIsAllowed => maxAirJumps > 0;
    void OnEnable()
    {
        inputReader.MoveInputEvent += vector => moveInputVector = vector;
        inputReader.JumpInputEvent += OnJumpInput;
        inputReader.JumpInputCancelledEvent += OnJumpInputCancelled;
    }

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairDotProduct = Mathf.Cos(maxStairAngle * Mathf.Deg2Rad);
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
        steepContactCount = 0;
        
        groundContactNormal = Vector3.zero;
        steepContactNormal= Vector3.zero;
    }

    void UpdateState()
    {
        groundedPhysicsStepsSinceLast += 1;
        jumpingPhysicsStepsSinceLast += 1;
        
        velocity = body.velocity;
        
        if(OnGround || SnapToGround() || CheckSteepContacts())
        {
            groundedPhysicsStepsSinceLast = 0;
            CheckForFalseLanding();
            
            if(groundContactCount > 1)
                groundContactNormal.Normalize();
        }
        else
            groundContactNormal = Vector3.up;
    }


    void HandleJumping()
    {
        if (!hasJumpInput) return;
        
        hasJumpInput = false;
        
        Jump();
    }

    void Jump()
    {
        Vector3 jumpDirection;
        
        if (OnGround)
            jumpDirection = groundContactNormal;
        else if (OnSteep)
        {
            jumpDirection = steepContactNormal;
            jumpPhase = 0;
        }
        else if (AirJumpingIsAllowed && jumpPhase <= maxAirJumps)
        {
            PreventExtraJumpAfterSurfaceFall();
            jumpDirection = groundContactNormal;
        }
        else
            return;
        
        jumpingPhysicsStepsSinceLast = 0;
        jumpPhase += 1;
        
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        
        jumpDirection = (jumpDirection + Vector3.up).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        
        if(alignedSpeed > 0f)
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        
        velocity += jumpDirection * jumpSpeed;
    }

    void CheckForFalseLanding() => jumpPhase = jumpingPhysicsStepsSinceLast > 1 ? 0 : jumpPhase;
    void PreventExtraJumpAfterSurfaceFall() => jumpPhase = jumpPhase == 0 ? 1 : jumpPhase;

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

    bool SnapToGround()
    {
        if (groundedPhysicsStepsSinceLast > 1 || jumpingPhysicsStepsSinceLast <= 2) return false;
        
        float speed = velocity.magnitude;
        if (speed > maxSnapToGroundSpeed) return false;
        
        if (!Physics.Raycast(body.position, Vector3.down, out RaycastHit hit, snapToGroundProbeDistance, groundProbeMask)) return false;
        if (hit.normal.y < GetMinDotProduct(hit.collider.gameObject.layer)) return false;
        
        groundContactCount = 1;
        groundContactNormal = hit.normal;

        float dot = Vector3.Dot(velocity, hit.normal);
        if(dot > 0f)
            velocity = (velocity - hit.normal * dot).normalized * speed;
        
        return true;
    }

    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepContactNormal.Normalize();
            if (steepContactNormal.y >= minGroundDotProduct)
            {
                groundContactCount = 1;
                groundContactNormal = steepContactNormal;
                return true;
            }
        }

        return false;
    }

    float GetMinDotProduct(int layer) => (stairProbeMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairDotProduct;

    Vector3 ProjectOnContactPlane(Vector3 vector) => vector - groundContactNormal * Vector3.Dot(vector, groundContactNormal);
    
    void OnJumpInput() => hasJumpInput = true;
    void OnJumpInputCancelled() => hasJumpInput = false;

    void OnCollisionEnter(Collision collision) => EvaluateCollision(collision);
    void OnCollisionStay(Collision collision) => EvaluateCollision(collision);
    void EvaluateCollision(Collision collision)
    {
        float minDotProduct = GetMinDotProduct(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minDotProduct)
            {
                groundContactCount += 1;
                groundContactNormal += normal;
            }
            else if (normal.y > -.01f)
            {
                steepContactCount += 1;
                steepContactNormal += normal;
            }
        }
    }

    void OnDisable()
    {
        inputReader.JumpInputEvent -= OnJumpInput;
        inputReader.JumpInputCancelledEvent -= OnJumpInputCancelled;
    }
}
