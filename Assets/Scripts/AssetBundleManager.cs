using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

// Test
public class AssetBundleManager : MonoBehaviour
{
    private enum State
    {
        Ready = 0,
        Busy,
    }

    private class RequestItem
    {
        public string url = string.Empty;
        public UnityAction<string> action = null;

        public RequestItem(string url, UnityAction<string> action)
        {
            this.url = url;
            this.action = action;
        }
    }

    private Dictionary<string, string> _assetObjectNameDic = new Dictionary<string, string>();
    private Queue<RequestItem> _requestQueue = new Queue<RequestItem>();
    private State _state = State.Ready;
    private AssetDownLoader _assetDownLoader = null;
    private string _path = string.Empty;

    public bool IsBusy { get { return _state != State.Ready; } }

    private void Start()
    {
        _assetDownLoader = new AssetDownLoader();

        string applicationPath = Application.dataPath;
        DirectoryInfo directory = new DirectoryInfo(applicationPath);
        _path = "file://" + directory.Parent.ToString() + "/AssetBundles/android/";
        StartCoroutine(_assetDownLoader.DownloadAndCache(_path + "android", OnDownloadedAssetBundleManifest));
    }

    private void OnDownloadedAssetBundleManifest(string url)
    {
        AssetBundle bundle = _assetDownLoader.GetAssetBundle(url);
        var manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        string[] names = manifest.GetAllAssetBundles();

        Debug.Log(names.Length);

        foreach (string name in names)
        {
            this.DownloadAssetBundle(_path + name, OnDownloadedAssetBundle);
        }
    }

    private void OnDownloadedAssetBundle(string url)
    {
        AssetBundle bundle = _assetDownLoader.GetAssetBundle(url);
        string[] names = bundle.GetAllAssetNames();

        foreach (string name in names)
        {
            _assetObjectNameDic.Add(name, url);
        }
    }

    private void NextDownload()
    {
        if (_requestQueue.Count <= 0)
        {
            return;
        }
        if (this.IsBusy)
        {
            return;
        }
        StartCoroutine("ProcessRequest", _requestQueue.Peek());
    }
    private IEnumerator ProcessRequest(RequestItem item)
    {
        _state = State.Busy;
        yield return StartCoroutine(_assetDownLoader.DownloadAndCache(item.url, item.action));

        _requestQueue.Dequeue();
        _state = State.Ready;
        this.NextDownload();
    }

    public float DownloadProgress()
    {
        return _assetDownLoader.Progress;
    }

    public void DownloadAssetBundle(string url, UnityAction<string> action = null)
    {
        _requestQueue.Enqueue(new RequestItem(url, action));
        this.NextDownload();
    }

    public T LoadAsset<T>(string name) where T : Object
    {
        string url;

        if (_assetObjectNameDic.TryGetValue(name, out url))
        {
            AssetBundle bundle = _assetDownLoader.GetAssetBundle(url);
            return bundle.LoadAsset<T>(name);
        }
        return null;      
    }

    public void Unload(string url, bool unloadAllLoadedObjects)
    {
        _assetDownLoader.Unload(url, unloadAllLoadedObjects);
    }

    public void AllUnload(bool unloadAllLoadedObjects)
    {
        _assetDownLoader.AllUnload(unloadAllLoadedObjects);
    }

    public void ClearCache()
    {
        Caching.ClearCache();
    }
}
