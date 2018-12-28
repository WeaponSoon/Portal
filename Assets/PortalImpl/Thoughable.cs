using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thoughable : MonoBehaviour {
    private List<Portal> nearPortals = new List<Portal>();
    private Portal thoughingPortal = null;
	
    public Portal ThonghingPortal
    {
        get
        {
            return thoughingPortal;
        }
    }

    public void AddNearPortal(Portal portal)
    {
        nearPortals.AddAsSet(portal);
    }
    public void RemovePortal(Portal portal)
    {
        if(nearPortals.Contains(portal))
        {
            nearPortals.Remove(portal);
        }
    }
    // Use this for initialization
	void Start () {
		
	}
	
    public void UpdateNearestPotalInRange()
    {
        thoughingPortal = null;
        for (int i = 0; i < nearPortals.Count; ++i)
        {
            if (thoughingPortal == null ||
                (gameObject.transform.position - thoughingPortal.transform.position).sqrMagnitude > (gameObject.transform.position - nearPortals[i].transform.position).sqrMagnitude)
            {
                thoughingPortal = nearPortals[i];
            }
        }
    }

	// Update is called once per frame
	protected virtual void Update () {
        var lastPortal = thoughingPortal;
        UpdateNearestPotalInRange();
        if(lastPortal != null && lastPortal.GetComponent<Collider>() != null)
            Physics.IgnoreCollision(GetComponent<Collider>(), lastPortal.GetComponent<Collider>(), false);
        if (ThonghingPortal != null && ThonghingPortal.GetComponent<Collider>() != null)
            Physics.IgnoreCollision(GetComponent<Collider>(), ThonghingPortal.GetComponent<Collider>(), true);
    }
}
