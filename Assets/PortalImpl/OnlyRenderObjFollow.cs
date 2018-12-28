using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyRenderObjFollow : MonoBehaviour {
    public Transform originTransform;
    public Portal portal;
	// Update is called once per frame
	void LateUpdate () {
        if (originTransform == null)
            return;
        if (portal == null || portal.otherPortal == null) 
        {
            transform.position = originTransform.position;
            transform.rotation = originTransform.rotation;
            transform.localScale = originTransform.localScale;
        }
        else
        {
            Transform Source = portal.transform;
            Transform Destination = portal.otherPortal.portalPlaneTransform;

            Matrix4x4 destinationFlipRotation = Matrix4x4.TRS(Vector3.zero, Quaternion.AngleAxis(180.0f, Vector3.up), PortalViewTree.OneV3);
            Matrix4x4 sourceInvMat = destinationFlipRotation * Source.worldToLocalMatrix;

            Vector3 cameraPositionInSourceSpace = PortalViewTree.ToV3(sourceInvMat * PortalViewTree.PosToV4(originTransform.position));
            Quaternion cameraRotationInSourceSpace = Quaternion.AngleAxis(180, Vector3.up) * Quaternion.Inverse(Source.rotation) * originTransform.rotation;
            
            transform.position = Destination.TransformPoint(cameraPositionInSourceSpace);
            transform.rotation = Destination.rotation * cameraRotationInSourceSpace;
            transform.localScale = originTransform.localScale;
        }
    }
}
