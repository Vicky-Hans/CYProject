using System;
using DH.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;

namespace DH.Game
{
    public class MoveComponent : MonoBehaviour
    {
        [Header("关闭模式下只存在左右")] public bool fullRotate;
        public Transform rotateRoot;

        internal Action<bool> OnStateChanged;
        /// <summary>
        /// 禁止模型翻转，用于部分动作翻转后表现不一致的情况
        /// </summary>
        internal bool LockFlip;
        internal Vector3 RightDirection;
        internal CharacterController2D CharacterController;
        internal CharacterController playerCtrl;
        internal Rigidbody2D rigidbody2D;
        internal Func<float> GetSpeed;
        internal event Action<CharacterController2D.CollideResult,Vector2> CollideEvent;

        private PlayerMove inputComponent;
        private Vector2 velocity;
        private bool isMoving;
        private CameraController cameraController;
        private Vector2 mapSize;

        public bool IsMoving
        {
            set
            {
                if (isMoving == value)
                {
                    return;
                }
                isMoving = value;
                OnStateChanged?.Invoke(isMoving);
                if (!isMoving)
                {
                    CollideEvent?.Invoke(CharacterController2D.CollideResult.None,Vector2.zero);
                }
            }
            get => isMoving;
        }

        public CameraController Controller => cameraController;

        public void Awake()
        {
            inputComponent = new PlayerMove();
            rigidbody2D = GetComponent<Rigidbody2D>();
            CharacterController = GetComponent<CharacterController2D>();
            playerCtrl = GetComponent<CharacterController>();
            cameraController = FindObjectOfType<CameraController>();
        }

        public void OnEnable()
        {
            inputComponent.Enable();
            inputComponent.Player.Move.started += Move;
            inputComponent.Player.Move.performed += Move;
            inputComponent.Player.Move.canceled += Move;
            //inputComponent.Player.Pick.performed += Pickup;
            // inputComponent.Player.Drop.performed += Drop;
        }

        public void OnDisable()
        {
            inputComponent.Player.Move.started -= Move;
            inputComponent.Player.Move.performed -= Move;
            inputComponent.Player.Move.canceled -= Move;
            //inputComponent.Player.Pick.performed -= Pickup;
            // inputComponent.Player.Drop.performed -= Drop;
            inputComponent.Disable();
        }

        public void Move(InputAction.CallbackContext cont)
        {
            if (GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret)
            {
                velocity = cont.ReadValue<Vector2>();
            }
        }

        public void Pickup(InputAction.CallbackContext callbackContext)
        {
            //playerCtrl.PickUpNearest();
        }
        
        private void Drop(InputAction.CallbackContext obj)
        {
            
        }

        public void ManuallyMove(Vector3 delta)
        {
            CharacterController.Move(delta);
        }

        private Vector2 GetMapSize()
        {
            if (mapSize == Vector2.zero && GameDataManager.Instance.CurStageType == EStateType.StageTypeSecret)
            {
                var curManager = (MapFightingManager)BattleManager.Instance.fightingManagerIns;
                if (curManager != null) mapSize = curManager.MapRange;
            }
            return mapSize;
        }

        private Vector3 ClampPlayerPos(Vector3 pos)
        {
            var rootPos = BattleManager.Instance.fightingManagerIns.fightBg.transform.position;
            pos -= rootPos;
            mapSize = GetMapSize();
            pos.x = ClampMinMax(pos.x, -mapSize.x / 2 + 2.56f, mapSize.x / 2 - 2.56f);
            pos.y = ClampMinMax(pos.y, -mapSize.y / 2 + 2.56f, mapSize.y / 2 - 2.56f);
            return pos + rootPos;
        }
        
        private float ClampMinMax(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
        
        private void FixedUpdate()
        {
            OnUpdate(Time.deltaTime);
        }

        private Vector3 currentDirection;
        public void OnUpdate(float deltaTime)
        {
            if (rigidbody2D.isKinematic) return;
            if (GameTime.Instance.Pause)
            {
                rigidbody2D.velocity = Vector3.zero;
                return;
            }
            // 移动
            if (velocity != Vector2.zero)
            {
                if (!IsMoving)
                {
                    playerCtrl.PlayWalk();
                }
                IsMoving = true;
                var speed = playerCtrl.RoleSpd;
                var direction = new Vector3(velocity.x, velocity.y, 0);
                direction.Normalize();
                currentDirection = direction;
                rigidbody2D.velocity = speed * direction;
                var pos = playerCtrl.transform.position;
                pos = ClampPlayerPos(pos);
                playerCtrl.transform.position = pos;
                RightDirection = direction;
                if (fullRotate) rotateRoot.right = direction;
                Controller.currentMoveDirection = direction;
                playerCtrl.FlipX(direction);
            }
            else
            {
                if (IsMoving)
                {
                    playerCtrl.PlayIdle();
                }
                IsMoving = false;
                rigidbody2D.velocity = Vector2.zero;
                currentDirection = Vector3.zero;
            }
        }

        private void OnCollisionStay2D(Collision2D col)
        {
            if (currentDirection == Vector3.zero) return;
            CollideEvent?.Invoke(CharacterController2D.CollideResult.Horizontal | CharacterController2D.CollideResult.Vertical, currentDirection);
        }
    }
}