using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using Xilium.CefGlue;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class CEFManager : MonoBehaviour
{
    private static CEFManager _instance;

    public static CEFManager instance {
        get {
            if(_instance == null) {
                _instance = FindObjectOfType<CEFManager>();
                if(_instance == null) {
                    var managerObj = new GameObject();
                    _instance = managerObj.AddComponent<CEFManager>();
                }
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    private bool initialized = false;

    public bool Initialized {
        get {
            return initialized;
        }
    }

    public int BrowserPageWidth = 1280;
    public int BrowserPageHeight = 720;
    
    public string locale = "en";
    private bool windowless = true;
    public bool singleProcess = true;
    public bool multiThreaded = false;

    public bool JSRunnable = true;
    public int frameRate = 60;

    private List<BaseCEFClient> registeredClients;

    private bool mShouldQuit = false;

    void Awake()
    {
        if(_instance == null) {
            _instance = this;
        }
        else if(_instance != this) {
            //duplicated object
            Destroy(this);
            return;
        }

        registeredClients = new List<BaseCEFClient>();

        CefRuntime.Load();

        var mainArgs = new CefMainArgs(new string[] { });
        var mainApp = new OffscreenCEFApp();
        var settings = new CefSettings
        {
            //Locale = locale,
            MultiThreadedMessageLoop = multiThreaded,
            SingleProcess = singleProcess,
            WindowlessRenderingEnabled = windowless,
            NoSandbox = true,
        };

        try {
            CefRuntime.Initialize(mainArgs, settings, mainApp, IntPtr.Zero);
        }
        catch (Exception e) {
            Debug.LogError("cef initialization failed:" + e.Message);
        }
        StartCoroutine("MessagePump");

        initialized = true;
    }

    public BaseCEFClient CreateBrowser(BaseCEFClient client = null, string url = null) {
        if(!initialized)
            return null;

        if(client == null)
            client = new BaseCEFClient(BrowserPageWidth, BrowserPageHeight);
        
        var browserSettings = new CefBrowserSettings() {
            JavaScript = JSRunnable? CefState.Enabled : CefState.Disabled,
            WindowlessFrameRate = frameRate
        };

        var windowSettings = CefWindowInfo.Create();
        windowSettings.SetAsWindowless(IntPtr.Zero, false);
        
        if(url != null)
            CefBrowserHost.CreateBrowser(windowSettings, client, browserSettings, url);
        else
            CefBrowserHost.CreateBrowser(windowSettings, client, browserSettings);
        
        registeredClients.Add(client);
        return client;
    }


    void OnDisable()
    {
        mShouldQuit = true;
        foreach(BaseCEFClient client in registeredClients) {
            client.Shutdown();
        }
        registeredClients.Clear();
        CefRuntime.Shutdown();
    }

    IEnumerator MessagePump()
    {
        while (!mShouldQuit) {
            CefRuntime.DoMessageLoopWork();
            foreach(BaseCEFClient client in registeredClients) {
                client.Update();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public class OffscreenCEFApp : CefApp
    {
    }
}

