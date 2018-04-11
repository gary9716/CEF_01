using UnityEngine;
using System.Collections;

public class GetBrowserTexture : MonoBehaviour {


    private Material mMtl;

	// Use this for initialization
	void Start ()
    {
		BaseCEFClient client = CEFManager.instance.CreateBrowser(null, "www.google.com");
        mMtl = GetComponent<MeshRenderer>().material;
        if(client != null && client.BrowserTexture != null) {
			mMtl.SetTexture("_MainTex", client.BrowserTexture);
		}
		
	}

}
