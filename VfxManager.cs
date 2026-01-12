using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct VfxObjectPools
{
    public List<GameObject> vfxPools;
}

public class VfxManager : MonoBehaviour
{
    public VfxObjectPools[] vfxObjectPools;

    public int[] vfxPoolAmount;

    public GameObject[] vfxes;

    Hayama_GameManager gameManager;

    GameObject vfx;

    GameObject getVfx;

    Transform getVfxTransform;

    int vfxCount;

    bool isPlayVfx;

    GameObject clone;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        vfxObjectPools = new VfxObjectPools[vfxPoolAmount.Length];

        for(int i = 0; i < vfxPoolAmount.Length; i++)
        {
            vfxObjectPools[i].vfxPools = new List<GameObject>();
        }

        gameManager = Hayama_GameManager.Instance;

        for(int i = 0; i < vfxPoolAmount.Length; i++)
        {
            for(int j = 0; j < vfxPoolAmount[i]; j++)
            {
                clone = Instantiate(vfxes[i]);
                vfxObjectPools[i].vfxPools.Add(clone);
                DontDestroyOnLoad(clone);
            }

            if (vfxPoolAmount[i] == 0)
            {
                vfxObjectPools[i].vfxPools.Add(null);
            }
        }
    }

    /// <summary>
    /// 一度Vfxをコピーする関数
    /// </summary>
    /// <param name="index">
    /// Vfx配列のindex
    /// </param>
    /// <param name="transform">
    /// コピーする位置
    /// </param>
    /// <param name="parent">
    /// コピーしたオブジェクトの親
    /// </param>
    public void OnShotVfxPlay(int index, Transform transform, Transform parent = null)
    {
        Instantiate(vfxes[index].gameObject, transform.position, Quaternion.identity, parent);
    }

    public void NewOnShotVfxPlay(int index, Vector3 position = default, Transform parent = null, Vector3 rotation = default, Vector3 size = default)
    {
        if (position == default) position = gameManager.VecZero;

        if (rotation == default) rotation = gameManager.VecZero;

        if (size == default) size = gameManager.VecOne;

        getVfx = NewOnShotVfxPlayGetType(index, transform, parent);

        getVfx.SetActive(true);
        getVfxTransform = getVfx.transform;

        getVfxTransform.position = position;
        getVfxTransform.eulerAngles = rotation;
        getVfxTransform.localScale = size;
        getVfxTransform.parent = parent;

        HideVfx(getVfx, 3);
    }

    async void HideVfx(GameObject vfx, float time)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(time), cancellationToken: gameManager.GetLinkCts().Token);

            vfx.SetActive(false);
        }

        catch (OperationCanceledException)
        {
            print("HideVfxがキャンセルされた");
        }
    }
    /// <summary>
    /// 一度Vfxをコピーし、それを返すゲッター関数
    /// </summary>
    /// <param name="index">
    /// Vfx配列のindex
    /// </param>
    /// <param name="transform">
    /// コピーする位置
    /// </param>
    /// <param name="parent">
    /// コピーしたオブジェクトの親
    /// </param>
    /// <returns></returns>
    public GameObject OnShotVfxPlayGetType(int index, Transform transform, Transform parent = null)
    {
        return Instantiate(vfxes[index].gameObject, transform.position, Quaternion.identity, parent);
    }

    public GameObject NewOnShotVfxPlayGetType(int index, Transform transform, Transform parent = null)
    {
        vfxCount = vfxObjectPools[index].vfxPools.Count;

        for (int i = 0; i < vfxCount; i++)
        {
            vfx = vfxObjectPools[index].vfxPools[i];

            if (vfx != null && vfx.activeSelf == false)
            {
                return vfx;
            }
        }

        vfxObjectPools[index].vfxPools.
            Add(Instantiate(vfxes[index].gameObject, transform.position, Quaternion.identity, parent));

        vfxCount = vfxObjectPools[index].vfxPools.Count;

        vfx = vfxObjectPools[index].vfxPools[vfxCount - 1];

        return vfx;
    }

    /// <summary>
    /// 親オブジェクト側でこの引数のオブジェクトの子オブジェクトとの親子関係を断ち切る関数
    /// </summary>
    /// <param name="obj">
    /// 子オブジェクト
    /// </param>
    public void ResetParent(GameObject obj)
    {
        if (obj == null) return;
        obj.transform.parent = null;
    }

    /// <summary>
    /// Vfx配列に入ってるものを指定時間表示して非表示にする関数
    /// </summary>
    /// <param name="index">
    /// Vfx配列のindex
    /// </param>
    /// <param name="useTime">
    /// 表示する時間
    /// </param>
    /// <returns></returns>
    async public UniTask SetActiveVfxPlay(int index, float useTime)
    {
        try
        {
            vfxes[index].gameObject.SetActive(true);

            await UniTask.Delay(TimeSpan.FromSeconds(useTime), cancellationToken: gameManager.GetLinkCts().Token);

            vfxes[index].gameObject.SetActive(false);
        }

        catch (OperationCanceledException)
        {
            print("SetActiveVfxPlayがキャンセルされた");
        }
    }

    /// <summary>
    /// ループ関数で使って出すスパンを指定でき、連続的にVfxを出す関数
    /// </summary>
    /// <param name="index">
    /// Vfx配列のindex
    /// </param>
    /// <param name="span">
    /// Vfxをコピーするスパン
    /// </param>
    /// <param name="transform">
    /// コピーする位置
    /// </param>
    /// <param name="parent">
    /// コピーしたオブジェクトの親
    /// </param>
    async public UniTask LoopVfxPlay(int index, float span, Transform transform, Transform parent = null)
    {
        try
        {
            if (!isPlayVfx)
            {
                isPlayVfx = true;

                Instantiate(vfxes[index].gameObject, transform.position, Quaternion.identity);

                if (span == 0) await UniTask.Yield(gameManager.GetLinkCts().Token);

                else await UniTask.Delay(TimeSpan.FromSeconds(span), cancellationToken: gameManager.GetLinkCts().Token);

                isPlayVfx = false;
            }
        }

        catch (OperationCanceledException)
        {
            print("LoopVfxPLayがキャンセルされた");
        }
    }

    async public UniTask NewLoopVfxPlay(int index, float span, Transform transform, Transform parent = null, Vector3 rotation = default)
    {
        try
        {
            if (!isPlayVfx)
            {
                isPlayVfx = true;

                if (rotation == default) rotation = gameManager.VecZero;

                getVfx = NewOnShotVfxPlayGetType(index, transform, parent);

                getVfx.SetActive(true);
                getVfx.transform.position = transform.position;
                getVfx.transform.eulerAngles = rotation;
                getVfx.transform.parent = parent;

                HideVfx(getVfx, 3);

                if (span == 0) await UniTask.Yield(gameManager.GetLinkCts().Token);

                else await UniTask.Delay(TimeSpan.FromSeconds(span), cancellationToken: gameManager.GetLinkCts().Token);

                isPlayVfx = false;
            }
        }

        catch (OperationCanceledException)
        {
            print("LoopVfxPLayがキャンセルされた");
        }
    }
}
