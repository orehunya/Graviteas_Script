using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum WeightType
{
    Light,
    Normal,
    Heavy,
}

public class Hayama_UIManager : MonoBehaviour
{
    public static Hayama_UIManager Instance;

    [SerializeField]
    [Tooltip("重さメーターの親オブジェクト")]
    GameObject weightImage;

    [SerializeField]
    [Tooltip("軽いイラストのオブジェクト")]
    GameObject lightWeightImage;

    [SerializeField]
    [Tooltip("通常イラストのオブジェクト")]
    GameObject normalWeightImage;

    [SerializeField]
    [Tooltip("重いイラストのオブジェクト")]
    GameObject heavyWeightImage;

    [SerializeField]
    [Tooltip("カーソルのTransform")]
    Transform cursorTransform;

    [SerializeField]
    [Tooltip("体重のタイプ")]
    WeightType weightType;

    [SerializeField]
    [Tooltip("カーソル移動スピード")]
    float moveSpeed;

    [Tooltip("カーソルの最初の位置")]
    Vector3 startPos;

    [Tooltip("カーソルの最後の位置")]
    Vector3 endPos;

    [Tooltip("カーソルが移動する経過時間")]
    float timer;

    [Tooltip("ゲームマネージャー")]
    Hayama_GameManager gameManager;

    [Tooltip("動かしているかフラグ")]
    bool isMove;

    [Tooltip("breakするかフラグ")]
    bool isBreak;

    [SerializeField]
    [Tooltip("フェードインアウトイメージ")]
    Image fadeImage;

    [Tooltip("フェードインアウト")]
    Hayama_FadeInOut fadeInOut;

    [SerializeField]
    [Tooltip("タイトルのボタンら")]
    GameObject titleButtons;

    [SerializeField]
    [Tooltip("動かすUIの配列")]
    Transform[] moveUIs;

    int titleButtonIndex;

    [SerializeField]
    float fadeSpeed;

    Hayama_PlayerInput hayama_PlayerInput;

    [SerializeField]
    [Tooltip("タイトルシーンName")]
    string titleSceneName;

    [SerializeField]
    [Tooltip("操作説明シーンName")]
    string operateSceneName;

    [SerializeField]
    [Tooltip("ステージセレクトシーンName")]
    string stageSelectSceneName;

    [SerializeField]
    [Tooltip("チュートリアルシーンName")]
    string tutorialSceneName;

    Vector3 startSize;

    Vector3 endSize;

    [SerializeField]
    [Tooltip("UIが動く時間")]
    float moveUITime;

    [Tooltip("UIが動く経過時間")]
    float moveUITimer;

    int agoMoveUIsIndex;

    [Tooltip("キャンセルトークン")]
    CancellationTokenSource moveUICts;

    CancellationTokenSource quitCT;

    CancellationToken applicationCT;

    bool canSelectMove;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        else Destroy(gameObject);

        isMove = false;

        isBreak = false;

        moveUICts = new CancellationTokenSource();

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        gameManager = Hayama_GameManager.Instance;
        hayama_PlayerInput = gameManager.hayama_PlayerInput;
        fadeInOut = new Hayama_FadeInOut(fadeImage, fadeSpeed);
        applicationCT = gameManager.GetApplicationCts();
        titleButtonIndex = 0;
        agoMoveUIsIndex = moveUIs.Length + 1;
        startSize = new Vector3(1, 1, 1);
        endSize = new Vector3(0.8f, 0.8f, 0.8f);
        quitCT = CancellationTokenSource.CreateLinkedTokenSource(applicationCT, moveUICts.Token);
        canSelectMove = true;
        titleButtons.SetActive(true);
        weightImage.SetActive(false);
        MoveUI(titleButtonIndex);
    }

    void Update()
    {
        if (hayama_PlayerInput.SelectInput())
        {
            if (SceneManager.GetActiveScene().buildIndex ==
                SceneManager.GetSceneByName("MainMenu").buildIndex)
            {
                switch (titleButtonIndex)
                {
                    case 0:
                        LoadSceneF(stageSelectSceneName);
                        MoveUICancel();
                        break;

                    case 1:
                        LoadSceneF(tutorialSceneName);
                        MoveUICancel();
                        break;

                    case 2:
                        MoveUICancel();
#if UNITY_EDITOR
                        UnityEditor.EditorApplication.isPlaying = false;//ゲームプレイ終了
#else
                        Application.Quit();//ゲームプレイ終了
#endif
                        break;
                }
            }
        }

        //左入力
        if (hayama_PlayerInput.MoveAndChangeWeightInput().x < 0)
        {
            if (titleButtonIndex == 0 || !canSelectMove) return;

            canSelectMove = false;
            titleButtonIndex--;
            MoveUICancel();
            MoveUI(titleButtonIndex);
        }

        //右入力
        else if (hayama_PlayerInput.MoveAndChangeWeightInput().x > 0)
        {
            if (titleButtonIndex == 2 || !canSelectMove) return;

            canSelectMove = false;
            titleButtonIndex++;
            MoveUICancel();
            MoveUI(titleButtonIndex);
        }

        if (canSelectMove) return;

        if (hayama_PlayerInput.MoveAndChangeWeightInput().x == 0) canSelectMove = true;

        if (SceneManager.GetActiveScene().buildIndex ==
            SceneManager.GetSceneByName("end").buildIndex)
        {
            if (hayama_PlayerInput.SelectInput())
            {
                LoadSceneF(titleSceneName);
            }
        }
    }

    public async void ChangeWeight(WeightType type)
    {
        if (isMove) isBreak = true;
        isMove = true;
        startPos = cursorTransform.position;
        timer = 0;

        switch (type)
        {
            case WeightType.Light:
                endPos = lightWeightImage.transform.position;
                break;

            case WeightType.Normal:
                endPos = normalWeightImage.transform.position;
                break;

            case WeightType.Heavy:
                endPos = heavyWeightImage.transform.position;
                break;
        }

        try
        {
            while (cursorTransform.position != endPos)
            {
                if (isBreak) break;
                timer += Time.deltaTime * moveSpeed;
                cursorTransform.position = Vector3.Lerp(startPos, endPos, timer);
                await UniTask.Yield(gameManager.GetApplicationCts());
            }
        }

        catch (OperationCanceledException)
        {
            Debug.Log("ChangeWeightがキャンセルされた");
        }

        if (!isBreak) isMove = false;
        else isBreak = false;
    }

    public async void LoadSceneF(string nextSceneName)
    {
        try
        {
            await fadeInOut.Fade(FadeType.FadeIn);
            SceneManager.LoadScene(nextSceneName);
            gameManager.Init();
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: gameManager.GetApplicationCts());
            SetUIs();
            await fadeInOut.Fade(FadeType.FadeOut);
        }

        catch (OperationCanceledException)
        {
            Debug.Log("LoadSceneFがキャンセルされた");
        }
    }

    void SetUIs()
    {
        if (SceneManager.GetActiveScene().buildIndex ==
                SceneManager.GetSceneByName("MainMenu").buildIndex)
        {
            titleButtons.SetActive(true);
            weightImage.SetActive(false);
        }

        else if (SceneManager.GetActiveScene().buildIndex ==
                    SceneManager.GetSceneByName("StageSelect").buildIndex)
        {
            titleButtons.SetActive(false);
            weightImage.SetActive(false);
        }

        else if (SceneManager.GetActiveScene().buildIndex ==
                    SceneManager.GetSceneByName("end").buildIndex)
        {
            titleButtons.SetActive(false);
            weightImage.SetActive(false);
        }

        else
        {
            titleButtons.SetActive(false);
            weightImage.SetActive(true);
        }
    }

    /// <summary>
    /// リザルト画面の選択ボタン画像を拡大縮小する関数
    /// </summary>
    /// <param name="index"></param>
    async void MoveUI(int index)
    {
        moveUITimer = 0;

        if (agoMoveUIsIndex == index) return;

        agoMoveUIsIndex = index;

        //指定されたインデックスのボタン画像を動かす

        try
        {
            while (true)
            {
                //最大まで大きくする
                while (moveUIs[index].localScale != endSize)
                {
                    moveUIs[index].localScale =
                        Vector3.Lerp(startSize, endSize, moveUITimer);

                    moveUITimer += Time.deltaTime;//moveSpeed制御

                    await UniTask.Yield(quitCT.Token);
                }

                //最小まで小さくする
                while (moveUIs[index].localScale != startSize)
                {
                    moveUIs[index].localScale =
                        Vector3.Lerp(startSize, endSize, moveUITimer);

                    moveUITimer -= Time.deltaTime;

                    await UniTask.Yield(quitCT.Token);//アプリケーション終了キャンセルトークン
                }

                await UniTask.Yield(quitCT.Token);
            }
        }

        catch (OperationCanceledException)
        {
            moveUIs[index].localScale = startSize;
            print("MoveUIがキャンセルされた");
        }
    }

    void MoveUICancel()
    {
        moveUICts.Cancel();
        moveUICts.Dispose();
        moveUICts = new CancellationTokenSource();
        quitCT = CancellationTokenSource.CreateLinkedTokenSource(applicationCT, moveUICts.Token);
    }
}
