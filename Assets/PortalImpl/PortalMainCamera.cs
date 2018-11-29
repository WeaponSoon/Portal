using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PortalMainCamera : MonoBehaviour {
    [SerializeField]
    private PortalViewTree viewTree = new PortalViewTree();

    private void Awake()
    {
        viewTree.rootCamera = GetComponent<Camera>();   
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        viewTree.BuildPortalViewTree();
        viewTree.RenderPortalViewTree();
	}
    
}
