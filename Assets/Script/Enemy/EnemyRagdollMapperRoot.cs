using UnityEngine;

public class EnemyRagdollMapperRoot : MonoBehaviour {

    public Transform masterRig;
    public Transform slaveRig;
    public Rigidbody slaveRigHips;

    Collider[] _colliders;
    Transform[] _slaveRigTransforms;
    EnemyRagdollMapperParts[] _slaveRigMappings;


    void Start () {

        _colliders = slaveRig.GetComponentsInChildren<Collider>();
        _slaveRigTransforms = slaveRig.GetComponentsInChildren<Transform>();

        for (int i = 1; i < _slaveRigTransforms.Length; i++)
        {
            string relPath = (GetObjectPath(_slaveRigTransforms[i]));
            Transform matchingPart = masterRig.Find(relPath) as Transform;
            EnemyRagdollMapperParts mapping = _slaveRigTransforms[i].gameObject.AddComponent<EnemyRagdollMapperParts>() as EnemyRagdollMapperParts;
            mapping.MatchingPart = matchingPart;

        }

        _slaveRigMappings = slaveRig.GetComponentsInChildren<EnemyRagdollMapperParts>();
        slaveRigHips.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;

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

        foreach (Collider col in _colliders)
            col.isTrigger = false;

        foreach(EnemyRagdollMapperParts mapping in _slaveRigMappings)
            mapping.enabled = false;
    }

}
