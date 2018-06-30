using System.Collections;
using UnityEngine;

public class EnemyRagdollMapperParts : MonoBehaviour {

    Transform matchingPart;
    Rigidbody rigidBody;
    float tightness;

    private void OnEnable()
    {
        tightness = 25f;
    }

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
            transform.localPosition = Vector3.Lerp(transform.localPosition, matchingPart.localPosition, tightness * Time.deltaTime);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, matchingPart.localRotation, tightness * Time.deltaTime);
            //transform.localPosition = matchingPart.localPosition;
            //transform.localRotation = matchingPart.localRotation;
        }

        else
            Debug.LogWarning(transform.name + "is Without Its Matching Part");
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
