using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrowserPlaneProperties : MonoBehaviour {
	public Transform centerPt;

	void Awake() {
		if(centerPt == null) {
			centerPt = GetComponent<Transform>();
		}
	}

}
