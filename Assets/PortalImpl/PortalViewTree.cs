﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PortalViewTree {

    public int maxLayer = 1;

    private int currentLayer = 0;
    public Camera rootCamera;
    private PortalNode rootNode = PortalNode.QueryNode(null);

    public static Vector3 ZeroV3 = new Vector3(0.0f, 0.0f, 0.0f);
    public static Vector3 OneV3 = new Vector3(1.0f, 1.0f, 1.0f);
    public static Vector3 ToV3(Vector4 v) { return new Vector3(v.x, v.y, v.z); }
    public static Quaternion QuaternionFromMatrix(Matrix4x4 m) { return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)); }
    public static Vector4 PosToV4(Vector3 v) { return new Vector4(v.x, v.y, v.z, 1.0f); }

    public void BuildPortalViewTree()
    {
        currentLayer = 0;
        rootNode.Recycle();
        BuildPortalViewTree(rootNode);
    }
    private void BuildPortalViewTree(PortalNode p)
    {
        if (currentLayer >= maxLayer)
            return;
        currentLayer++;
        
        Camera currentCam = p.thisPortal == null ? rootCamera : p.thisPortal.portalCamera;
        ScreenPoatalArea portalRect = p.thisPortal == null ? 
            new ScreenPoatalArea() { scrrenRect = new Rect(0, 0, Screen.width, Screen.height),minDeep = currentCam.nearClipPlane, maxDeep = currentCam.nearClipPlane } 
            : p.thisPortal.otherPortal.GetPortalRect(currentCam);
        foreach (var pair in PortalPair.portalPairs)
        {
            if(pair.portalA != null && pair.portalA.ShouldCameraRender(currentCam,portalRect))
            {
                p.nextPairs.Add(PortalNode.QueryNode(pair.portalA));
            }

            if (pair.portalB != null && pair.portalB.ShouldCameraRender(currentCam, portalRect))
            {
                p.nextPairs.Add(PortalNode.QueryNode(pair.portalB));
            }
        }
        foreach (var node in p.nextPairs)
        {
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

            node.position = portalCamera.transform.position;
            node.rotation = portalCamera.transform.rotation;

            // Calculate clip plane for portal (for culling of objects inbetween destination camera and portal)
            Vector4 clipPlaneWorldSpace = new Vector4(node.thisPortal.otherPortal.portalForward.x, node.thisPortal.otherPortal.portalForward.y,
                node.thisPortal.otherPortal.portalForward.z, Vector3.Dot(Destination.position, -node.thisPortal.otherPortal.portalForward));
            Vector4 clipPlaneCameraSpace = Matrix4x4.Transpose(Matrix4x4.Inverse(portalCamera.worldToCameraMatrix)) * clipPlaneWorldSpace;

            // Update projection based on new clip plane
            // Note: http://aras-p.info/texts/obliqueortho.html and http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
            portalCamera.projectionMatrix = rootCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
            node.projMat = portalCamera.projectionMatrix;
            BuildPortalViewTree(node);
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
            p.thisPortal.portalCamera.Render();
        }
        else
        {
            //rootCamera.Render();
        }
    }
}

public class PortalNode
{
    private const int MAX_POOL_COUNT = 50;
    private static Queue<PortalNode> pool = new Queue<PortalNode>();
    public static PortalNode QueryNode(Portal v)
    {
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
        return new PortalNode(v);
    }
    public bool inQueue = false;
    public Vector3 position;
    public Quaternion rotation = Quaternion.identity;
    public Portal thisPortal;
    public Matrix4x4 projMat;
    public List<PortalNode> nextPairs = new List<PortalNode>();
    private PortalNode(Portal v)
    {
        this.thisPortal = v;
    }
    public void Recycle()
    {
        foreach (var node in nextPairs)
        {
            node.Recycle();
            node.nextPairs.Clear();
            node.thisPortal = null;
            
            if(pool.Count < MAX_POOL_COUNT && !node.inQueue)
            {
                node.inQueue = true;
                pool.Enqueue(node);
            }
        }
        nextPairs.Clear();
    }
}