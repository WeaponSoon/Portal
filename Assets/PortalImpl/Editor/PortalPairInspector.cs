using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PortalPair))]
public class PortalPairInspector : Editor {

    PortalPair portalPair;
    static Portal portal;

    private void OnEnable()
    {
        portalPair = (PortalPair)target;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.BeginVertical();
        portal = (Portal)EditorGUILayout.ObjectField(portal, typeof(Portal),true);

        EditorGUILayout.BeginHorizontal();
        if (EditorGUILayout.DropdownButton(new GUIContent("测试A按钮"), FocusType.Passive))
        {
            var porIns = GameObject.Instantiate<GameObject>(portal.gameObject).GetComponent<Portal>();
            portalPair.portalA = porIns;
        }
        if (EditorGUILayout.DropdownButton(new GUIContent("测试B按钮"), FocusType.Passive))
        {
            var porIns = GameObject.Instantiate<GameObject>(portal.gameObject).GetComponent<Portal>();
            portalPair.portalB = porIns;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}
