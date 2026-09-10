using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CopCatcher : MonoBehaviour
{
    public string thiefTag = "Thief";
    public float explosionSpeed = 5f;
    public int particleCount = 30;

    bool caught;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (caught) return;

        Transform root = hit.transform.root;
        if (!root.CompareTag(thiefTag)) return;

        Explode();
    }

    void Explode()
    {
        caught = true;

        ExplosionEffect.Spawn(transform.position, explosionSpeed, particleCount);

        GetComponent<CharacterController>().enabled = false;
        GetComponent<Drive>().enabled = false;

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = false;

        if (GameOverManager.Instance != null)
            GameOverManager.Instance.GameOver();
    }
}
