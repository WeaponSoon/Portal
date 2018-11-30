using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PortalMainCamera : MonoBehaviour {
    [SerializeField]
    private PortalViewTree viewTree = new PortalViewTree();
    private TimeDebugger tdb = new TimeDebugger();
    private TimeDebugger tdr = new TimeDebugger();
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