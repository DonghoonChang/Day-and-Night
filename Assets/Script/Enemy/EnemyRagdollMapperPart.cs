using UnityEngine;

public class EnemyRagdollMapperParts : MonoBehaviour {

    const float tightness = 25f;

    Transform matchingPart;
    Rigidbody rigidBody;

    public Transform MatchingPart
    {
        set
        {
            matchingPart = value;
        }
    }

    private void FixedUpdate()
    {
        if (matchingPart != null)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, matchingPart.localPosition, tightness * GameTime.deltaTime);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, matchingPart.localRotation, tightness * GameTime.deltaTime);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
