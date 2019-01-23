using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class AssetBundleTest2 : MonoBehaviour
{
    public string downloadUrl = string.Empty;

    void Start()
    {
        AssetBundleManager.Instance.DownloadAllAssetBundle(DataPath, null);

        GameObject prefab = AssetBundleManager.Instance.LoadAsset<GameObject>("Assets/Bundles/Prefabs/Lobby/Capsule.prefab");
        GameObject obj = Instantiate(prefab);
        obj.transform.localPosition = Vector3.zero;

        GameObject prefab2 = AssetBundleManager.Instance.LoadAsset<GameObject>("Assets/Bundles/Prefabs/Lobby/Capsule.prefab");
        GameObject obj2 = Instantiate(prefab2);
        obj2.transform.localPosition = Vector3.zero;
    }


    void Update()
    {
    
    }

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
}
