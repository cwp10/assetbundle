using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Network
{
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
        private string DataPath
        {
            get
            {
                DirectoryInfo directory = new DirectoryInfo(Application.dataPath);

#if UNITY_EDITOR && UNITY_ANDROID
                return "file://" + directory.Parent.ToString() + "/AssetBundles/android";
#elif UNITY_EDITOR && UNITY_IOS
                return "file://" + directory.Parent.ToString() + "/AssetBundles/ios";
#elif UNITY_EDITOR && UNITY_STANDALONE
                return "file://" + directory.Parent.ToString() + "/AssetBundles/pc";
#elif UNITY_ANDROID
                return Path.Combine(downloadUrl, "android");
#elif UNITY_IOS
                return Path.Combine(downloadUrl, "ios");
#elif UNITY_STANDALONE
                return Path.Combine(downloadUrl, "pc");
#endif
            }
        }

        public bool IsBusy { get { return _state != State.Ready; } }
        public string downloadUrl = string.Empty;

        private void Start()
        {
            _assetDownLoader = new AssetDownLoader();
            this.DownloadedAssetBundleManifest();
        }

        private void DownloadedAssetBundleManifest()
        {
            string[] folders = DataPath.Split('/');
            string manifestName = folders[folders.Length - 1];
            string path = Path.Combine(DataPath, manifestName);
            StartCoroutine(_assetDownLoader.DownloadAndCache(path, OnDownloadedAssetBundleManifest));
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

        private void OnDownloadedAssetBundleManifest(string url)
        {
            AssetBundle bundle = _assetDownLoader.GetAssetBundle(url);
            var manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            string[] names = manifest.GetAllAssetBundles();

            foreach (string name in names)
            {
                string path = Path.Combine(DataPath, name);
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
}
