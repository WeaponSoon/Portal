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

    private List<Thoughable> inRangeThoughable = new List<Thoughable>();

    [HideInInspector]
    public Camera portalCamera
    {
        get;private set;
    }
    [SerializeField]
    private MeshRenderer portalPlaneRenderer;

    private Vector3[] _vertecis = new Vector3[4];
    public Vector3[] vertecis
    {
        get
        {
            return _vertecis;
        }
    }

    public float SizeX = 2;
    public float SizeY = 3;
    public float ThoughableRangeZLength = 6;
    public float doorZLength = 0.05f;

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

    private BoxCollider thoughableRange;
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
            Vector3[] vets = new Vector3[4];
            //vets[0] = temBounds.center +
            //    new Vector3(temBounds.extents.x, temBounds.extents.y, temBounds.extents.z);
            //vets[1] = temBounds.center +
            //    new Vector3(temBounds.extents.x, temBounds.extents.y, -temBounds.extents.z);
            //vets[2] = temBounds.center +
            //    new Vector3(temBounds.extents.x, -temBounds.extents.y, -temBounds.extents.z);
            //vets[3] = temBounds.center +
            //    new Vector3(temBounds.extents.x, -temBounds.extents.y, temBounds.extents.z);
            //vets[4] = temBounds.center +
            //    new Vector3(-temBounds.extents.x, temBounds.extents.y, temBounds.extents.z);
            //vets[5] = temBounds.center +
            //    new Vector3(-temBounds.extents.x, temBounds.extents.y, -temBounds.extents.z);
            //vets[6] = temBounds.center +
            //    new Vector3(-temBounds.extents.x, -temBounds.extents.y, -temBounds.extents.z);
            //vets[7] = temBounds.center +
            //    new Vector3(-temBounds.extents.x, -temBounds.extents.y, temBounds.extents.z);

            for (int i = 0; i < vets.Length; ++i)
            {
                vets[i] = portalPlaneRenderer.transform.TransformPoint(vertecis[i]);
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
        _vertecis[0] = new Vector3(-SizeX / 2, SizeY / 2, 0);
        _vertecis[1] = new Vector3(SizeX / 2, SizeY / 2, 0);
        _vertecis[2] = new Vector3(SizeX / 2, -SizeY / 2, 0);
        _vertecis[3] = new Vector3(-SizeX / 2, -SizeY / 2, 0);
        int[] indices = new int[] { 3,1,0,3,2,1};
        Mesh mesh = new Mesh();
        mesh.SetVertices(new List<Vector3>(vertecis));
        mesh.SetTriangles(indices, 0);
        GetComponent<MeshFilter>().mesh = mesh;
        thoughableRange = gameObject.AddComponent<BoxCollider>();
        thoughableRange.isTrigger = false;
        thoughableRange.center = new Vector3(0, 0, 0);
        thoughableRange.size = new Vector3(SizeX, SizeY, doorZLength);
        
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
        for(int i = 0; i < inRangeThoughable.Count; ++i)
        {
            inRangeThoughable[i].RemovePortal(this);
        }
    }
    private void FixedUpdate()
    {
        OnMyCollision();
    }
    private void OnMyCollision()
    {
        var pos = gameObject.transform.TransformPoint(new Vector3(0, 0, ThoughableRangeZLength / 2));
        Matrix4x4 scale = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, gameObject.transform.lossyScale);
        Collider[] newInRangeTem = Physics.OverlapBox(pos, scale.MultiplyVector(new Vector3(SizeX, SizeY, ThoughableRangeZLength)) / 2, 
            gameObject.transform.rotation);
        List<Thoughable> newInRange = new List<Thoughable>();
        for(int i = 0; i < newInRangeTem.Length; ++i)
        {
            if(newInRangeTem[i].GetComponent<Thoughable>() != null)
            {
                newInRange.Add(newInRangeTem[i].GetComponent<Thoughable>());
            }
        }
        List<Thoughable> willOut = inRangeThoughable.Diff(newInRange);
        List<Thoughable> willIn = newInRange.Diff(inRangeThoughable);
        //TODO call func
        for(int i = 0; i < willOut.Count; ++i)
        {
            willOut[i].RemovePortal(this);
        }
        for (int i = 0; i < willIn.Count; ++i)
        {
            willIn[i].AddNearPortal(this);
        }
        inRangeThoughable = newInRange;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var tem = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(gameObject.transform.position, transform.rotation, transform.lossyScale);
        Gizmos.DrawWireCube(new Vector3(0, 0, ThoughableRangeZLength / 2), new Vector3(SizeX, SizeY, ThoughableRangeZLength));
        Gizmos.matrix = tem;
    }
}
