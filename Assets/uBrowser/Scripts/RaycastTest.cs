using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTest : MonoBehaviour {

	public float maxDetectDistance = -1;
	public LayerMask browserLayer;
	public GameObject testCube;
	public Camera cam;

	void Start() {
		if(cam == null)
			cam = GetComponent<Camera>();

	}

	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButtonDown(0)) { //left mouse key pressed down
			RaycastForBrowserPlane();
		}
	}


	void RaycastForBrowserPlane() {
		if(maxDetectDistance <= 0) {
			maxDetectDistance = float.MaxValue;
		} 

		Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray.origin, ray.direction, out hit, maxDetectDistance,browserLayer.value,QueryTriggerInteraction.Collide)) {
			testCube.transform.position = hit.point;
		}
	}
}
