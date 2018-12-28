using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PortalMainCamera : Thoughable {
    [SerializeField]
    private PortalViewTree viewTree = new PortalViewTree();
    private TimeDebugger tdb = new TimeDebugger();
    private TimeDebugger tdr = new TimeDebugger();
    private SphereCollider sphereCollider;
    private void Awake()
    {
        viewTree.rootCamera = GetComponent<Camera>();
        sphereCollider = GetComponent<SphereCollider>();
        if(sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
        }
        sphereCollider.center = Vector3.zero;
        var nearClip = GetComponent<Camera>().nearClipPlane;
        var vertAngle = GetComponent<Camera>().fieldOfView / 2;
        var heght = nearClip * Mathf.Tan(vertAngle/180 * Mathf.PI);
        var width = heght * GetComponent<Camera>().aspect * heght;
        var radius = new Vector3(heght, width, nearClip).magnitude;
        sphereCollider.radius = radius;
        
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	protected override void Update () {
        base.Update();
        viewTree.BuildPortalViewTree();
        viewTree.RenderPortalViewTree();
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination);
    }
}

public class TimeDebugger
{
    private Stopwatch sw = new Stopwatch();
    public long thisCountMs
    {
        get; private set;
    }
    public long maxCountMs
    {
        get; private set;
    }
    public long averageConutMs
    {
        get; private set;
    }
    public long sqrDiffMs
    {
        get; private set;
    }
    private long sqrAverageCountMs = 0;
    private long countTimes = 0;
    public void StartCount()
    {
        sw.Start();
    }
    public void EndCount()
    {
        sw.Stop();
        thisCountMs = sw.ElapsedMilliseconds;
        sw.Reset();
        ++countTimes;
        maxCountMs = (thisCountMs > maxCountMs ? thisCountMs : maxCountMs);
        averageConutMs = (averageConutMs * (countTimes - 1) + thisCountMs) / countTimes;
        sqrAverageCountMs = (sqrAverageCountMs * (countTimes - 1) + thisCountMs * thisCountMs) / countTimes;
        sqrDiffMs = sqrAverageCountMs - averageConutMs * averageConutMs;
    }

}