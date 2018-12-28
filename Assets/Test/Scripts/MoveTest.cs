using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour {

    public GameObject testo;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        var ot = GetComponent<Camera>().WorldToViewportPoint(testo.transform.position);
        Debug.Log(ot);
        ot = GetComponent<Camera>().WorldToScreenPoint(testo.transform.position);
        Debug.Log(ot);
        //GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * 0.3f * Time.deltaTime);
    }
}
