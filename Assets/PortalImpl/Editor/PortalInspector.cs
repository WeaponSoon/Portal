using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Portal))]
public class PortalInspector : Editor {

    Portal portal;

    private void OnEnable()
    {
        portal = (Portal)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.BeginVertical();
        if (EditorGUILayout.DropdownButton(new GUIContent("测试按钮"), FocusType.Passive))
        {
            portal.ShouldCameraRender(Camera.main);
        }
        EditorGUILayout.EndVertical();
    }
}
