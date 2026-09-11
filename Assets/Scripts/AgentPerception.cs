using UnityEngine;

// Shared "who is close to me" mechanism used by every agent's AI script.
// Looks up all GameObjects with the given tag(s) and returns the nearest one
// within range, or null if nothing qualifies.
public static class AgentPerception
{
    public static GameObject FindClosestWithTag(Vector3 origin, string tag, float maxRadius, GameObject exclude, out float distance)
    {
        return FindClosestWithTags(origin, new[] { tag }, maxRadius, exclude, out distance);
    }

    public static GameObject FindClosestWithTags(Vector3 origin, string[] tags, float maxRadius, GameObject exclude, out float distance)
    {
        GameObject closest = null;
        float closestSqrDist = maxRadius * maxRadius;

        foreach (string tag in tags)
        {
            if (string.IsNullOrEmpty(tag)) continue;

            foreach (GameObject candidate in GameObject.FindGameObjectsWithTag(tag))
            {
                if (candidate == exclude) continue;

                float sqrDist = (candidate.transform.position - origin).sqrMagnitude;
                if (sqrDist < closestSqrDist)
                {
                    closestSqrDist = sqrDist;
                    closest = candidate;
                }
            }
        }

        distance = closest != null ? Mathf.Sqrt(closestSqrDist) : Mathf.Infinity;
        return closest;
    }

    // Keeps an agent from flip-flopping between two similarly-close targets every
    // detection tick (which looked like a jerky snap toward a new direction).
    // If the current target is still in range and still has the expected tag,
    // callers should keep chasing/fleeing it instead of re-picking the "closest" one.
    public static bool StillValid(GameObject candidate, Vector3 origin, float maxRadius, string tag)
    {
        if (candidate == null || !candidate.CompareTag(tag)) return false;
        return (candidate.transform.position - origin).sqrMagnitude <= maxRadius * maxRadius;
    }

    public static bool StillValid(GameObject candidate, Vector3 origin, float maxRadius, string[] tags)
    {
        if (candidate == null) return false;
        if ((candidate.transform.position - origin).sqrMagnitude > maxRadius * maxRadius) return false;

        foreach (string tag in tags)
            if (!string.IsNullOrEmpty(tag) && candidate.CompareTag(tag)) return true;

        return false;
    }
}
