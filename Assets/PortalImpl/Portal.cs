using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ScreenPoatalArea
{
    public Rect scrrenRect;
    public float minDeep;
    public float maxDeep;
}

public class Portal : MonoBehaviour {

    public Shader portalPlaneShader;

    [HideInInspector]
    public Camera portalCamera
    {
        get;private set;
    }
    [SerializeField]
    private MeshRenderer portalPlaneRenderer;
    
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
    public Texture portalTexture
    {
        get
        {
            return portalPlaneRenderer.material.GetTexture("_MainTex");
        }
        set
        {
            portalPlaneRenderer.material.SetTexture("_MainTex", value);
        }
    }
    public RenderTexture portalCameraTarget
    {
        get
        {
            return portalCamera.targetTexture;
        }
        set
        {
            portalCamera.targetTexture = value;
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

    public ScreenPoatalArea GetPortalRect(Camera target)
    {
        var mesh = portalPlaneRenderer.GetComponent<MeshFilter>();
        if (mesh != null && mesh.mesh != null)
        {
            Bounds temBounds = mesh.mesh.bounds;
            Vector3[] vets = new Vector3[8];
            vets[0] = temBounds.center + 
                new Vector3(temBounds.extents.x, temBounds.extents.y, temBounds.extents.z);
            vets[1] = temBounds.center +
                new Vector3(temBounds.extents.x, temBounds.extents.y, -temBounds.extents.z);
            vets[2] = temBounds.center +
                new Vector3(temBounds.extents.x, -temBounds.extents.y, -temBounds.extents.z);
            vets[3] = temBounds.center +
                new Vector3(temBounds.extents.x, -temBounds.extents.y, temBounds.extents.z);
            vets[4] = temBounds.center +
                new Vector3(-temBounds.extents.x, temBounds.extents.y, temBounds.extents.z);
            vets[5] = temBounds.center +
                new Vector3(-temBounds.extents.x, temBounds.extents.y, -temBounds.extents.z);
            vets[6] = temBounds.center +
                new Vector3(-temBounds.extents.x, -temBounds.extents.y, -temBounds.extents.z);
            vets[7] = temBounds.center +
                new Vector3(-temBounds.extents.x, -temBounds.extents.y, temBounds.extents.z);
            
            for (int i = 0; i < vets.Length; ++i)
            {
                vets[i] = portalPlaneRenderer.transform.TransformPoint(vets[i]);
                vets[i] = target.WorldToScreenPoint(vets[i]);
            }
            float minDeep = vets[0].z;
            float maxDeep = vets[0].z;
            Rect rect = new Rect();
            rect.xMin = vets[0].x;
            rect.xMax = vets[0].x;
            rect.yMin = vets[0].y;
            rect.yMax = vets[0].y;
            for (int i = 0; i < vets.Length; ++i)
            {
                rect.xMin = vets[i].x < rect.xMin ? vets[i].x : rect.xMin;
                rect.xMax = vets[i].x > rect.xMax ? vets[i].x : rect.xMax;
                rect.yMin = vets[i].y < rect.yMin ? vets[i].y : rect.yMin;
                rect.yMax = vets[i].y > rect.yMax ? vets[i].y : rect.yMax;
                minDeep = vets[i].z < minDeep ? vets[i].z : minDeep;
                maxDeep = vets[i].z > maxDeep ? vets[i].z : maxDeep;
            }
            return new ScreenPoatalArea() { scrrenRect = rect, maxDeep = maxDeep, minDeep = minDeep };
        }
        return new ScreenPoatalArea() { scrrenRect=new Rect(0,0,1,1),minDeep = 0,maxDeep=0};
    }

    public bool ShouldCameraRender(Camera cam, ScreenPoatalArea spa)
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(cam);

        var mySpa = GetPortalRect(cam);
        float xMin = Mathf.Max(mySpa.scrrenRect.xMin, spa.scrrenRect.xMin);
        float xMax = Mathf.Min(mySpa.scrrenRect.xMax, spa.scrrenRect.xMax);
        float yMin = Mathf.Max(mySpa.scrrenRect.yMin, spa.scrrenRect.yMin);
        float yMax = Mathf.Min(mySpa.scrrenRect.yMax, spa.scrrenRect.yMax);
        
        return motherPair != null && motherPair.portalA != null && motherPair.portalB != null && 
           GeometryUtility.TestPlanesAABB(planes, portalBounds) &&
           xMin < xMax && yMin < yMax && mySpa.maxDeep > spa.minDeep &&
           Vector3.Dot(portalPlaneRenderer.transform.position - cam.transform.position, portalForward) < 0;
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
        
        portalCamera.enabled = false;
        portalCamera.transform.parent = portalPlaneRenderer.transform;
        portalPlaneRenderer.material = new Material(portalPlaneShader);
        
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
