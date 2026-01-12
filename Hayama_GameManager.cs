using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hayama_GameManager : MonoBehaviour
{
    public static Hayama_GameManager Instance;

    CancellationToken applicationCT;

    CancellationToken endGameCT;

    CancellationTokenSource cts;

    CancellationTokenSource endLinkedCts;

    public VfxManager vfxManager;

    public Hayama_PlayerInput hayama_PlayerInput;

    public Vector3 VecZero { get; private set; }

    public Vector3 VecUp { get; private set; }

    public Vector3 VecRight { get; private set; }

    public Vector3 VecOne { get; private set; }

    public Vector3 VfxRotateZ180 { get; private set; }

    public Vector2 Vec2Zero { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else Destroy(gameObject);

        VecZero = new Vector3(0, 0, 0);
        VecUp = new Vector3(0, 1, 0);
        VecRight = new Vector3(1, 0, 0);
        VecOne = new Vector3(1, 1, 1);
        VfxRotateZ180 = new Vector3(0, 0, 180);
        Vec2Zero = new Vector3(0, 0);
        hayama_PlayerInput = new Hayama_PlayerInput();

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        applicationCT = Application.exitCancellationToken;

        cts = new CancellationTokenSource();

        endGameCT = cts.Token;

        endLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(applicationCT, endGameCT);
    }

    async void Update()
    {
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    fadeInOut.Fade(FadeType.FadeIn);
        //}

        //if (Input.GetKeyDown(KeyCode.Y))
        //{
        //    fadeInOut.Fade(FadeType.FadeOut);
        //}

        //if (fadeInOut.GetFadeFinish())
        //{
        //    Debug.Log("fadeFinish");
        //}
    }

    /// <summary>
    /// アプリケーション終了時とプレイヤー死亡時のキャンセルトークン
    /// </summary>
    /// <returns>
    /// キャンセルトークン
    /// </returns>
    public CancellationTokenSource GetLinkCts()
    {
        return endLinkedCts;
    }

    /// <summary>
    /// アプリケーション終了時のキャンセルトークン
    /// </summary>
    /// <returns>
    /// キャンセルトークン
    /// </returns>
    public CancellationToken GetApplicationCts()
    {
        return applicationCT;
    }

    /// <summary>
    /// プレイヤーが死んだときのキャンセルトークン作動関数
    /// </summary>
    public void CancellToken()
    {
        if (cts == null) return;
        cts.Cancel();
        cts.Dispose();
        cts = null;
    }
}
