using UnityEngine;

/// <summary>
/// Spawns N small bright dots on the arch canvas with horizontal drift
/// and per-particle fade lifecycles. Each dot has its own random speed,
/// direction, and lifetime; particles wrap horizontally at canvas edges
/// (creating natural across-the-arch drift through the strip-and-rotate
/// transform) and respawn at random positions when their lifetime expires.
///
/// Attach to a GameObject under Arch_Scene with the Arch layer.
/// </summary>
public class ArchParticles : MonoBehaviour
{
    [Header("Population")]
    [Tooltip("Number of particles to maintain.")]
    public int particleCount = 200;

    [Header("Canvas extent (world units)")]
    [Tooltip("Half-width of the canvas in world units. Particle X spans -extentX to +extentX.")]
    public float extentX = 11.25f;

    [Tooltip("Half-height of the canvas in world units. Particle Y spans -extentY to +extentY.")]
    public float extentY = 5f;

    [Header("Particle appearance")]
    [Tooltip("Random size range for particle quads.")]
    public Vector2 sizeRange = new Vector2(0.04f, 0.10f);

    [Tooltip("Material for particle dots. Should be Unlit Transparent.")]
    public Material particleMaterial;

    [Header("Motion")]
    [Tooltip("Random horizontal drift speed range. Sign chosen randomly per particle.")]
    public Vector2 driftSpeedRange = new Vector2(0.3f, 1.2f);

    [Header("Lifetime")]
    [Tooltip("Random total lifetime range in seconds.")]
    public Vector2 lifetimeRange = new Vector2(4f, 12f);

    [Tooltip("Fraction of lifetime spent fading in (0..1).")]
    public float fadeInFraction = 0.2f;

    [Tooltip("Fraction of lifetime spent fading out (0..1).")]
    public float fadeOutFraction = 0.3f;

    private struct Particle
    {
        public Transform t;
        public MaterialPropertyBlock mpb;
        public Renderer renderer;
        public float age;       // seconds since spawn
        public float lifetime;  // total lifetime in seconds
        public float driftX;    // x velocity in world units per second (signed)
    }

    private Particle[] particles;

    void Start()
    {
        if (particleMaterial == null)
        {
            Debug.LogError("ArchParticles: particleMaterial not assigned.");
            return;
        }

        particles = new Particle[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Quad);
            dot.name = $"Particle_{i:D3}";
            dot.layer = gameObject.layer;
            dot.transform.SetParent(transform, false);
            dot.transform.localRotation = Quaternion.identity;

            Object.Destroy(dot.GetComponent<MeshCollider>());

            MeshRenderer mr = dot.GetComponent<MeshRenderer>();
            mr.sharedMaterial = particleMaterial;

            particles[i] = new Particle
            {
                t = dot.transform,
                mpb = new MaterialPropertyBlock(),
                renderer = mr,
                age = 0f,
                lifetime = 0f,
                driftX = 0f,
            };

            // Stagger initial lifetimes so particles don't all spawn/die together.
            // Each particle starts with random age within a random lifetime, so the
            // population is in steady-state from frame one.
            RespawnParticle(i, randomAge: true);
        }
    }

    void Update()
    {
        if (particles == null) return;

        float dt = Time.deltaTime;
        for (int i = 0; i < particles.Length; i++)
        {
            Particle p = particles[i];

            // Advance age
            p.age += dt;
            if (p.age >= p.lifetime)
            {
                particles[i] = p;
                RespawnParticle(i, randomAge: false);
                continue;
            }

            // Advance position
            Vector3 pos = p.t.localPosition;
            pos.x += p.driftX * dt;
            // Wrap horizontally
            if (pos.x > extentX) pos.x -= 2f * extentX;
            else if (pos.x < -extentX) pos.x += 2f * extentX;
            p.t.localPosition = pos;

            // Compute alpha from age
            float lifeFraction = p.age / p.lifetime;
            float alpha;
            if (lifeFraction < fadeInFraction)
            {
                alpha = lifeFraction / fadeInFraction;
            }
            else if (lifeFraction > 1f - fadeOutFraction)
            {
                alpha = (1f - lifeFraction) / fadeOutFraction;
            }
            else
            {
                alpha = 1f;
            }

            p.mpb.SetColor("_BaseColor", new Color(1f, 1f, 1f, alpha));
            p.renderer.SetPropertyBlock(p.mpb);

            particles[i] = p;
        }
    }

    private void RespawnParticle(int i, bool randomAge)
    {
        Particle p = particles[i];

        float x = Random.Range(-extentX, extentX);
        float y = Random.Range(-extentY, extentY);
        float size = Random.Range(sizeRange.x, sizeRange.y);

        p.t.localPosition = new Vector3(x, y, 0f);
        p.t.localScale = new Vector3(size, size, 1f);

        p.lifetime = Random.Range(lifetimeRange.x, lifetimeRange.y);
        p.age = randomAge ? Random.Range(0f, p.lifetime) : 0f;

        float speed = Random.Range(driftSpeedRange.x, driftSpeedRange.y);
        p.driftX = Random.value < 0.5f ? -speed : speed;

        particles[i] = p;
    }
}
