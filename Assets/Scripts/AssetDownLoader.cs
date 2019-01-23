using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace AssetBundleSystem
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

        private Dictionary<string, AssetBundleData> _assetBundleDatas = null;
        private float _progress = 0;
        private MonoBehaviour _behaviour = null;

        public float Progress { get { return _progress; } }

        public AssetDownLoader(MonoBehaviour behaviour)
        {
            this._behaviour = behaviour;
            this._assetBundleDatas = new Dictionary<string, AssetBundleData>();
        }

        public AssetBundle GetAssetBundle(string key)
        {
            AssetBundleData assetBundleData;

            if (_assetBundleDatas.TryGetValue(key, out assetBundleData))
            {
                return assetBundleData.assetBundle;
            }
            else
            {
                return null;
            }
        }

        public IEnumerator DownloadAssetBundle(string url, UnityAction<string> action)
        {
            if (_assetBundleDatas.ContainsKey(url))
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
                    yield return _behaviour.StartCoroutine(AddAssetBundles(www, url, action));
                }
            }
        }

        public IEnumerator DownloadAndCacheAssetBundle(string url, UnityAction<string> action)
        {
            if (_assetBundleDatas.ContainsKey(url))
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

                        while (!Caching.ready)
                        {
                            yield return null;
                        }

                        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url, hashString))
                        {
                            yield return www.SendWebRequest();
                            yield return _behaviour.StartCoroutine(AddAssetBundles(www, url, action));
                        }
                    }
                }
            }
        }

        public void Unload(string url, bool unloadLoadedObjects)
        {
            AssetBundleData assetBundleData;

            if (_assetBundleDatas.TryGetValue(url, out assetBundleData))
            {
                assetBundleData.assetBundle.Unload(unloadLoadedObjects);
                assetBundleData.assetBundle = null;
                _assetBundleDatas.Remove(url);
            }
        }

        public void AllUnload(bool unloadAllLoadedObjects)
        {
            foreach (KeyValuePair<string, AssetBundleData> kVP in _assetBundleDatas)
            {
                kVP.Value.assetBundle.Unload(unloadAllLoadedObjects);
                kVP.Value.assetBundle = null;
            }

            _assetBundleDatas = new Dictionary<string, AssetBundleData>();
        }

        private IEnumerator AddAssetBundles(UnityWebRequest www, string url, UnityAction<string> action)
        {
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

                _assetBundleDatas.Add(url, new AssetBundleData(url, DownloadHandlerAssetBundle.GetContent(www)));

                if (action != null)
                {
                    action(url);
                }
            }
        }
    }
}