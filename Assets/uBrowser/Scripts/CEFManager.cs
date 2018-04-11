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
    
    public string locale = "en-US";
    private bool windowless = true;
    public bool singleProcess = false;
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
            Locale = locale,
            MultiThreadedMessageLoop = multiThreaded,//注意：强烈建议设置成true,要不然你得在你的程序中自己处理消息循环；自己调用CefDoMessageLoopWork()
            SingleProcess = singleProcess,//注意：强烈不建议使用单进程，单进程不稳定，而且Chromium内核不支持
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

