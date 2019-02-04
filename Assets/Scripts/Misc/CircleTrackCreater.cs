using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

[CustomEditor(typeof(CircleTrackCreater))]
public class CircleTrackCreaterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(GUI.Button(EditorGUILayout.GetControlRect(), "Create Track"))
        {
            CircleTrackCreater trackCreater = target as CircleTrackCreater;

            trackCreater.Path.m_Waypoints = new Cinemachine.CinemachineSmoothPath.Waypoint[trackCreater.Nodes];

            for (int i = 0; i < trackCreater.Nodes; i++)
            {
                float currentRadiens = 360f / trackCreater.Nodes * i * Mathf.Deg2Rad;

                var waypoint = new Cinemachine.CinemachineSmoothPath.Waypoint();

                waypoint.position = new Vector3(Mathf.Cos(currentRadiens) * trackCreater.Radius, trackCreater.YValue, Mathf.Sin(currentRadiens) * trackCreater.Radius);

                trackCreater.Path.m_Waypoints[i] = waypoint;
            }
        }
    }
}

#endif

public class CircleTrackCreater : MonoBehaviour
{
    public Cinemachine.CinemachineSmoothPath Path;

    public float YValue;
    public float Radius;
    public int Nodes;
}
