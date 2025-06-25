using UnityEngine;
using System.Collections.Generic; // Required for List

[RequireComponent(typeof(ParticleSystem))] // Automatically add ParticleSystem if missing
public class InteractiveParticleOrb : MonoBehaviour
{
    [Header("Orb Settings")]
    [Range(100, 10000)]
    public int particleCount = 2000;
    public float orbRadius = 5f;
    public float particleSize = 0.1f;
    public Gradient particleColor = GetDefaultGradient(); // Nice default color

    [Header("Idle Behavior")]
    public float idleDriftSpeed = 0.5f;
    public float rotationSpeed = 10f; // Slow rotation of the whole orb

    [Header("Mouse Interaction")]
    public float interactionRadius = 8f; // How close mouse needs to be
    public float mouseAttractStrength = 5f;
    public float mouseOrbitStrength = 10f;
    public float returnToOrbStrength = 1f;
    public float velocityDamping = 0.5f; // How quickly particles slow down

    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;
    private Vector3[] particleHomePositions; // Store original relative positions
    private Camera mainCamera;
    private float interactionRadiusSqr; // For efficient distance checking

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("InteractiveParticleOrb requires a Main Camera tagged in the scene!");
            enabled = false; // Disable script if no camera
            return;
        }

        InitializeParticles();
        interactionRadiusSqr = interactionRadius * interactionRadius; // Pre-calculate squared radius
    }

    void InitializeParticles()
    {
        particles = new ParticleSystem.Particle[particleCount];
        particleHomePositions = new Vector3[particleCount];

        var main = ps.main;
        main.startSize = particleSize;
        main.startSpeed = 0; // We control speed manually
        main.startLifetime = Mathf.Infinity; // Live forever
        main.maxParticles = particleCount;
        main.loop = false; // We manage them ourselves
        main.simulationSpace = ParticleSystemSimulationSpace.World; // Important for mouse interaction

        var emission = ps.emission;
        emission.enabled = false; // Disable standard emission

        var shape = ps.shape;
        shape.enabled = false; // Disable standard shape emission

        // --- Manually create particles in a sphere ---
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 homePos = Random.insideUnitSphere * orbRadius;
            particleHomePositions[i] = homePos;

            particles[i].position = transform.position + homePos; // Initial world position
            particles[i].startColor = particleColor.Evaluate(Random.value); // Use gradient
            particles[i].startSize = particleSize * Random.Range(0.8f, 1.2f); // Slight size variation
            particles[i].startLifetime = Mathf.Infinity;
            particles[i].remainingLifetime = Mathf.Infinity;
            particles[i].velocity = Random.insideUnitSphere * idleDriftSpeed; // Start with gentle drift
        }

        ps.SetParticles(particles, particleCount); // Apply the initial particles
        ps.Play(); // Need to call Play() even with manual control sometimes
    }

    void Update()
    {
        // Optional: Slow orb rotation
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        // Get current particles from the system
        int numParticlesAlive = ps.GetParticles(particles);

        // Get mouse position in world space (on the same Z-plane as the orb)
        Vector3 mouseScreenPos = Input.mousePosition;
        // Set Z to the distance from camera to the orb's center
        mouseScreenPos.z = mainCamera.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        bool isMouseClose = (mouseWorldPos - transform.position).sqrMagnitude < interactionRadiusSqr * 2f; // A bit wider check

        // Update each particle
        for (int i = 0; i < numParticlesAlive; i++)
        {
            Vector3 particleWorldPos = particles[i].position;
            Vector3 homeWorldPos = transform.TransformPoint(particleHomePositions[i]); // Update home pos based on orb rotation/position

            Vector3 vectorToMouse = mouseWorldPos - particleWorldPos;
            float distToMouseSqr = vectorToMouse.sqrMagnitude;

            Vector3 targetVelocity = particles[i].velocity;

            // --- Interaction Logic ---
            if (isMouseClose && distToMouseSqr < interactionRadiusSqr)
            {
                // Attract towards mouse
                targetVelocity += vectorToMouse.normalized * mouseAttractStrength * (1f - distToMouseSqr / interactionRadiusSqr) * Time.deltaTime;

                // Add orbiting force (perpendicular to attraction vector and a world axis like Up)
                Vector3 orbitDirection = Vector3.Cross(vectorToMouse.normalized, Vector3.up); // Adjust Up if your orb isn't horizontal
                targetVelocity += orbitDirection * mouseOrbitStrength * (1f - distToMouseSqr / interactionRadiusSqr) * Time.deltaTime;

                 // Damp velocity more when interacting
                targetVelocity *= (1.0f - (velocityDamping*1.5f) * Time.deltaTime);
            }
            else
            {
                // Return towards home position
                Vector3 vectorToHome = homeWorldPos - particleWorldPos;
                targetVelocity += vectorToHome * returnToOrbStrength * Time.deltaTime;

                // Add some gentle idle drift/swirl relative to home
                targetVelocity += Random.insideUnitSphere * idleDriftSpeed * 0.1f * Time.deltaTime; // Less random drift when returning

                 // Damp velocity
                targetVelocity *= (1.0f - velocityDamping * Time.deltaTime);
            }

            // Clamp velocity (optional)
            // if (targetVelocity.sqrMagnitude > maxSpeed * maxSpeed) {
            //     targetVelocity = targetVelocity.normalized * maxSpeed;
            // }

            particles[i].velocity = targetVelocity;

            // Optional: Change color based on distance/interaction (simple example)
            float closeness = Mathf.Clamp01(1f - distToMouseSqr / interactionRadiusSqr);
            particles[i].startColor = particleColor.Evaluate(closeness); // Color shifts based on closeness
        }

        // Apply the updated particles back to the system
        ps.SetParticles(particles, numParticlesAlive);
    }


    // Helper to get a nice default gradient if user doesn't set one
    private static Gradient GetDefaultGradient()
    {
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.5f, 0f, 1f), 0.0f), new GradientColorKey(new Color(0f, 1f, 1f), 0.5f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        return grad;
    }

    // Draw gizmo in editor for interaction radius
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}