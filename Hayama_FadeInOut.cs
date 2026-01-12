using Cysharp.Threading.Tasks;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// フェードタイプ
/// </summary>
public enum FadeType
{
    [Tooltip("フェードイン")]
    FadeIn,

    [Tooltip("フェードアウト")]
    FadeOut,
}

public class Hayama_FadeInOut
{
    [Tooltip("フェード操作するオブジェクトのSpriteRenderer")]
    Image image;

    [Tooltip("アルファ値")]
    float alpha;

    [Tooltip("フェード経過時間")]
    float timer;

    [Tooltip("フェードスピード")]
    float fadeSpeed;

    [Tooltip("フェード始めのアルファ値")]
    float startAlpha;

    [Tooltip("フェード終わりのアルファ値")]
    float endAlpha;

    [Tooltip("キャンセルトークンをもらう")]
    Hayama_GameManager gameManager;

    [Tooltip("カラー")]
    Color color;

    [Tooltip("フェードできるかフラグ")]
    bool canFade;

    [Tooltip("フェードが終わったかフラグ")]
    bool isFinishFade;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="renderer">
    /// フェード操作するオブジェクトのSpriteRenderer
    /// </param>
    /// <param name="fadeSpeed">
    /// フェードスピード
    /// </param>
    public Hayama_FadeInOut(Image image, float fadeSpeed)
    {
        this.image = image;
        this.fadeSpeed = fadeSpeed;
        gameManager = Hayama_GameManager.Instance;
        canFade = true;
        color = this.image.color;
    }

    /// <summary>
    /// フェード関数
    /// </summary>
    /// <param name="type">
    /// フェードのタイプ
    /// </param>
    public async UniTask Fade(FadeType type)
    {
        if (!canFade) return;

        isFinishFade = false;
        canFade = false;
        timer = 0;

        if (type == FadeType.FadeIn)
        {
            startAlpha = 0;
            endAlpha = 1;
        }

        else if (type == FadeType.FadeOut)
        {
            startAlpha = 1;
            endAlpha = 0;
        }

        try
        {
            while (alpha != endAlpha)
            {
                timer += Time.deltaTime * fadeSpeed;

                alpha = Mathf.Lerp(startAlpha, endAlpha, timer);

                image.color = new Color(color.r, color.g, color.b, alpha);

                await UniTask.Yield(gameManager.GetApplicationCts());
            }

            isFinishFade = true;
            ResetFadeFinish();
            canFade = true;
        }

        catch (OperationCanceledException)
        {
            Debug.Log("FadeInOutがキャンセルされた");
        }
    }

    /// <summary>
    /// フェード処理が終わったことを知らせるゲッター
    /// </summary>
    /// <returns></returns>
    public bool GetFadeFinish()
    {
        return isFinishFade;
    }

    async void ResetFadeFinish()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: gameManager.GetApplicationCts());

        isFinishFade = false;
    }
}
