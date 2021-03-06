﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AssetBundleSystem;

public class AssetBundleManager : UnitySingleton<AssetBundleManager>
{
    private AssetBundleClient _assetBundleClient = null;

    public float Progress
    {
        get
        {
            return _assetBundleClient.Progress;
        }
    }

    public int TotalDownloadCount
    {
        get
        {
            return _assetBundleClient.TotalDownloadCount;
        }
    }

    public int DownloadCount
    {
        get
        {
            return _assetBundleClient.DownloadCount;
        }
    }

    public void Init()
    {
        _assetBundleClient = new AssetBundleClient(this);
    }

    public void DownloadAllAssetBundle(string url, UnityAction<string> action)
    {
        _assetBundleClient.DownloadedAllAssetBundle(url, action);
    }

    public void DownloadAssetBundle(string url, UnityAction<string> action)
    {
        _assetBundleClient.DownloadAssetBundle(url, action);
    }

    public T LoadAsset<T>(string name) where T : Object
    {
        return _assetBundleClient.LoadAsset<T>(name);
    }

    public T[] LoadAllAssets<T>(string url) where T : Object
    {
        return _assetBundleClient.LoadAllAssets<T>(url);
    }

    public void Unload(string url, bool unloadLoadedObjects)
    {
        _assetBundleClient.Unload(url, unloadLoadedObjects);
    }

    public void AllUnload(bool unloadAllLoadedObjects)
    {
        _assetBundleClient.AllUnload(unloadAllLoadedObjects);
    }

    public void ClearCache()
    {
        _assetBundleClient.ClearCache();
    }
}
