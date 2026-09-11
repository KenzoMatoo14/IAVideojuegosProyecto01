using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThiefCatcher : MonoBehaviour
{
    public string thiefTag = "Thief";
    public float explosionSpeed = 5f;
    public int particleCount = 30;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // hit.transform.root would be the top of the ENTIRE hierarchy - if thieves
        // are grouped under an organizing empty GameObject (e.g. "Robbers") in the
        // scene, that container is what gets returned, not the individual thief,
        // and it doesn't have the tag - so the catch silently never fired. Walk up
        // from the collider instead until we find the actual tagged thief.
        Transform thief = FindTaggedAncestor(hit.transform, thiefTag);
        if (thief == null) return;

        Explode(thief.gameObject);
    }

    static Transform FindTaggedAncestor(Transform current, string tag)
    {
        while (current != null)
        {
            if (current.CompareTag(tag)) return current;
            current = current.parent;
        }
        return null;
    }

    void Explode(GameObject thief)
    {
        ExplosionEffect.Spawn(thief.transform.position, explosionSpeed, particleCount);
        Destroy(thief);

        if (ThiefManager.Instance != null)
            ThiefManager.Instance.ThiefCaught();
    }
}
