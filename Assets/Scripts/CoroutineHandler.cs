using System.Collections;
using UnityEngine;

public class CoroutineHandler : UnitySingleton<CoroutineHandler>
{
    public static Coroutine StartStaticCoroutine(IEnumerator coroutine)
    {
        return Instance.StartCoroutine(coroutine);
    }

    public static Coroutine StopStaticCoroutine(IEnumerator coroutine)
    {
        return Instance.StartCoroutine(coroutine);
    }
}
