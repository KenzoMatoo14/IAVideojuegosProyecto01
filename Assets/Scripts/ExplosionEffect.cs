using UnityEngine;

public static class ExplosionEffect
{
    public static void Spawn(Vector3 position, float speed = 5f, int particleCount = 30)
    {
        GameObject fx = new GameObject("ExplosionFX");
        fx.transform.position = position;

        ParticleSystem ps = fx.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = speed;
        main.startSize = 0.3f;
        main.startColor = Color.red;
        main.duration = 0.3f;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)particleCount) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
        psRenderer.material = new Material(Shader.Find("Sprites/Default"));

        Object.Destroy(fx, main.startLifetime.constant + 0.5f);
    }
}
