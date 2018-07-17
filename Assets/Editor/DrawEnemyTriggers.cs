using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace MyGame.GUI
{
    [CustomEditor(typeof(Enemy.EnemyCharacter))]
    public class DrawEnemyTriggers : Editor
    {

        Enemy.EnemyCharacter t;


        private void OnEnable()
        {
            t = target as Enemy.EnemyCharacter;

            SceneView.onSceneGUIDelegate -= OnScene;
            SceneView.onSceneGUIDelegate += OnScene;
        }

        private void OnScene(SceneView view)
        {
            Handles.color = new Color(255f / 255f, 165f / 255f, 0f / 255f, 0.1f);
            Handles.DrawSolidDisc(t.transform.position, t.transform.up, t.GetTriggerRadiusWorldSpace());

            Handles.color = new Color(211f / 255f, 72f / 255f, 54f / 255f, 0.15f);
            Handles.DrawSolidArc(t.transform.position, t.transform.up, t.transform.forward, -t.FieldOfView, t.ChaseDistanceStart);
            Handles.DrawSolidArc(t.transform.position, t.transform.up, t.transform.forward, +t.FieldOfView, t.ChaseDistanceStart);

            Handles.color = new Color(72f / 255f, 133f / 255f, 237f / 255f, 0.1f);
            Handles.DrawSolidArc(t.transform.position, t.transform.up, t.transform.forward, -t.FieldOfView, t.ChaseDistanceStop);
            Handles.DrawSolidArc(t.transform.position, t.transform.up, t.transform.forward, +t.FieldOfView, t.ChaseDistanceStop);
        }

        void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= OnScene;

        }

        void OnDestory()
        {
            SceneView.onSceneGUIDelegate -= OnScene;
        }
    }
}

