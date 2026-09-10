using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThiefCatcher : MonoBehaviour
{
    public string thiefTag = "Thief";
    public float explosionSpeed = 5f;
    public int particleCount = 30;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Transform root = hit.transform.root;
        if (!root.CompareTag(thiefTag)) return;

        Explode(root.gameObject);
    }

    void Explode(GameObject thief)
    {
        ExplosionEffect.Spawn(thief.transform.position, explosionSpeed, particleCount);
        Destroy(thief);

        if (ThiefManager.Instance != null)
            ThiefManager.Instance.ThiefCaught();
    }
}
