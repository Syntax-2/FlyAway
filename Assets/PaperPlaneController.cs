using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PaperPlaneController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Flight Controls")]
    public float turnSpeed = 2f;
    public float rollSpeed = 3f;

    [Header("Aerodynamics")]
    [Tooltip("The main multiplier for all lift forces.")]
    public float liftMultiplier = 10f;
    [Tooltip("The main multiplier for all drag forces.")]
    public float dragMultiplier = 1f;

    [Header("Angle of Attack Curves")]
    [Tooltip("How much lift is generated based on the angle of attack.")]
    public AnimationCurve liftCurve;
    [Tooltip("How much drag is generated based on the angle of attack.")]
    public AnimationCurve dragCurve;

    [Header("Auto-Stabilization")]
    [Tooltip("How strongly the plane tries to nose-down during a stall to recover speed.")]
    public float stallRecoveryTorque = 0.5f;

    private float horizontalInput;
    private float verticalInput;
    private bool isFlying = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Disable physics until launch
    }

    public void Launch(float powerPercentage)
    {
        rb.isKinematic = false;
        rb.AddForce(transform.forward * powerPercentage, ForceMode.VelocityChange); // Use VelocityChange for an instant speed boost
        isFlying = true;
    }

    void FixedUpdate()
    {
        if (!isFlying) return;

        // Get joystick input
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Calculate physics and apply forces
        ApplyAerodynamicForces();
        ApplySteering();
    }

    private void ApplyAerodynamicForces()
    {
        // We need a direction of travel to calculate anything
        if (rb.linearVelocity.magnitude < 0.1f) return;

        // 1. Calculate Angle of Attack (AoA)
        // This is the angle between the plane's forward direction and its current velocity
        float angleOfAttack = Vector3.SignedAngle(transform.forward, rb.linearVelocity, transform.right);

        // 2. Calculate Lift
        // Get the lift coefficient from our curve based on the AoA
        float liftCoefficient = liftCurve.Evaluate(angleOfAttack);
        // Lift is perpendicular to the direction of travel (velocity)
        Vector3 liftDirection = Vector3.Cross(rb.linearVelocity.normalized, transform.right);
        // The force is stronger at higher speeds (speed squared)
        float liftForce = liftCoefficient * rb.linearVelocity.sqrMagnitude * liftMultiplier;
        rb.AddForce(liftDirection * liftForce);

        // 3. Calculate Drag
        // Get the drag coefficient from our curve
        float dragCoefficient = dragCurve.Evaluate(angleOfAttack);
        // Drag opposes the direction of travel
        Vector3 dragDirection = -rb.linearVelocity.normalized;
        // The force is also stronger at higher speeds
        float dragForce = dragCoefficient * rb.linearVelocity.sqrMagnitude * dragMultiplier;
        rb.AddForce(dragDirection * dragForce);

        // 4. Apply Stall Recovery
        // If the angle of attack is too high, the plane stalls. We apply a torque to nose-down and recover.
        if (Mathf.Abs(angleOfAttack) > 30f) // 30 degrees is a high stall angle
        {
            rb.AddRelativeTorque(Vector3.right * stallRecoveryTorque * Mathf.Sign(angleOfAttack));
        }
    }

    private void ApplySteering()
    {
        // Standard flight control torque
        float yaw = horizontalInput * turnSpeed;
        float pitch = verticalInput * turnSpeed * -1f; // Invert for intuitive controls
        float roll = horizontalInput * rollSpeed * -1f; // Roll into turns

        rb.AddRelativeTorque(pitch, yaw, roll);
    }
}