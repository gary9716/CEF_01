using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xilium.CefGlue;
using System;
using System.Runtime.InteropServices;

public class BaseCEFClient : CefClient {

	private readonly BaseCEFClientLoadHandler   mLoadHandler;
    private readonly BaseCEFClientRenderHandler mRenderHandler;

	public System.Object    sPixelLock;
    public byte[]           sPixelBuffer;

	public Texture2D BrowserTexture;
	public CefBrowserHost sHost;

	public BaseCEFClient(int pageWidth, int pageHeight) {
		sPixelLock = new object();
        sPixelBuffer = new byte[pageWidth * pageHeight * 4];
		BrowserTexture = new Texture2D(pageWidth, pageHeight);
		mLoadHandler = new BaseCEFClientLoadHandler(this);
		mRenderHandler = new BaseCEFClientRenderHandler(this);
	}

	public void UpdateTexture()
    {
        if (sHost != null && BrowserTexture != null)
        {
            lock (sPixelLock)
            {
                BrowserTexture.LoadRawTextureData(sPixelBuffer);
                BrowserTexture.Apply();
            }
        }
    }

	//called by CEFManager in MessagePump Coroutine
	public virtual void Update() {
		UpdateTexture();
	}

	public virtual void Shutdown() {
		if(sHost != null) {
			sHost.Dispose();
		}
	}

	#region Interface 
    protected override CefRenderHandler GetRenderHandler()
    {
        return mRenderHandler;
    }

    protected override CefLoadHandler GetLoadHandler()
    {
        return mLoadHandler;
    }
    #endregion

	#region Handlers
    public class BaseCEFClientLoadHandler : CefLoadHandler
    {
		BaseCEFClient mClient;
        public BaseCEFClientLoadHandler(BaseCEFClient client)
        {
			mClient = client;
        }

        protected override void OnLoadStart(CefBrowser browser, CefFrame frame)
        {
            Debug.Log("load handler load start...");
            if (browser != null) {
                mClient.sHost = browser.GetHost();
            }
            if (frame.IsMain)
                Debug.Log("START: " + browser.GetMainFrame().Url);
        }

        protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {	
            if (frame.IsMain)
                Debug.Log(string.Format("END: {0}, {1}", browser.GetMainFrame().Url, httpStatusCode.ToString()));
        }
    }

    public class BaseCEFClientRenderHandler : CefRenderHandler
    {
        BaseCEFClient mClient;

        public BaseCEFClientRenderHandler(BaseCEFClient client)
        {
			mClient = client;
        }

        protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect)
        {
            return GetViewRect(browser, ref rect);
        }

        protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY)
        {
            screenX = viewX;
            screenY = viewY;
            return true;
        }

        protected override bool GetViewRect(CefBrowser browser, ref CefRectangle rect)
        {
            rect.X = 0;
            rect.Y = 0;
            rect.Width = mClient.BrowserTexture.width;
            rect.Height = mClient.BrowserTexture.height;
            return true;
        }

        protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height)
        {
            if (browser != null)
            {
                lock (mClient.sPixelLock)
                {
                    Marshal.Copy(buffer, mClient.sPixelBuffer, 0, mClient.sPixelBuffer.Length);
                }
            }
        }

        protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo)
        {
            return false;
        }

        protected override void OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo)
        {
        }

        protected override void OnPopupSize(CefBrowser browser, CefRectangle rect)
        {
        }

        protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y)
        {
        }
    }
    #endregion

}
