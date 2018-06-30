using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace MyGame.GUI
{
    [CustomEditor(typeof(Enemy.Enemy))]
    public class DrawEnemyTriggers : Editor
    {

        Enemy.Enemy t;


        private void OnEnable()
        {
            t = target as Enemy.Enemy;

            SceneView.onSceneGUIDelegate -= OnScene;
            SceneView.onSceneGUIDelegate += OnScene;
        }

        private void OnScene(SceneView view)
        {
            Handles.color = new Color(60f / 255f, 186 / 255f, 84 / 255f, 0.1f);
            Handles.DrawSolidDisc(t.transform.position, t.transform.up, t.alertTriggers.watchDistance);

            Handles.color = new Color(255f / 255f, 165f / 255f, 0f / 255f, 0.1f);
            Handles.DrawSolidDisc(t.transform.position, t.transform.up, t.alertTriggers.followDistance);

            Handles.color = new Color(211f / 255f, 72f / 255f, 54f / 255f, 0.1f);
            Handles.DrawSolidDisc(t.transform.position, t.transform.up, t.alertTriggers.attackDistance);

            Handles.color = new Color(211f / 255f, 72f / 255f, 54f / 255f, 0.15f);
            Handles.DrawSolidArc(t.transform.position, t.transform.up, t.transform.forward, -t.alertTriggers.FieldOfView, t.alertTriggers.chaseDistance);
            Handles.DrawSolidArc(t.transform.position, t.transform.up, t.transform.forward, +t.alertTriggers.FieldOfView, t.alertTriggers.chaseDistance);

            Handles.color = new Color(72f / 255f, 133f / 255f, 237f / 255f, 0.1f);
            Handles.DrawSolidArc(t.transform.position, t.transform.up, t.transform.forward, -t.alertTriggers.FieldOfView, t.alertTriggers.stopChaseDistance);
            Handles.DrawSolidArc(t.transform.position, t.transform.up, t.transform.forward, +t.alertTriggers.FieldOfView, t.alertTriggers.stopChaseDistance);


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

