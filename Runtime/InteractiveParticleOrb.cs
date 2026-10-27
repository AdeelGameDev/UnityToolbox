using UnityEngine;

namespace Adeel.UnityToolbox
{
    /// <summary>
    /// Turns a <see cref="ParticleSystem"/> into a swirling sphere of particles
    /// that react to and orbit the mouse cursor.
    /// </summary>
    /// <remarks>
    /// Uses the legacy <see cref="Input"/> API. If the project is set to
    /// "Input System Package (New)" only, switch Active Input Handling to
    /// "Both" or port the mouse read to the new API.
    /// </remarks>
    [RequireComponent(typeof(ParticleSystem))] // Automatically add ParticleSystem if missing
    public class InteractiveParticleOrb : MonoBehaviour
    {
        [Header("Orb Settings")]
        [Range(100, 10000)]
        public int particleCount = 2000;
        [Range(0.1f, 100f)]
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

        private ParticleSystem _particleSystem;
        private ParticleSystem.Particle[] _particles;
        private Vector3[] _particleHomePositions; // Store original relative positions
        private Camera _mainCamera;
        private float _interactionRadiusSqr; // For efficient distance checking

        private void Start()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogError("InteractiveParticleOrb requires a Main Camera tagged in the scene!");
                enabled = false; // Disable script if no camera
                return;
            }

            InitializeParticles();
            _interactionRadiusSqr = interactionRadius * interactionRadius; // Pre-calculate squared radius
        }

        private void InitializeParticles()
        {
            _particles = new ParticleSystem.Particle[particleCount];
            _particleHomePositions = new Vector3[particleCount];

            ParticleSystem.MainModule main = _particleSystem.main;
            main.startSize = particleSize;
            main.startSpeed = 0; // We control speed manually
            main.startLifetime = Mathf.Infinity; // Live forever
            main.maxParticles = particleCount;
            main.loop = false; // We manage them ourselves
            main.simulationSpace = ParticleSystemSimulationSpace.World; // Important for mouse interaction

            ParticleSystem.EmissionModule emission = _particleSystem.emission;
            emission.enabled = false; // Disable standard emission

            ParticleSystem.ShapeModule shape = _particleSystem.shape;
            shape.enabled = false; // Disable standard shape emission

            // --- Manually create particles in a sphere ---
            for (int i = 0; i < particleCount; i++)
            {
                Vector3 homePos = Random.insideUnitSphere * orbRadius;
                _particleHomePositions[i] = homePos;

                _particles[i].position = transform.position + homePos; // Initial world position
                _particles[i].startColor = particleColor.Evaluate(Random.value); // Use gradient
                _particles[i].startSize = particleSize * Random.Range(0.8f, 1.2f); // Slight size variation
                _particles[i].startLifetime = Mathf.Infinity;
                _particles[i].remainingLifetime = Mathf.Infinity;
                _particles[i].velocity = Random.insideUnitSphere * idleDriftSpeed; // Start with gentle drift
            }

            _particleSystem.SetParticles(_particles, particleCount); // Apply the initial particles
            _particleSystem.Play(); // Need to call Play() even with manual control sometimes
        }

        private void Update()
        {
            // Optional: Slow orb rotation
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            // Get current particles from the system
            int numParticlesAlive = _particleSystem.GetParticles(_particles);

            // Get mouse position in world space (on the same Z-plane as the orb)
            Vector3 mouseScreenPos = Input.mousePosition;
            // Set Z to the distance from camera to the orb's center
            mouseScreenPos.z = _mainCamera.WorldToScreenPoint(transform.position).z;
            Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(mouseScreenPos);

            bool isMouseClose = (mouseWorldPos - transform.position).sqrMagnitude < _interactionRadiusSqr * 2f; // A bit wider check

            // Update each particle
            for (int i = 0; i < numParticlesAlive; i++)
            {
                Vector3 particleWorldPos = _particles[i].position;
                // Update home pos based on orb rotation/position
                Vector3 homeWorldPos = transform.TransformPoint(_particleHomePositions[i]);

                Vector3 vectorToMouse = mouseWorldPos - particleWorldPos;
                float distToMouseSqr = vectorToMouse.sqrMagnitude;

                Vector3 targetVelocity = _particles[i].velocity;

                // --- Interaction Logic ---
                if (isMouseClose && distToMouseSqr < _interactionRadiusSqr)
                {
                    float falloff = 1f - distToMouseSqr / _interactionRadiusSqr;

                    // Attract towards mouse
                    targetVelocity += vectorToMouse.normalized * mouseAttractStrength * falloff * Time.deltaTime;

                    // Add orbiting force (perpendicular to attraction vector and a world axis like Up)
                    Vector3 orbitDirection = Vector3.Cross(vectorToMouse.normalized, Vector3.up);
                    targetVelocity += orbitDirection * mouseOrbitStrength * falloff * Time.deltaTime;

                    // Damp velocity more when interacting
                    targetVelocity *= 1.0f - velocityDamping * 1.5f * Time.deltaTime;
                }
                else
                {
                    // Return towards home position
                    Vector3 vectorToHome = homeWorldPos - particleWorldPos;
                    targetVelocity += vectorToHome * returnToOrbStrength * Time.deltaTime;

                    // Add some gentle idle drift/swirl relative to home
                    targetVelocity += Random.insideUnitSphere * idleDriftSpeed * 0.1f * Time.deltaTime;

                    // Damp velocity
                    targetVelocity *= 1.0f - velocityDamping * Time.deltaTime;
                }

                _particles[i].velocity = targetVelocity;

                // Color shifts based on closeness
                float closeness = Mathf.Clamp01(1f - distToMouseSqr / _interactionRadiusSqr);
                _particles[i].startColor = particleColor.Evaluate(closeness);
            }

            // Apply the updated particles back to the system
            _particleSystem.SetParticles(_particles, numParticlesAlive);
        }

        // Helper to get a nice default gradient if user doesn't set one
        private static Gradient GetDefaultGradient()
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(0.5f, 0f, 1f), 0.0f),
                    new GradientColorKey(new Color(0f, 1f, 1f), 0.5f),
                    new GradientColorKey(Color.white, 1.0f)
                },
                new[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
            );
            return gradient;
        }

        // Draw gizmo in editor for interaction radius
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
