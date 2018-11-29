using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {

    public Shader portalPlaneShader;

    [HideInInspector]
    public Camera portalCamera
    {
        get;private set;
    }
    [SerializeField]
    private Renderer portalPlaneRenderer;
    
    public Transform portalPlaneTransform
    {
        get
        {
            return portalPlaneRenderer.transform;
        }
    }
    
    private PortalPair _motherPair = null;
    public PortalPair motherPair
    {
        get
        {
            return _motherPair;
        }
        set
        {
            _motherPair = value;
        }
    }

    public Portal otherPortal
    {
        get
        {
            if(motherPair == null)
            {
                return null;
            }
            return motherPair.portalA == this ? motherPair.portalB : motherPair.portalA;
        }
    }

    public Bounds portalBounds
    {
        get
        {
            return portalPlaneRenderer.bounds;
        }
    }

    public Vector3 portalForwardInPlaneLocalSpace = new Vector3(0,0,-1);
    public Vector3 portalForward
    {
        get
        {
            portalForwardInPlaneLocalSpace.Normalize();
            if (portalForwardInPlaneLocalSpace.sqrMagnitude <= 0.01f)
            {
                return portalPlaneRenderer.transform.forward;
            }
            var basX = portalPlaneRenderer.transform.right;
            var basY = portalPlaneRenderer.transform.up;
            var basZ = portalPlaneRenderer.transform.forward;
            return
                (portalForwardInPlaneLocalSpace.x * basX +
                portalForwardInPlaneLocalSpace.y * basY +
                portalForwardInPlaneLocalSpace.z * basZ);
        }
        
    }

    public bool ShouldCameraRender(Camera cam)
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(cam);
        

        return motherPair != null && motherPair.portalA != null && motherPair.portalB != null && 
           GeometryUtility.TestPlanesAABB(planes, portalBounds) &&
           Vector3.Dot(cam.transform.forward, portalForward) < 0;
    }

	public virtual void OnConnected(PortalPair pair)
    {

    }
    public virtual void OnDisconnected(PortalPair pair)
    {

    }

    RenderTexture rt;
    private void Awake()
    {
        if(portalPlaneShader == null)
        {
            portalPlaneShader = Shader.Find("Unlit/PortalPlaneShader");
        }
        rt = new RenderTexture(Screen.width, Screen.height, 24);
        portalCamera = new GameObject("PortalCamera",typeof(Camera)).GetComponent<Camera>();
        portalCamera.targetTexture = rt;
        portalCamera.enabled = false;
        portalCamera.transform.parent = portalPlaneRenderer.transform;
        portalPlaneRenderer.material = new Material(portalPlaneShader);
        portalPlaneRenderer.material.SetTexture("_MainTex",rt);
    }
    private void OnDestroy()
    {
        if(motherPair != null)
        {
            if (motherPair.portalA == this)
                motherPair.portalA = null;
            else if (motherPair.portalB == this)
                motherPair.portalB = null;
        }
    }

}
