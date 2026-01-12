using UnityEngine;
using UnityEngine.InputSystem;

public class Hayama_PlayerInput
{
    Gamepad pad;

    const string xBox = "Xbox";

    const string xBox2 = "XInputControllerWindows";

    const string proController = "Pro Controller";

    string padName;

    public float MoveX { get; private set; }

    public float MoveY { get; private set; }

    public bool Jump { get; private set; }

    public bool ChangeGravity { get; private set; }

    public bool GravityReset { get; private set; }

    public bool OperateScene { get; private set; }

    public bool Select { get; private set; }

    /// <summary>
    /// PlayerInputのコンストラクタ
    /// </summary>
    public Hayama_PlayerInput()
    {
        pad = Gamepad.current;

        if(pad == null) return;

        if(pad.description.product == "") padName = Gamepad.current.name;

        else padName = pad.description.product;

        Debug.Log($"name: {pad.name}");
        Debug.Log($"product: {pad.description.product}");
        Debug.Log($"manufacturer: {pad.description.manufacturer}");
        Debug.Log($"interface: {pad.description.interfaceName}");

    }

    /// <summary>
    /// 移動と重力変化のインプット関数(Lstickと十字キー対応)
    /// </summary>
    /// <returns>
    /// 移動方向のベクトル(Vector2)
    /// </returns>
    public Vector2 MoveAndChangeWeightInput()
    {
        pad = Gamepad.current;

        if (Gamepad.current == null) pad = null;

        //ゲームパッド接続済みなら
        if (pad != null)
        {
            if(pad.leftStick != null)
            {
                MoveY = pad.leftStick.ReadValue().y;

                MoveX = pad.leftStick.ReadValue().x;
            }

            if (pad.dpad != null)
            {
                if (pad.dpad.left?.isPressed == true)
                {
                    MoveX = -1;
                }

                else if (pad.dpad.right?.isPressed == true)
                {
                    MoveX = 1;
                }

                else if (pad.dpad.up?.isPressed == true)
                {
                    MoveY = 1;
                }

                else if (pad.dpad.down?.isPressed == true)
                {
                    MoveY = -1;
                }

                else
                {
                    MoveX = 0;
                    MoveY = 0;
                }
            }
        }

        //ゲームパッド接続されてなかったら
        else
        {
            MoveY = Input.GetAxisRaw("Vertical");
            MoveX = Input.GetAxisRaw("Horizontal");
        }

        return new Vector2(MoveX, MoveY);
    }

    /// <summary>
    /// ジャンプインプット関数
    /// </summary>
    /// <returns>
    /// ジャンプフラグ
    /// </returns>
    public bool JumpInput()
    {
        pad = Gamepad.current;

        if (Gamepad.current == null) pad = null;

        //ゲームパッド接続済みなら
        if (pad != null)
        {
            //ゲームパッドがXboxコントローラーなら
            if (padName.Contains(xBox) || padName.Contains(xBox2)) Jump = pad.aButton.wasPressedThisFrame;

            //ゲームパッドがスイッチコントローラーなら
            else if (padName.Contains(proController)) Jump = pad.buttonEast.wasPressedThisFrame;
        }

        //ゲームパッド接続されてなかったら
        else
        {
            Jump = Input.GetKeyDown(KeyCode.Space);
        }

        return Jump;
    }

    /// <summary>
    /// 重力反転インプット関数
    /// </summary>
    /// <returns>
    /// 重力反転フラグ
    /// </returns>
    public bool ChangeGravityInput()
    {
        pad = Gamepad.current;

        if (Gamepad.current == null) pad = null;

        //ゲームパッド接続済みなら
        if (pad != null)
        {
            //ゲームパッドがXboxコントローラーなら
            if (padName.Contains(xBox) || padName.Contains(xBox2)) ChangeGravity = ChangeGravity = pad.xButton.wasPressedThisFrame;

            //ゲームパッドがスイッチコントローラーなら
            else if (padName.Contains(proController)) ChangeGravity = pad.buttonNorth.wasPressedThisFrame;
        }

        //ゲームパッド接続されてなかったら
        else
        {
            ChangeGravity = Input.GetKeyDown(KeyCode.LeftShift);
        }

        return ChangeGravity;
    }

    /// <summary>
    /// 重力リセットインプット関数
    /// </summary>
    /// <returns>
    /// 重力リセットフラグ
    /// </returns>
    public bool GravityResetInput()
    {
        pad = Gamepad.current;

        if (Gamepad.current == null) pad = null;

        //ゲームパッド接続済みなら
        if (pad != null)
        {
            GravityReset = pad.rightTrigger.wasPressedThisFrame;
        }

        //ゲームパッド接続されてなかったら
        else
        {
            GravityReset = Input.GetKeyDown(KeyCode.R);
        }

        return GravityReset;
    }

    /// <summary>
    /// 操作説明シーン遷移インプット関数
    /// </summary>
    /// <returns>
    /// 操作説明シーン遷移フラグ
    /// </returns>
    public bool OperateSceneInput()
    {
        pad = Gamepad.current;

        if (Gamepad.current == null) pad = null;

        //ゲームパッド接続済みなら
        if (pad != null)
        {
            OperateScene = pad.rightTrigger.wasPressedThisFrame;
        }

        //ゲームパッド接続されてなかったら
        else
        {
            OperateScene = Input.GetKeyDown(KeyCode.Z);
        }

        return OperateScene;
    }

    /// <summary>
    /// 選択インプット関数
    /// </summary>
    /// <returns>
    /// 選択フラグ
    /// </returns>
    public bool SelectInput()
    {
        pad = Gamepad.current;

        if (Gamepad.current == null) pad = null;

        //ゲームパッド接続済みなら
        if (pad != null)
        {
            Select = pad.rightTrigger.wasPressedThisFrame;
        }

        //ゲームパッド接続されてなかったら
        else
        {
            Select = Input.GetKeyDown(KeyCode.Space);
        }

        return Select;
    }
}
