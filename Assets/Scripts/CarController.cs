using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{

    [Header("Vehicle Settings")]
    [SerializeField] private float maxAcceleration = 30.0f;
    [SerializeField] private float brakeAcceleration = 50.0f;
    [SerializeField] private float turnSensitivity = 5.0f;
    [SerializeField] private float maxSteeringAngle = 30.0f;
    [SerializeField] private Vector3 _centreOfMass;
    [SerializeField] private float torqueConstant = 100f;
    [SerializeField] private float engineBreakTorque = 100f;

    [Header("Physics Tuning")]
    [SerializeField] private float drag = 0f;
    [SerializeField] private float angularDrag = 0.05f;

    [Header("Friction")]
    [SerializeField] private float forwardExtremumSlip = 0.4f;
    [SerializeField] private float forwardExtremumValue = 1f;
    [SerializeField] private float forwardAsymptoteSlip = 0.8f;
    [SerializeField] private float forwardAsymptoteValue = 0.5f;
    [SerializeField] private float forwardStiffness = 2.0f;
    [SerializeField] private float sidewaysExtremumSlip = 0.2f;
    [SerializeField] private float sidewaysExtremumValue = 1f;
    [SerializeField] private float sidewaysAsymptoteSlip = 0.5f;
    [SerializeField] private float sidewaysAsymptoteValue = 0.75f;
    [SerializeField] private float sidewaysStiffness = 2.5f;
    [SerializeField] private float brakeSidewaysStiffness = 0.5f;
    [SerializeField] private float brakeForwardStiffness = 1.0f;

    [Header("Wheels")]
    public List<Wheel> wheels;

    private CarControls controls;
    private Vector2 moveVector;
    private float moveInput;
    private float steerInput;
    private Rigidbody carRigidBody;

    public enum Axel { Front, Rear }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public GameObject wheelEffectObj;
        public Axel axel;
    }

    void Awake()
    {
        controls = new CarControls();
        controls.Gameplay.Move.performed += ctx => moveVector = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => moveVector = Vector2.zero;
    }

    private void OnEnable() => controls.Gameplay.Enable();
    private void OnDisable() => controls.Gameplay.Disable();

    void Start()
    {
        carRigidBody = GetComponent<Rigidbody>();
        carRigidBody.centerOfMass = _centreOfMass;
        carRigidBody.linearDamping = drag;
        carRigidBody.angularDamping = angularDrag;

        SetupWheelFriction();
    }

    void SetupWheelFriction()
    {
        foreach (var wheel in wheels)
        {
            WheelFrictionCurve forwardFriction = wheel.wheelCollider.forwardFriction;
            forwardFriction.extremumSlip = forwardExtremumSlip;
            forwardFriction.extremumValue = forwardExtremumValue;
            forwardFriction.asymptoteSlip = forwardAsymptoteSlip;
            forwardFriction.asymptoteValue = forwardAsymptoteValue;
            forwardFriction.stiffness = forwardStiffness;
            wheel.wheelCollider.forwardFriction = forwardFriction;

            WheelFrictionCurve sidewaysFriction = wheel.wheelCollider.sidewaysFriction;
            sidewaysFriction.extremumSlip = sidewaysExtremumSlip;
            sidewaysFriction.extremumValue = sidewaysExtremumValue;
            sidewaysFriction.asymptoteSlip = sidewaysAsymptoteSlip;
            sidewaysFriction.asymptoteValue = sidewaysAsymptoteValue;
            sidewaysFriction.stiffness = sidewaysStiffness;
            wheel.wheelCollider.sidewaysFriction = sidewaysFriction;
        }
    }

    void Update()
    {
        GetInputs();
        AnimateWheels();
        WheelEffects();
    }

    void FixedUpdate()
    {
        Move();
        Steer();
        Brake();
    }

    void GetInputs()
    {
        moveInput = moveVector.y;
        steerInput = moveVector.x;
    }

    void Move()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Rear) //TODO: Make this togglable (FWD, RWD, AWD)
            {
                wheel.wheelCollider.motorTorque = moveInput * maxAcceleration * torqueConstant;
                // wheel.wheelCollider.brakeTorque = 0;
            }
        }
    }

    void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                float targetAngle = steerInput * turnSensitivity * maxSteeringAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(
                    wheel.wheelCollider.steerAngle,
                    targetAngle,
                    Time.fixedDeltaTime * 10f
                );
            }
        }
    }

void Brake()
{
    bool isBraking = (Keyboard.current?.spaceKey?.isPressed ?? false) || (Gamepad.current?.aButton?.isPressed ?? false);
    float torque = isBraking ? 300 * brakeAcceleration : 0f;
    float speed = carRigidBody.linearVelocity.magnitude;

    foreach (var wheel in wheels)
    {
        wheel.wheelCollider.brakeTorque = torque;

        var forwardFriction = wheel.wheelCollider.forwardFriction;
        var sidewaysFriction = wheel.wheelCollider.sidewaysFriction;

        if (isBraking)
        {
            forwardFriction.stiffness = Mathf.Lerp(forwardStiffness, brakeForwardStiffness, speed / 50f);
            sidewaysFriction.stiffness = Mathf.Lerp(sidewaysStiffness, brakeSidewaysStiffness, speed / 50f);
        }
        else
        {
            forwardFriction.stiffness = forwardStiffness;
            sidewaysFriction.stiffness = sidewaysStiffness;
        }

        wheel.wheelCollider.forwardFriction = forwardFriction;
        wheel.wheelCollider.sidewaysFriction = sidewaysFriction;
    }
}


    void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            wheel.wheelModel.transform.SetPositionAndRotation(pos, rot);
        }
    }

    // //Setup Dynamic Friction at high speeds?

    // float speed = carRigidBody.velocity.magnitude;

    // foreach (var wheel in wheels)
    // {
    //     WheelFrictionCurve s = wheel.wheelCollider.sidewaysFriction;
    //     s.stiffness = Mathf.Lerp(1.2f, 0.5f, speed / 50f); // less grip at higher speed
    //     wheel.wheelCollider.sidewaysFriction = s;
    // }

    void WheelEffects()
    {
        bool isBraking = (Keyboard.current?.spaceKey?.isPressed ?? false) || (Gamepad.current?.aButton?.isPressed ?? false);

        foreach (var wheel in wheels)
        {
            if (isBraking && wheel.axel == Axel.Rear)
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = true;
            }
            else
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = false;
            }
        }
    }

}