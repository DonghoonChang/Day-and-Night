using UnityEngine;

public class EnemyRagdollMapperRoot : MonoBehaviour {

    public GameObject masterRig;
    public GameObject slaveRig;
    public Rigidbody slaveRigHips;

    Transform[] slaveRigTransforms;
    EnemyRagdollMapperParts[] slaveRigMappings;


    void Start () {

        slaveRigTransforms = slaveRig.GetComponentsInChildren<Transform>();
        for (int i = 1; i < slaveRigTransforms.Length; i++)
        {
            string relPath = (GetObjectPath(slaveRigTransforms[i]));
            Transform matchingPart = masterRig.transform.Find(relPath) as Transform;
            EnemyRagdollMapperParts mapping = slaveRigTransforms[i].gameObject.AddComponent<EnemyRagdollMapperParts>() as EnemyRagdollMapperParts;
            mapping.MatchingPart = matchingPart;

        }

        slaveRigMappings = slaveRig.GetComponentsInChildren<EnemyRagdollMapperParts>();
        slaveRigHips.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ
                                  | RigidbodyConstraints.FreezeRotationX| RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

	}

    string GetObjectPath(Transform target)
    {
        string path = "/" + target.name;

        Transform parentTF = target.parent;
        while (parentTF != slaveRig.transform)
        {
            path = "/" + parentTF.name + path;
            parentTF = parentTF.parent;
        }

        return path.Substring(1);
    }

    public void OnKilled()
    {
        slaveRigHips.constraints = RigidbodyConstraints.None;
        foreach(EnemyRagdollMapperParts mapping in slaveRigMappings)
            mapping.enabled = false;
    }

}
