using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Network
{
    public class AssetDownLoader
    {
        private class AssetBundleData
        {
            public string url = string.Empty;
            public AssetBundle assetBundle = null;

            public AssetBundleData(string url, AssetBundle bundle)
            {
                this.url = url;
                this.assetBundle = bundle;
            }
        }

        private Dictionary<string, AssetBundleData> _assetBundleDataDic = null;
        private float _progress = 0;

        public float Progress { get { return _progress; } }

        public AssetDownLoader()
        {
            _assetBundleDataDic = new Dictionary<string, AssetBundleData>();
        }

        public AssetBundle GetAssetBundle(string key)
        {
            AssetBundleData assetBundleData;

            if (_assetBundleDataDic.TryGetValue(key, out assetBundleData))
            {
                return assetBundleData.assetBundle;
            }
            else
            {
                return null;
            }
        }

        public IEnumerator DownloadAssetBundleManifest(string url, UnityAction<string> action)
        {
            string key = url;

            if (_assetBundleDataDic.ContainsKey(key))
            {
                if (action != null)
                {
                    action(url);
                }
                yield return null;
            }
            else 
            {
                using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url))
                {
                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError)
                    {
                        Debug.Log(www.error);
                    }
                    else
                    {
                        while (!www.isDone)
                        {
                            yield return null;
                        }

                        _assetBundleDataDic.Add(key, new AssetBundleData(url, DownloadHandlerAssetBundle.GetContent(www)));

                        if (action != null)
                        {
                            action(url);
                        }
                    }
                }
            }
        }

        public IEnumerator DownloadAndCache(string url, UnityAction<string> action)
        {
            string key = url;

            if (_assetBundleDataDic.ContainsKey(key))
            {
                if (action != null)
                {
                    action(url);
                }
                yield return null;
            }
            else
            {
                using (UnityWebRequest wwwManifest = UnityWebRequest.Get(url + ".manifest"))
                {
                    yield return wwwManifest.SendWebRequest();

                    if (wwwManifest.isNetworkError || wwwManifest.isHttpError)
                    {
                        Debug.Log(wwwManifest.error);
                    }
                    else
                    {
                        Hash128 hashString = (default(Hash128));
                        var hashRow = wwwManifest.downloadHandler.text.ToString().Split("\n".ToCharArray())[5];
                        hashString = Hash128.Parse(hashRow.Split(':')[1].Trim());

                        if (wwwManifest.downloadHandler.text.Contains("ManifestFileVersion"))
                        {
                            if (hashString.isValid)
                            {
                                if (Caching.IsVersionCached(url, hashString))
                                {
                                    Debug.Log("already cached!");
                                }
                                else
                                {
                                    Debug.Log("No cached");
                                }
                            }
                            else
                            {
                                Debug.LogError("Invalid hash:" + hashString);
                                yield break;
                            }
                        }
                        else
                        {
                            yield break;
                        }

                        while (!Caching.ready)
                        {
                            yield return null;
                        }

                        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url, hashString))
                        {
                            yield return www.SendWebRequest();

                            if (www.isNetworkError || www.isHttpError)
                            {
                                Debug.Log(www.error);
                            }
                            else
                            {
                                while (!www.isDone)
                                {
                                    _progress = www.downloadProgress;
                                    yield return null;
                                }

                                _progress = 1f;
                                _assetBundleDataDic.Add(key, new AssetBundleData(url, DownloadHandlerAssetBundle.GetContent(www)));

                                if (action != null)
                                {
                                    action(url);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Unload(string url, bool unloadLoadedObjects)
        {
            AssetBundleData assetBundleData;

            if (_assetBundleDataDic.TryGetValue(url, out assetBundleData))
            {
                assetBundleData.assetBundle.Unload(unloadLoadedObjects);
                assetBundleData.assetBundle = null;
                _assetBundleDataDic.Remove(url);
            }
        }

        public void AllUnload(bool unloadAllLoadedObjects)
        {
            foreach (KeyValuePair<string, AssetBundleData> kVP in _assetBundleDataDic)
            {
                kVP.Value.assetBundle.Unload(unloadAllLoadedObjects);
                kVP.Value.assetBundle = null;
            }

            _assetBundleDataDic = new Dictionary<string, AssetBundleData>();
        }
    }
}