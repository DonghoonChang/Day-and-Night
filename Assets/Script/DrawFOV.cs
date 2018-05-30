using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyNavController))]
public class DrawFOV : Editor {

    EnemyNavController t;

    private void OnEnable()
    {
        t = (EnemyNavController)target;
    }
    private void OnSceneGUI()
    {
        Handles.color = new Color(1, 1, 1, 0.2f);
        Handles.DrawSolidArc(t.transform.position, t.transform.up, t.transform.forward, -45, t.fovRadius);
        Handles.DrawSolidArc(t.transform.position, t.transform.up, t.transform.forward, 45, t.fovRadius);
    }
}
