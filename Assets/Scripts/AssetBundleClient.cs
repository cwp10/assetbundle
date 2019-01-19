using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Network
{
    public class AssetBundleClient
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

        private Dictionary<string, string> _assetObjectNameDic = null;
        private Queue<RequestItem> _requestQueue = null;
        private State _state = State.Ready;
        private AssetDownLoader _assetDownLoader = null;
        private string _rootPath = string.Empty;
        private int _totalDownloadCount = 0;
        private int _downloadCount = 0;

        private UnityAction<string> _downloadedCallback = null;

        public bool IsBusy { get { return _state != State.Ready; } }
        public float Progress
        {
            get
            {
                return _assetDownLoader.Progress;
            }
        }

        public int TotalDownloadCount
        {
            get
            {
                return _totalDownloadCount;
            }
        }

        public int DownloadCount
        {
            get
            {
                return _downloadCount;
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
            CoroutineHandler.StartStaticCoroutine(ProcessRequest(_requestQueue.Peek()));
        }
        private IEnumerator ProcessRequest(RequestItem item)
        {
            _state = State.Busy;
            yield return CoroutineHandler.StartStaticCoroutine(_assetDownLoader.DownloadAndCache(item.url, item.action));

            _requestQueue.Dequeue();
            _state = State.Ready;
            this.NextDownload();
        }

        private void OnDownloadedAssetBundleManifest(string url)
        {
            AssetBundle bundle = _assetDownLoader.GetAssetBundle(url);
            var manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            string[] names = manifest.GetAllAssetBundles();
            _downloadCount = names.Length;
            _totalDownloadCount = _downloadCount;
            
            foreach (string name in names)
            {
                string path = Path.Combine(_rootPath, name);
                this.DownloadAssetBundle(path, OnDownloadedAssetBundle);
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

            _downloadCount--;

            if (_downloadCount > 0) return;

            if (_downloadedCallback != null)
            {
                _downloadedCallback(url);
                _downloadedCallback = null;
            }
        }

        public AssetBundleClient()
        {
            _assetDownLoader = new AssetDownLoader();
            _assetObjectNameDic = new Dictionary<string, string>();
            _requestQueue = new Queue<RequestItem>();
        }

        public void DownloadedAssetBundleManifest(string url, UnityAction<string> action)
        {
            _rootPath = url;
            _downloadedCallback = action;

            string[] sp = url.Split('/');
            string file = sp[sp.Length - 1];
            string path = Path.Combine(_rootPath, file);

            CoroutineHandler.StartStaticCoroutine(_assetDownLoader.DownloadAssetBundleManifest(path, OnDownloadedAssetBundleManifest));
        }

        public void DownloadAssetBundle(string url, UnityAction<string> action)
        {
            _requestQueue.Enqueue(new RequestItem(url, action));
            this.NextDownload();
        }

        public T LoadAsset<T>(string name) where T : Object
        {
            string url = string.Empty;
            name = name.ToLower();

            if (_assetObjectNameDic.TryGetValue(name, out url))
            {
                AssetBundle bundle = _assetDownLoader.GetAssetBundle(url);
                return bundle.LoadAsset<T>(name);
            }
            return null;
        }

        public void Unload(string url, bool unloadLoadedObjects)
        {
            _assetDownLoader.Unload(url, unloadLoadedObjects);
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
}
