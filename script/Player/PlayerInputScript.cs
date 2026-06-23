using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputScript : MonoBehaviour
{
    public static PlayerInputScript instance = null;
    public InputAction jumpInput {get; private set; }
    public InputAction shotInput {get; private set; }
    public InputAction normalAttackInput {get; private set; }
    public InputAction attackRotateAdjustInput {get; private set; }
    public InputAction heavyAttackInput {get; private set; }
    public InputAction dashInput {get; private set; }
    public InputAction SkillSlotNInput {get; private set; }
    public InputAction SkillSlotEInput {get; private set; }
    public InputAction SkillSlotSInput {get; private set; }
    public InputAction SkillSlotWInput {get; private set; }
    public InputAction SkillInput {get; private set; }
    public InputAction LockOnInput {get; private set; }
    public InputAction LockOnChangeTargetLInput {get; private set; }
    public InputAction LockOnChangeTargetRInput {get; private set; }
    public InputAction LockOnChangeTargetUpInput {get; private set; }
    public InputAction LockOnChangeTargetDownInput {get; private set; }
    public InputAction ScopeInput {get; private set; }
    public InputAction ParryInput {get; private set; }

    public InputAction UI_Submit {get; private set; }
    public InputAction UI_Cancel {get; private set; }
    public InputAction UI_Left {get; private set; }
    public InputAction UI_Right {get; private set; }
    public InputAction UI_ControlUI {get; private set; }

    public Vector2 moveInputVector  {get; private set; }
    public Vector2 cameraInputVector{get; private set; }
    public bool canInput { get;set; }
    void Awake()
     {
         if (instance != null && instance != this)
         {
             Destroy(gameObject);
         }
         else
         {
             instance = this;
             DontDestroyOnLoad(gameObject);
         }
         canInput  = true;
         jumpInput = new InputAction();
         shotInput  = new InputAction();
         normalAttackInput = new InputAction();
         attackRotateAdjustInput = new  InputAction();
         heavyAttackInput  = new InputAction();
         dashInput = new InputAction();
         SkillInput = new InputAction();
         SkillSlotNInput  = new InputAction();
         SkillSlotEInput  = new InputAction();
         SkillSlotSInput  = new InputAction();
         SkillSlotWInput  = new InputAction();
         LockOnChangeTargetLInput  = new InputAction();
         LockOnChangeTargetRInput = new InputAction();
         LockOnChangeTargetUpInput  = new InputAction();
         LockOnChangeTargetDownInput = new InputAction();
         LockOnInput  = new InputAction();
         ScopeInput = new InputAction();
         ParryInput = new InputAction();

         UI_Submit= new InputAction();
         UI_Cancel= new InputAction();
         UI_Left = new InputAction();
         UI_Right = new InputAction();
         UI_ControlUI = new InputAction();
     }

    public void Input_UI_Submit(InputAction.CallbackContext _ctx)
    {
        UI_Submit = _ctx.action;
    }

    public void Input_UI_Cancel(InputAction.CallbackContext _ctx)
    {
        UI_Cancel = _ctx.action;
    }
    public void Input_UI_Left(InputAction.CallbackContext _ctx)
    {
        UI_Left = _ctx.action;
    }
    public void Input_UI_Right(InputAction.CallbackContext _ctx)
    {
        UI_Right = _ctx.action;
    }
    public void Input_UI_ControlUI(InputAction.CallbackContext _ctx)
    {
        UI_ControlUI = _ctx.action;
    }

     public void Input_Look(InputAction.CallbackContext _ctx)
    {
        cameraInputVector =  _ctx.ReadValue<Vector2>().normalized;
    }
    public void Input_Move(InputAction.CallbackContext _ctx)
    {
        moveInputVector = _ctx.ReadValue<Vector2>().normalized;
    }
    public void Input_NormalAttack(InputAction.CallbackContext _ctx)
    {
        normalAttackInput= _ctx.action;
    }
      public void Input_AttackRotateAdjust(InputAction.CallbackContext _ctx)
        {
            attackRotateAdjustInput  = _ctx.action;
        }
    public void Input_HeavyAttack(InputAction.CallbackContext _ctx)
    {
        heavyAttackInput  = _ctx.action;
    }
    public void Input_Shot(InputAction.CallbackContext _ctx)
    {
        shotInput = _ctx.action;
    }
    public void Input_Parry(InputAction.CallbackContext _ctx)
    {
        ParryInput = _ctx.action;
    }
    public void Input_Dash(InputAction.CallbackContext _ctx)
    {
        dashInput= _ctx.action;
    }
    public void Input_Jump(InputAction.CallbackContext _ctx)
    {
        jumpInput = _ctx.action;
    }
    public void Input_Skill(InputAction.CallbackContext _ctx)
    {
        SkillInput = _ctx.action;
    }

    public void Input_SkillSlotN(InputAction.CallbackContext _ctx)
    {
        SkillSlotNInput = _ctx.action;
    }
    public void Input_SkillSlotE(InputAction.CallbackContext _ctx)
    {
        SkillSlotEInput = _ctx.action;
    }
    public void Input_SkillSlotS(InputAction.CallbackContext _ctx)
    {
        SkillSlotSInput = _ctx.action;
    }
    public void Input_SkillSlotW(InputAction.CallbackContext _ctx)
    {
        SkillSlotWInput = _ctx.action;
    }
    public void Input_LockOn(InputAction.CallbackContext _ctx)
    {
        LockOnInput = _ctx.action;
    }
    public void Input_LockOnChangeTargetL(InputAction.CallbackContext _ctx)
    {
        LockOnChangeTargetLInput = _ctx.action;
    }
    public void Input_LockOnChangeTargetR(InputAction.CallbackContext _ctx)
    {
        LockOnChangeTargetRInput = _ctx.action;
    }

    public void Input_LockOnChangeTargetUp(InputAction.CallbackContext _ctx)
    {
        LockOnChangeTargetUpInput = _ctx.action;
    }
    public void Input_LockOnChangeTargetDown(InputAction.CallbackContext _ctx)
    {
        LockOnChangeTargetDownInput = _ctx.action;
    }
    public void Input_Scope(InputAction.CallbackContext _ctx)
    {
        ScopeInput = _ctx.action;
    }
}
