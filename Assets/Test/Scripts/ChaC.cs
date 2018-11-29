using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaC : MonoBehaviour {
    public GameObject fpsCamera;
    public float yawRatio = 60;
    public float pitchRatio = 60;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        float forward = Input.GetAxis("Vertical");
        float right = Input.GetAxis("Horizontal");
        float yaw = Input.GetAxis("Mouse X");
        float pitch = Input.GetAxis("Mouse Y");

        Move(forward, right);
        Rot(yaw, pitch);
#endif
        GetComponent<CharacterController>().Move(-Vector3.up * 9.8f);
        //Vector3 moveDir = forward * transform.forward + right * transform.right;
        //GetComponent<CharacterController>().Move(moveDir * Time.deltaTime * 5);
        //gameObject.transform.Rotate(Vector3.up, yaw * Time.deltaTime * yawRatio,Space.World);
        //gameObject.transform.Rotate(transform.right, -pitch * Time.deltaTime * pitchRatio,Space.World);
        //GetComponent<CharacterController>().Move(-Vector3.up * 9.8f);
    }

    public void Move(float forward, float right, bool inlocal = true)
    {
       if(inlocal)
       {
            Vector3 moveDir = forward * Vector3.ProjectOnPlane(transform.forward, transform.up).normalized + right * transform.right;
            GetComponent<CharacterController>().Move(moveDir * Time.deltaTime * 5);
            
       }
    }
    public void Rot(float yaw, float pitch)
    {
        gameObject.transform.Rotate(Vector3.up, yaw * Time.deltaTime * yawRatio, Space.World);
        fpsCamera.transform.Rotate(transform.right, -pitch * Time.deltaTime * pitchRatio, Space.World);
    }

}
