using UnityEngine;

public class RobotController : MonoBehaviour
{
    [Header("Wheel Colliders (physical)")]
    public WheelCollider wheelFrontLeft;
    public WheelCollider wheelFrontRight;
    public WheelCollider wheelBackLeft;
    public WheelCollider wheelBackRight;

    [Header("Visual Wheel Meshes")]
    public Transform visualFrontLeft;
    public Transform visualFrontRight;
    public Transform visualBackLeft;
    public Transform visualBackRight;

    [Header("Sensors")]
    public Transform sensorFront;
    public Transform sensorLeft;
    public Transform sensorRight;
    public Transform yawReference;

    [Header("Driving")]
    public float maxMotorTorque = 350f;
    public float maxSteerAngle = 30f;
    public float targetCruiseSpeed = 12f;
    public float maxSpeed = 18f;

    [Header("Sensors / Steering Logic")]
    public float sensorRange = 6f;
    public float avoidStrength = 2.0f;
    public float centerStrength = 0.6f;
    public float sideDeadZone = 0.2f;

    [Header("Smoothness / Stability")]
    public float accelResponse = 2.5f;
    public float steerResponse = 6f;
    public float yawDamping = 0.01f;
    public float brakeTorque = 300f;

    private Rigidbody rb;
    private float desiredSpeed;
    private float currentSteerAngle;
    private float previousYaw;

    #region Unity

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        desiredSpeed = targetCruiseSpeed;

        Vector3 com = rb.centerOfMass;
        com.y -= 0.2f;
        rb.centerOfMass = com;

        if (sensorFront != null)
            sensorFront.localRotation = Quaternion.Euler(0f, 0f, 0f);

        if (sensorLeft != null)
            sensorLeft.localRotation = Quaternion.Euler(0f, -45f, 0f);

        if (sensorRight != null)
            sensorRight.localRotation = Quaternion.Euler(0f, 45f, 0f);

        if (yawReference != null)
            previousYaw = yawReference.eulerAngles.y;
        else
            previousYaw = transform.eulerAngles.y;
    }

    private void FixedUpdate()
    {
        float frontDist = CastSensor(sensorFront);
        float leftDist  = CastSensor(sensorLeft);
        float rightDist = CastSensor(sensorRight);

        HandleSteering(frontDist, leftDist, rightDist);
        HandleMotor(frontDist);

        ApplyWheelForces();

        UpdateVisualWheel(wheelFrontLeft,  visualFrontLeft);
        UpdateVisualWheel(wheelFrontRight, visualFrontRight);
        UpdateVisualWheel(wheelBackLeft,   visualBackLeft);
        UpdateVisualWheel(wheelBackRight,  visualBackRight);
    }

    #endregion

    #region Sensors

    private float CastSensor(Transform sensor)
    {
        if (sensor == null)
            return sensorRange;

        RaycastHit hit;
        if (Physics.Raycast(sensor.position, sensor.forward, out hit, sensorRange))
        {
            Debug.DrawLine(sensor.position, hit.point, Color.red);
            return hit.distance;
        }

        Debug.DrawRay(sensor.position, sensor.forward * sensorRange, Color.green);
        return sensorRange;
    }

    #endregion

    #region Motor / Speed

    private void HandleMotor(float frontDist)
    {
        float speed = rb.velocity.magnitude;

        float slowDownStart = sensorRange * 0.7f;
        float stopDist      = 1.0f;

        float targetSpeed = targetCruiseSpeed;

        if (frontDist < slowDownStart)
        {

            float t = Mathf.InverseLerp(stopDist, slowDownStart, frontDist);
            t = Mathf.Clamp01(t);
            targetSpeed = Mathf.Lerp(0f, targetCruiseSpeed * 0.6f, t);
        }

        targetSpeed = Mathf.Min(targetSpeed, maxSpeed);

        desiredSpeed = Mathf.Lerp(desiredSpeed, targetSpeed,
                                  accelResponse * Time.fixedDeltaTime);

        float speedError = desiredSpeed - speed;

        float throttle = Mathf.Clamp(speedError, -1f, 1f);

        if (throttle < 0f)
        {
            ApplyMotorTorque(0f);
            ApplyBrake(brakeTorque * -throttle);
        }
        else
        {
            float torque = throttle * maxMotorTorque;
            ApplyBrake(0f);
            ApplyMotorTorque(torque);
        }
    }

    private void ApplyMotorTorque(float torque)
    {
        if (wheelBackLeft != null)
            wheelBackLeft.motorTorque = torque;

        if (wheelBackRight != null)
            wheelBackRight.motorTorque = torque;

        if (wheelFrontLeft != null)
            wheelFrontLeft.motorTorque = 0f;

        if (wheelFrontRight != null)
            wheelFrontRight.motorTorque = 0f;
    }

    private void ApplyBrake(float torque)
    {
        if (wheelFrontLeft != null)
            wheelFrontLeft.brakeTorque = torque;
        if (wheelFrontRight != null)
            wheelFrontRight.brakeTorque = torque;
        if (wheelBackLeft != null)
            wheelBackLeft.brakeTorque = torque;
        if (wheelBackRight != null)
            wheelBackRight.brakeTorque = torque;
    }

    #endregion

    #region Steering

    private void HandleSteering(float frontDist, float leftDist, float rightDist)
    {
        float sideDiff = (rightDist - leftDist) / sensorRange;
        float centerSteer = 0f;

        if (Mathf.Abs(sideDiff) > sideDeadZone)
            centerSteer = sideDiff * centerStrength;

        float avoidSteer = 0f;
        float avoidThreshold = sensorRange * 0.8f;

        if (frontDist < avoidThreshold)
        {
            float intensity = 1f - (frontDist / avoidThreshold);
            intensity = Mathf.Clamp01(intensity);

            if (leftDist > rightDist)
                avoidSteer = -avoidStrength * intensity;
            else
                avoidSteer =  avoidStrength * intensity;
        }

        float steeringCommand = centerSteer + avoidSteer;

        float yaw = (yawReference != null)
            ? yawReference.eulerAngles.y
            : transform.eulerAngles.y;

        float yawRate = Mathf.DeltaAngle(previousYaw, yaw) / Time.fixedDeltaTime;
        previousYaw = yaw;

        steeringCommand -= yawRate * yawDamping;

        steeringCommand = Mathf.Clamp(steeringCommand, -1f, 1f);

        float targetAngle = steeringCommand * maxSteerAngle;

        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetAngle,
                                       steerResponse * Time.fixedDeltaTime);
    }

    private void ApplyWheelForces()
    {
        if (wheelFrontLeft != null)
            wheelFrontLeft.steerAngle = currentSteerAngle;

        if (wheelFrontRight != null)
            wheelFrontRight.steerAngle = currentSteerAngle;

        if (wheelBackLeft != null)
            wheelBackLeft.steerAngle = 0f;

        if (wheelBackRight != null)
            wheelBackRight.steerAngle = 0f;
    }

    #endregion

    #region Visual Wheels

    private void UpdateVisualWheel(WheelCollider col, Transform trans)
    {
        if (col == null || trans == null)
            return;

        Vector3 pos;
        Quaternion rot;
        col.GetWorldPose(out pos, out rot);

        trans.position = pos;
        trans.rotation = rot;
    }

    #endregion
}