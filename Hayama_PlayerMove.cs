using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class Hayama_PlayerMove : MonoBehaviour
{
    #region SerializeField
    [SerializeField]
    [Header("PlayerのRigidbody")]
    Rigidbody2D rb2;

    [SerializeField]
    [Header("PlayerのSpriteRenderer")]
    SpriteRenderer spriteRenderer;

    [SerializeField]
    [Header("PlayerのAnimator")]
    Animator animator;

    [SerializeField]
    [Header("PlayerのCollider")]
    CapsuleCollider2D playerCollider;

    [SerializeField]
    [Header("Playerの移動の速さ")]
    float moveSpeed;

    [SerializeField]
    [Header("Playerの軽いときのジャンプ力")]
    float lightJumpPower;

    [SerializeField]
    [Header("Playerの重いときのジャンプ力")]
    float heavyJumpPower;

    [SerializeField]
    [Header("Playerのジャンプ力")]
    float normalJumpPower;

    [SerializeField]
    [Header("軽いときのジャンプする時間")]
    float lightJumpTime;

    [SerializeField]
    [Header("通常の重さのときのジャンプする時間")]
    float normalJumpTime;

    [SerializeField]
    [Header("重いときのジャンプする時間")]
    float heavyJumpTime;

    [SerializeField]
    [Header("滞空する時間")]
    float stopGravityTime;

    [SerializeField]
    [Header("地面を検知するrayのtransform")]
    Transform bottomRayTransform;

    [SerializeField]
    [Header("天井を検知するrayのtransform")]
    Transform upperRayTransform;

    [SerializeField]
    [Header("左の壁を検知するrayのtransform")]
    Transform leftRayTransform;

    [SerializeField]
    [Header("右の壁を検知するrayのtransform")]
    Transform rightRayTransform;

    [SerializeField]
    [Header("壁を検知するrayのdistance")]
    float rayDistance = 0f;

    [SerializeField]
    [Header("Rayが無視するLayer")]
    LayerMask ignoreLayers;

    [SerializeField]
    [Header("軽くなった重力量")]
    float lightGravityPower;

    [SerializeField]
    [Header("重くなった重力量")]
    float heavyGravityPower;

    [SerializeField]
    [Header("通常重力量")]
    float normalGravityPower;

    [SerializeField]
    [Header("重いときの着地のディレイ")]
    float heavyStayTime;

    [SerializeField]
    [Header("CameraShake")]
    CameraShake cameraShake;

    [SerializeField]
    [Header("死亡時跳ね上がる速さ")]
    float deathSpeed;

    [SerializeField]
    [Header("死亡時跳ね上がる時間")]
    float deathTime;

    [SerializeField]
    Transform vfxTransform;

    [SerializeField]
    float takeOnDelayTime;

    [SerializeField]
    Goal goal;

    [SerializeField]
    [Header("軽いときの踏ん張り時間")]
    float lightJumpDelayTime;

    [SerializeField]
    [Header("通常の重さのときの踏ん張り時間")]
    float normalJumpDelayTime;

    [SerializeField]
    [Header("重いときの踏ん張り時間")]
    float heavyJumpDelayTime;
    #endregion

    #region 変数
    Transform myTransform;

    bool isGroundTouch;

    public bool isDownGravity;
    //パブリック

    Vector3 moveVec;

    Vector3 stopVec;

    float jumpingTimer;

    float agoGravityPower;

    public bool canControll;

    RaycastHit2D[] uppers;

    RaycastHit2D[] bottoms;

    RaycastHit2D[] lefts;

    RaycastHit2D[] rights;

    Vector2 xRaySize;

    Vector2 yRaySize;

    bool isFindBottom;

    bool isFindUpper;

    public bool isHeavy; //重いとき

    public bool isLight; //軽いとき

    bool canTakeOn;

    bool canHeavyDelay;

    [SerializeField]
    bool isDeath;

    BoxCollider2D lightBlockCollider;

    Hayama_PlayerInput hayama_PlayerInput;

    float deathTimer;

    Hayama_GameManager gameManager;

    VfxManager vfxManager;

    float nowJumpPower;

    bool canGravityChange;

    bool canLightGimmick;

    bool jumpUping;

    float nowJumpDelayTime;

    float nowJumpTime;

    public bool Unko = true;

    Hayama_UIManager uIManager;
    #endregion

    
    void Start()
    {
        gameManager = Hayama_GameManager.Instance;
        uIManager = Hayama_UIManager.Instance;
        hayama_PlayerInput = gameManager.hayama_PlayerInput;
        myTransform = transform;
        isGroundTouch = true;
        isDownGravity = true;
        canControll = true;
        isLight = false;
        isHeavy = false;
        canTakeOn = true;
        canHeavyDelay = true;
        isDeath = false;
        canGravityChange = true;
        canLightGimmick = true;
        jumpUping = false;
        nowJumpPower = normalJumpPower;
        rb2.gravityScale = normalGravityPower;
        nowJumpDelayTime = normalJumpDelayTime;
        nowJumpTime = normalJumpTime;
        xRaySize = new Vector2(0.1f, myTransform.localScale.y);
        yRaySize = new Vector2(myTransform.localScale.x + 0.2f, 0.1f);

        bottoms = new RaycastHit2D[2];

        vfxManager = gameManager.vfxManager;

        Unko = true;
    }

    void Update()
    {
        if(!Unko) return;


        if (canControll)
        {
            Move();

            Jump();

            ChangeGravity();

            ChangeWeight();

            WallSearchCollider();
        }

        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    void WallSearchCollider()
    {
        lefts = Physics2D.BoxCastAll(leftRayTransform.position, xRaySize, 0, gameManager.Vec2Zero, rayDistance,~ignoreLayers);

        rights = Physics2D.BoxCastAll(rightRayTransform.position, xRaySize, 0, gameManager.Vec2Zero, rayDistance, ~ignoreLayers);

        if (lefts.Length != 0)
        {
            foreach (RaycastHit2D hit in lefts)
            {
                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Goal"))
                    {
                        continue;
                    }

                    if (!hit.collider.CompareTag("lightBlock"))
                    {
                        stopVec = gameManager.VecRight;
                    }
                }
            }
        }

        else if (rights.Length != 0)
        {
            foreach (RaycastHit2D hit in rights)
            {
                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Goal"))
                    {
                        continue;
                    }

                    if (!hit.collider.CompareTag("lightBlock"))
                    {
                        stopVec = gameManager.VecRight * -1;
                    }
                }
            }
        }

        else
        {
            stopVec = gameManager.VecZero;
        }
    }

    void ChangeWeight()
    {
        if (canGravityChange)
        {
            //上入力されたら
            if (0.3f <= hayama_PlayerInput.MoveAndChangeWeightInput().y &&
               hayama_PlayerInput.MoveAndChangeWeightInput().y <= 1)
            {
                canGravityChange = false;

                //重量が下向きなら
                if (isDownGravity)
                {
                    rb2.gravityScale = lightGravityPower;
                    nowJumpPower = lightJumpPower;
                    nowJumpDelayTime = lightJumpDelayTime;
                    nowJumpTime = lightJumpTime;
                    isHeavy = false;
                    isLight = true;
                    uIManager.ChangeWeight(WeightType.Light);
                    SearchBottomCollider();

                    //軽量化
                    vfxManager.NewOnShotVfxPlay(3, vfxTransform.position, vfxTransform, vfxTransform.eulerAngles);
                }

                //重量が上向き
                else
                {
                    //重量の向き反転するから-1かける
                    rb2.gravityScale = heavyGravityPower * -1;
                    nowJumpPower = heavyJumpPower * -1;
                    nowJumpDelayTime = heavyJumpDelayTime;
                    nowJumpTime = heavyJumpTime;
                    isHeavy = true;
                    isLight = false;
                    uIManager.ChangeWeight(WeightType.Heavy);
                    SearchBottomCollider();

                    //重量化
                    vfxManager.NewOnShotVfxPlay(5, vfxTransform.position, vfxTransform, vfxTransform.eulerAngles);
                }
            }

            //下入力されたら
            else if (-1 <= hayama_PlayerInput.MoveAndChangeWeightInput().y &&
                     hayama_PlayerInput.MoveAndChangeWeightInput().y <= -0.3f)
            {
                canGravityChange = false;

                //重量が下向きなら
                if (isDownGravity)
                {
                    rb2.gravityScale = heavyGravityPower;
                    nowJumpPower = heavyJumpPower;
                    nowJumpDelayTime = heavyJumpDelayTime;
                    nowJumpTime = heavyJumpTime;
                    isHeavy = true;
                    isLight = false;
                    uIManager.ChangeWeight(WeightType.Heavy);
                    SearchBottomCollider();

                    //重量化
                    vfxManager.NewOnShotVfxPlay(5, vfxTransform.position, vfxTransform, vfxTransform.eulerAngles);
                }

                //重量が上向きなら
                else
                {
                    //重量の向き反転するから-1かける
                    rb2.gravityScale = lightGravityPower * -1;
                    nowJumpPower = lightJumpPower * -1;
                    nowJumpDelayTime = lightJumpDelayTime;
                    nowJumpTime = lightJumpTime;
                    isHeavy = false;
                    isLight = true;
                    uIManager.ChangeWeight(WeightType.Light);
                    SearchBottomCollider();

                    //軽量化
                    vfxManager.NewOnShotVfxPlay(3, vfxTransform.position, vfxTransform, vfxTransform.eulerAngles);
                }
            }
        }

        else
        {
            if (hayama_PlayerInput.MoveAndChangeWeightInput().y == 0) canGravityChange = true;
        }

        //重量通常化入力されたら
        if (hayama_PlayerInput.GravityResetInput())
        {
            //重量が下向きなら
            if (isDownGravity)
            {
                rb2.gravityScale = normalGravityPower;
                nowJumpPower = normalJumpPower;

                //重力リセット
                vfxManager.NewOnShotVfxPlay(4, vfxTransform.position, vfxTransform, vfxTransform.eulerAngles);
            }

            //重量が上向きなら
            //重量の向き反転するから-1かける
            else
            {
                rb2.gravityScale = normalGravityPower * -1;
                nowJumpPower = normalJumpPower * -1;

                //重力リセット
                vfxManager.NewOnShotVfxPlay(4, vfxTransform.position, vfxTransform, vfxTransform.eulerAngles);
            }

            nowJumpDelayTime = normalJumpDelayTime;
            nowJumpTime = normalJumpTime;
            isHeavy = false;
            isLight = false;
            uIManager.ChangeWeight(WeightType.Normal);
            SearchBottomCollider();
        }
    }

    void FixedUpdate()
    {
        SearchBottomCollider();
    }

    /// <summary>
    /// 死んだ後の関数
    /// </summary>
    async void Death()
    {
        if (!isDeath) return;

        canControll = false;

        //isDeath = false;

        gameManager.CancellToken();

        playerCollider.isTrigger = true;
        deathTimer = 0;

        if(!jumpUping) agoGravityPower = rb2.gravityScale;

        rb2.gravityScale = 0;

        animator.SetTrigger("Death");

        spriteRenderer.flipY = !spriteRenderer.flipY;

        //血しぶき
        vfxManager.NewOnShotVfxPlay(7, vfxTransform.position, rotation: vfxTransform.eulerAngles, size: gameManager.VecOne * 3);

        try
        {
            while (deathTimer <= deathTime)
            {
                if (isDownGravity)
                {
                    myTransform.position += gameManager.VecUp * deathSpeed * Time.deltaTime;
                }

                else
                {
                    myTransform.position -= gameManager.VecUp * deathSpeed * Time.deltaTime;
                }

                deathTimer += Time.deltaTime;

                await UniTask.Yield(gameManager.GetApplicationCts());
            }

            rb2.gravityScale = agoGravityPower;
            await UniTask.Delay(TimeSpan.FromSeconds(3.0f), cancellationToken: gameManager.GetApplicationCts());

            if(goal == null)
            {
                Debug.LogWarning("ゴールが存在または、インスペクターで設定されてへんで");
                return;
            }

            goal.LoadSceneF();
        }

        catch (OperationCanceledException)
        {
            print("Deathがキャンセルされた");
        }
    }

    /// <summary>
    /// 重力反転関数
    /// </summary>
    void ChangeGravity()
    {
        //重力反転入力されたら
        if (hayama_PlayerInput.ChangeGravityInput())
        {
            spriteRenderer.flipY = !spriteRenderer.flipY;
            isDownGravity = !isDownGravity;
            nowJumpPower *= -1;
            rb2.gravityScale *= -1;
            agoGravityPower *= -1;
            canControll = false;
            isGroundTouch = false;
            rb2.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

            if (vfxTransform.eulerAngles.z == 180 || vfxTransform.eulerAngles.z == -180) 
                vfxTransform.eulerAngles = gameManager.VecZero;

            else vfxTransform.eulerAngles = gameManager.VfxRotateZ180;

            //重力反転
            vfxManager.NewOnShotVfxPlay(6, vfxTransform.position, vfxTransform, vfxTransform.eulerAngles);

            animator.SetBool("isJumping", true);

            TakeOnDelay();
        }
    }

    /// <summary>
    /// ジャンプ関数
    /// </summary>
    async void Jump()
    {
        try
        {
            //ジャンプ入力されて、かつ、地面についていたら
            if (hayama_PlayerInput.JumpInput() && isGroundTouch)
            {
                jumpUping = true;

                await UniTask.Delay(TimeSpan.FromSeconds(nowJumpDelayTime));

                isGroundTouch = false;
                agoGravityPower = rb2.gravityScale;
                rb2.gravityScale = 0;
                animator.SetBool("isJumping", true);
                TakeOnDelay();

                //足元煙
                vfxManager.NewOnShotVfxPlay(0, vfxTransform.position, rotation: vfxTransform.eulerAngles);

                //ジャンプで勢いでてるVfx
                //vfxManager.OnShotVfxPlay();

                jumpingTimer = 0;

                if (!canLightGimmick) canLightGimmick = true;

                while (jumpingTimer <= nowJumpTime)
                {
                    if (SearchUpperCollider() || !canGravityChange) break;

                    if(!Unko)
                    {
                        await UniTask.Yield(gameManager.GetLinkCts().Token);
                        continue;
                    }

                    myTransform.position += gameManager.VecUp * nowJumpPower * Time.deltaTime;
                    if (Unko) jumpingTimer += Time.deltaTime;
                    await UniTask.Yield(cancellationToken: gameManager.GetLinkCts().Token);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(stopGravityTime), cancellationToken: gameManager.GetLinkCts().Token);

                jumpUping = false;
                //isStopGravity = false;
                rb2.gravityScale = agoGravityPower;
            }
        }

        catch (OperationCanceledException)
        {
            print("Jumpがキャンセルされた");
        }
    }

    /// <summary>
    /// 移動関数　キャラの向きも変わるよ
    /// </summary>
    void Move()
    {
        //移動入力がなかったら
        if (hayama_PlayerInput.MoveAndChangeWeightInput().x == 0)
        {
            animator.SetBool("isRunning", false);
            moveVec = gameManager.VecZero;
            return;
        }

        //移動入力で左入力だったら
        if (-1 <= hayama_PlayerInput.MoveAndChangeWeightInput().x &&
            hayama_PlayerInput.MoveAndChangeWeightInput().x <= -0.3f)
        {
            moveVec = gameManager.VecRight * -1;
            spriteRenderer.flipX = true;
            animator.SetBool("isRunning", true);

            if (isGroundTouch)
            {
                //足元煙
                vfxManager.NewLoopVfxPlay(0, 0.2f, vfxTransform, rotation: vfxTransform.eulerAngles);
            }

            SearchBottomCollider();
        }

        //移動入力で右入力だったら
        if (0.3f <= hayama_PlayerInput.MoveAndChangeWeightInput().x &&
            hayama_PlayerInput.MoveAndChangeWeightInput().x <= 1)
        {
            moveVec = gameManager.VecRight;
            spriteRenderer.flipX = false;
            animator.SetBool("isRunning", true);

            if (isGroundTouch)
            {
                //足元煙
                vfxManager.NewLoopVfxPlay(0, 0.2f, vfxTransform, rotation: vfxTransform.eulerAngles);
            }

            SearchBottomCollider();
        }

        myTransform.position += (moveVec + stopVec) * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// 地面を検知する関数
    /// </summary>
    /// <returns></returns>
    void SearchBottomCollider()
    {
        //重力が下のとき
        if (isDownGravity) bottoms = Physics2D.BoxCastAll(bottomRayTransform.position, yRaySize, 0, gameManager.Vec2Zero, rayDistance, ~ignoreLayers);

        //重力が上のとき
        else bottoms = Physics2D.BoxCastAll(upperRayTransform.position, yRaySize, 0, gameManager.Vec2Zero, rayDistance, ~ignoreLayers);

        //地面検知で見つけたColliderを全て調べる
        foreach (RaycastHit2D raycastHit2Ds in bottoms)
        {
            if (raycastHit2Ds.collider != null)
            {
                rb2.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;

                foreach (RaycastHit2D raycastHit2Ds2 in bottoms)
                {
                    //見つけたColliderがギミック、プレイヤーじゃなかったら
                    if (!raycastHit2Ds2.collider.CompareTag("WeakBlock") &&
                       !raycastHit2Ds2.collider.CompareTag("AFewWeakBlock") &&
                       !raycastHit2Ds2.collider.CompareTag("lightBlock") &&
                       !raycastHit2Ds2.collider.CompareTag("Player") &&
                       !raycastHit2Ds2.collider.CompareTag("DamageBlock") && !isDeath)
                    {
                        //重い状態で空中にいたらかつ重いときのディレイが重複してなかったら
                        if (isHeavy && !isGroundTouch && canHeavyDelay)
                        {
                            canHeavyDelay = false;
                            HeavyDelay();
                        }

                        //重いときのディレイが重複してなかったら
                        else if (canHeavyDelay)
                        {
                            canControll = true;
                        }

                        if (!isGroundTouch && !isHeavy) ImpactVfxPlay();

                        //重くなかったら
                        if (!isHeavy) isGroundTouch = true;

                        animator.SetBool("isJumping", false);
                        return;
                    }
                }

                //脆いブロックだったら
                if (raycastHit2Ds.collider.CompareTag("WeakBlock") && !isDeath)
                {
                    //重いとき
                    if (isHeavy)
                    {
                        //瓦礫
                        vfxManager.NewOnShotVfxPlay(8, raycastHit2Ds.collider.gameObject.transform.position, size: gameManager.VecOne * 0.2f);

                        Destroy(raycastHit2Ds.collider.gameObject);

                        isGroundTouch = false;
                    }

                    //それ以外
                    else
                    {
                        if (!isGroundTouch) ImpactVfxPlay();
                        animator.SetBool("isJumping", false);
                        isGroundTouch = true;
                        canControll = true;
                    }
                }

                //少し脆いブロックだったら
                else if (raycastHit2Ds.collider.CompareTag("AFewWeakBlock") && !isDeath)
                {
                    //重くて空中からの着地だったら
                    if (isHeavy && !isGroundTouch)
                    {
                        //瓦礫
                        vfxManager.NewOnShotVfxPlay(8, raycastHit2Ds.collider.gameObject.transform.position, size: gameManager.VecOne * 0.2f);

                        Destroy(raycastHit2Ds.collider.gameObject);

                        isGroundTouch = false;
                    }

                    //それ以外
                    else
                    {
                        if (!isGroundTouch) ImpactVfxPlay();
                        animator.SetBool("isJumping", false);
                        isGroundTouch = true;
                        canControll = true;
                    }
                }

                //軽くないと乗れないブロックだったら
                else if (raycastHit2Ds.collider.CompareTag("lightBlock") && !isDeath)
                {
                    //軽かったら
                    if (isLight)
                    {
                        if (!isGroundTouch) ImpactVfxPlay();

                        animator.SetBool("isJumping", false);
                        canControll = true;

                        //軽いブロックのColliderを取って、当たり判定を消す
                        lightBlockCollider = raycastHit2Ds.collider.gameObject.GetComponent<BoxCollider2D>();
                        lightBlockCollider.isTrigger = false;
                        isGroundTouch = true;
                        LightBlockDelay();

                        if (canLightGimmick)
                        {
                            canLightGimmick = false;

                            //葉が散る
                            vfxManager.NewOnShotVfxPlay(9, raycastHit2Ds.collider.gameObject.transform.position);
                        }
                    }

                    //それ以外
                    else
                    {
                        ////軽いブロックのSpriteRendererを取って、Spriteを変える
                        //lightBlockRenderer = raycastHit2Ds.collider.gameObject.GetComponent<SpriteRenderer>();
                        //lightBlockRenderer.sprite = changeLightBlock;

                        if (canLightGimmick)
                        {
                            canLightGimmick = false;

                            //葉が散る
                            vfxManager.NewOnShotVfxPlay(9, raycastHit2Ds.collider.gameObject.transform.position);
                        }
                    }
                }

                else if (raycastHit2Ds.collider.CompareTag("DamageBlock") && !isDeath)
                {
                    canControll = false;
                    playerCollider.isTrigger = true;
                    isDeath = true;
                    Death();
                }
            }

            else isGroundTouch = false;
        }
    }

    Vector3 GetTakeOnVfxSize()
    {
        if (isLight) return gameManager.VecOne * 0.75f;

        else if (isHeavy) return gameManager.VecOne * 1.5f;

        else return gameManager.VecOne;
    }

    /// <summary>
    /// 軽かったら乗れるブロックの変化関数
    /// </summary>
    async void LightBlockDelay()
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: gameManager.GetLinkCts().Token);

            //変えた項目を元に戻す
            lightBlockCollider.isTrigger = true;
        }

        catch (OperationCanceledException)
        {
            print("LightBlockDelayがキャンセルされた");
        }
    }

    /// <summary>
    /// 天井を検知する関数
    /// </summary>
    /// <returns></returns>
    bool SearchUpperCollider()
    {
        isFindUpper = false;

        //重力が下のとき
        if (isDownGravity) uppers = Physics2D.BoxCastAll(upperRayTransform.position, yRaySize, 0, gameManager.Vec2Zero, rayDistance, ~ignoreLayers);

        //重力が上のとき
        else uppers = Physics2D.BoxCastAll(bottomRayTransform.position, yRaySize, 0, gameManager.Vec2Zero, rayDistance, ~ignoreLayers);

        foreach(RaycastHit2D raycastHit2D in uppers)
        {
            if (raycastHit2D.collider != null)
            {
                //プレイヤーじゃなかったら
                if (!raycastHit2D.collider.CompareTag("Player"))
                {
                    if (raycastHit2D.collider.CompareTag("DamageBlock") && !isDeath)
                    {
                        isDeath = true;
                        Death();
                    }

                    else if (raycastHit2D.collider.CompareTag("lightBlock") && !isDeath)
                    {
                        if (!isLight) isFindUpper = false;
                    }

                    else isFindUpper = true;
                }
            }
        }

        return isFindUpper;
    }

    /// <summary>
    /// 重いときの着地ディレイ関数
    /// </summary>
    async void HeavyDelay()
    {
        try
        {
            canControll = false;

            ImpactVfxPlay();

            //カメラシェイク
            cameraShake.Shake(0.2f, 0.25f);

            await UniTask.Delay(TimeSpan.FromSeconds(heavyStayTime), cancellationToken: gameManager.GetLinkCts().Token);

            isGroundTouch = true;
            canControll = true;
            canHeavyDelay = true;
        }

        catch (OperationCanceledException)
        {
            print("HeavyDelayがキャンセルされた");
        }
    }

    void ImpactVfxPlay()
    {
        if (isDownGravity) vfxManager.NewOnShotVfxPlay(1, bottomRayTransform.position + (gameManager.VecUp * 0.1f),
                                                       size: GetTakeOnVfxSize());

        else vfxManager.NewOnShotVfxPlay(2, upperRayTransform.position + (gameManager.VecUp * -0.1f),
                                         size: GetTakeOnVfxSize());
    }

    /// <summary>
    /// 地面を検知できるようになるディレイ関数
    /// </summary>
    async void TakeOnDelay()
    {
        try
        {
            if(canTakeOn)
            {
                canTakeOn = false;

                await UniTask.Delay(TimeSpan.FromSeconds(takeOnDelayTime), cancellationToken: gameManager.GetLinkCts().Token);

                canTakeOn = true;
            }
        }

        catch (OperationCanceledException)
        {
            print("TakeOnDelayがキャンセルされた");
        }
    } 

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bottomRayTransform.position, yRaySize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(upperRayTransform.position, yRaySize);
    }
}
