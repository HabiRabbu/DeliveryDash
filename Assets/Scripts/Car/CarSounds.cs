using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class CarSounds : MonoBehaviour
{
    public float minSpeed;
    public float maxSpeed;
    private float currentSpeed;

    private Rigidbody carRigidBody;
    private AudioSource carAudio;

    public float minPitch;
    public float maxPitch;
    private float pitchFromCar;

    void Start()
    {
        carAudio = GetComponent<AudioSource>();
        carRigidBody = GetComponent<Rigidbody>();

        carAudio.Play();
    }

    void Update()
    {
        EngineSound();
    }

    void EngineSound()
    {
        currentSpeed = carRigidBody.linearVelocity.magnitude;
        float speedPercent = Mathf.InverseLerp(minSpeed, maxSpeed, currentSpeed);
        carAudio.pitch = Mathf.Lerp(minPitch, maxPitch, speedPercent);
    }
}
