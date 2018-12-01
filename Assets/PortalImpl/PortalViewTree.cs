using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PortalViewTree {

    public int maxLayer = 1;

    //private int currentLayer = 0;
    public Camera rootCamera;
    private PortalNode rootNode;// 

    public static Vector3 ZeroV3 = new Vector3(0.0f, 0.0f, 0.0f);
    public static Vector3 OneV3 = new Vector3(1.0f, 1.0f, 1.0f);
    public static Vector3 ToV3(Vector4 v) { return new Vector3(v.x, v.y, v.z); }
    public static Quaternion QuaternionFromMatrix(Matrix4x4 m) { return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)); }
    public static Vector4 PosToV4(Vector3 v) { return new Vector4(v.x, v.y, v.z, 1.0f); }

    public void BuildPortalViewTree()
    {
        if(rootNode == null)
        {
            rootNode = PortalNode.QueryNode(null, -1);
        }
        //currentLayer = 0;
        rootNode.Recycle();
        BuildPortalViewTree(rootNode, 0);
    }
    private void BuildPortalViewTree(PortalNode p, int lastDepth)
    {
        if (lastDepth >= maxLayer)
            return;

        lastDepth++;
        
        Camera currentCam = p.thisPortal == null ? rootCamera : p.thisPortal.portalCamera;
        
        if (p.thisPortal != null)
        {
            currentCam.transform.position = p.position;
            currentCam.transform.rotation = p.rotation;
            currentCam.projectionMatrix = p.projMat;
            currentCam.targetTexture = p.rt;
        }
        
        ScreenPoatalArea portalRect = p.thisPortal == null ? 
            new ScreenPoatalArea() { scrrenRect = new Rect(0, 0, Screen.width, Screen.height),minDeep = currentCam.nearClipPlane, maxDeep = currentCam.nearClipPlane } 
            : p.thisPortal.otherPortal.GetPortalRect(currentCam);
        foreach (var pair in PortalPair.portalPairs)
        {
            if(pair.portalA != null  && pair.portalA.ShouldCameraRender(currentCam,portalRect))
            {
                p.nextPairs.Add(PortalNode.QueryNode(pair.portalA, lastDepth-1));
            }

            if (pair.portalB != null  && pair.portalB.ShouldCameraRender(currentCam, portalRect))
            {
                p.nextPairs.Add(PortalNode.QueryNode(pair.portalB, lastDepth - 1));
            }
        }
        
        foreach (var node in p.nextPairs)
        {
            if (p.thisPortal != null)
            {
                currentCam.transform.position = p.position;
                currentCam.transform.rotation = p.rotation;
                currentCam.projectionMatrix = p.projMat;
                currentCam.targetTexture = p.rt;
            }
            Transform Source = node.thisPortal.portalPlaneTransform;
            Transform Destination = node.thisPortal.otherPortal.portalPlaneTransform;
            Camera portalCamera = node.thisPortal.portalCamera;
            
            // Rotate Source 180 degrees so PortalCamera is mirror image of MainCamera
            Matrix4x4 destinationFlipRotation = Matrix4x4.TRS(ZeroV3, Quaternion.AngleAxis(180.0f, Vector3.up), OneV3);
            Matrix4x4 sourceInvMat = destinationFlipRotation * Source.worldToLocalMatrix;

            // Calculate translation and rotation of MainCamera in Source space
            Vector3 cameraPositionInSourceSpace = ToV3(sourceInvMat * PosToV4(currentCam.transform.position));
            Quaternion cameraRotationInSourceSpace = Quaternion.AngleAxis(180, Vector3.up) * Quaternion.Inverse(Source.rotation) * currentCam.transform.rotation;

            // Transform Portal Camera to World Space relative to Destination transform,
            // matching the Main Camera position/orientation
            portalCamera.transform.position = Destination.TransformPoint(cameraPositionInSourceSpace);
            portalCamera.transform.rotation = Destination.rotation * cameraRotationInSourceSpace;

            node.position = Destination.TransformPoint(cameraPositionInSourceSpace);
            node.rotation = Destination.rotation * cameraRotationInSourceSpace;

            // Calculate clip plane for portal (for culling of objects inbetween destination camera and portal)
            Vector4 clipPlaneWorldSpace = new Vector4(node.thisPortal.otherPortal.portalForward.x, node.thisPortal.otherPortal.portalForward.y,
                node.thisPortal.otherPortal.portalForward.z, Vector3.Dot(Destination.position, -node.thisPortal.otherPortal.portalForward));
            Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(portalCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;

            // Update projection based on new clip plane
            // Note: http://aras-p.info/texts/obliqueortho.html and http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
            //portalCamera.projectionMatrix = rootCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
            node.projMat = rootCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
            BuildPortalViewTree(node, lastDepth);
        }
    }

    public void RenderPortalViewTree()
    {
        
        RenderPortalViewTree(rootNode);
    }
    private void RenderPortalViewTree(PortalNode p)
    {
        foreach (var node in p.nextPairs)
        {
            RenderPortalViewTree(node);
        }
        if(p.thisPortal != null)
        {
            p.thisPortal.portalCamera.transform.position = p.position;
            p.thisPortal.portalCamera.transform.rotation = p.rotation;
            p.thisPortal.portalCamera.projectionMatrix = p.projMat;
            p.thisPortal.portalCameraTarget = p.rt;
            foreach (var node in p.nextPairs)
            {
                node.thisPortal.portalTexture = node.rt;
            }
            p.thisPortal.portalCamera.Render();
        }
        else
        {
            foreach (var node in p.nextPairs)
            {
                node.thisPortal.portalTexture = node.rt;
            }
            rootCamera.Render();
        }
    }
}

public class PortalNode
{
    private const int MAX_POOL_COUNT = 50;
    private static Queue<PortalNode> highResPool = new Queue<PortalNode>();
    private static Queue<PortalNode> middleResPool = new Queue<PortalNode>();
    private static Queue<PortalNode> lowResPool = new Queue<PortalNode>();
    private static readonly Vector2[] RES_LAYER = new Vector2[] {
        new Vector2(Screen.width, Screen.height),
        new Vector2(Screen.width/1.414f, Screen.height/1.414f),
        new Vector2(Screen.width/2, Screen.height/2)
    };
    public static PortalNode QueryNode(Portal v, int layer)
    {
        Queue<PortalNode> pool;
        if (layer <= 0)
            pool = highResPool;
        else if (layer == 1)
            pool = middleResPool;
        else
            pool = lowResPool;
        
        if(pool.Count > 0)
        {
            var ret = pool.Dequeue();
            ret.thisPortal = v;
            ret.rotation = Quaternion.identity;
            ret.position = Vector3.zero;
            ret.projMat = Matrix4x4.identity;
            ret.inQueue = false;
            
            return ret;
        }
        Debug.Log("New Node Instance!");
        layer = Mathf.Clamp(layer, 0, 2);
        var res = RES_LAYER[layer];
        var temp = new PortalNode(v, pool);
        temp.rt = new RenderTexture((int)res.x, (int)res.y, 24);
        return temp;
    }
    public Queue<PortalNode> belonePool;
    public RenderTexture rt;
    public bool inQueue = false;
    public Vector3 position;
    public Quaternion rotation = Quaternion.identity;
    public Portal thisPortal;
    public Matrix4x4 projMat;
    public List<PortalNode> nextPairs = new List<PortalNode>();
    private PortalNode(Portal v, Queue<PortalNode> pool)
    {
        belonePool = pool;
        
        this.thisPortal = v;
    }
    public void Recycle()
    {
        foreach (var node in nextPairs)
        {
            node.Recycle();
            node.nextPairs.Clear();
            node.thisPortal = null;
            
            if(node.belonePool.Count < MAX_POOL_COUNT && !node.inQueue)
            {
                node.inQueue = true;
                node.belonePool.Enqueue(node);
            }
        }
        nextPairs.Clear();
    }
}