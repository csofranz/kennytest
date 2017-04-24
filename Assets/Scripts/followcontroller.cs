using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followcontroller : MonoBehaviour {

	//
	// script that makes this object follow the leader
	// you can config offset x, y, z
	//
	public GameObject theLeader; // we'll follow this object around
	public float offset;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		// ok, set my position to that of the leader
		Vector3 thePos = theLeader.transform.position;
		Quaternion theRot = theLeader.transform.rotation;
		GameObject self = gameObject;
		self.transform.position = thePos;
		self.transform.rotation = theRot;

		self.transform.Translate (Vector3.forward * offset);

	}
}
