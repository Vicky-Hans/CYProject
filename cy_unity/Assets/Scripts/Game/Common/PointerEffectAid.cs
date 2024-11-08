using Cysharp.Threading.Tasks;
using DH.Asset;
using DH.UIFramework;
using UnityEngine;

namespace DH.Game
{
    public class PointerEffectAid : BaseView
    {
        const string ClickEffectPrefab = "UI/Common/ClickEffectPrefab";
        const string TouchEffectPrefab = "UI/Common/TouchMoveEffectPrefab";
        private GameObject touchMoveEffectNode = null;

        private void Start()
        {
            DealySetTouchDown(new Vector2(10000,10000)).Forget();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 localPoint = new Vector2();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, Input.mousePosition, AppGlobal.Instance.UICamera, out localPoint);
                CreateClickEffect(localPoint);
            } else if (Input.GetMouseButton(0))
            {
                Vector2 localPoint = new Vector2();
                RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, Input.mousePosition, AppGlobal.Instance.UICamera, out localPoint);
                CreateAndMoveTouchMoveEffect(localPoint, false);
            } else if (Input.GetMouseButtonUp(0))
            {
                DelayReleaseTouchMoveEffect();
            }
        }

        private async UniTaskVoid DealySetTouchDown(Vector2 localPoint)
        {
            if(touchMoveEffectNode != null) return;
            CreateAndMoveTouchMoveEffect(localPoint, true);
        }

        private async UniTaskVoid DelayRelease(GameObject go)
        {
            
            await UniTask.Delay(1500);
            AssetsManager.ReleaseInstance(go);
        }
        private void DelayReleaseTouchMoveEffect()
        {
            
            // await UniTask.Delay(500);
            if (touchMoveEffectNode != null)
            {
                
                touchMoveEffectNode.transform.localPosition = new Vector3(10000, 10000);
                // AssetsManager.ReleaseInstance(touchMoveEffectNode);
                // touchMoveEffectNode = null;
            }
        }

        public async void CreateClickEffect(Vector2 pos)
        {

            GameObject go = await AssetsManager.InstantiateWithParentAsync(ClickEffectPrefab, transform, false);
            if (go == null)
                return;
            // go.transform.SetParent(transform);
            go.transform.localScale = Vector3.one;

            ParticleSystem ps = go.GetComponentInChildren<ParticleSystem>();
            go.transform.localPosition = pos;
            go.transform.rotation = Quaternion.identity;
            ps.Play(true);
            DelayRelease(go);
        }

        public async void CreateAndMoveTouchMoveEffect(Vector2 pos, bool isCrreate = true)
        {
            if (touchMoveEffectNode == null && isCrreate)
            {
                touchMoveEffectNode = await AssetsManager.InstantiateWithParentAsync(TouchEffectPrefab, transform, false);
                if (touchMoveEffectNode == null)
                    return;
                touchMoveEffectNode.transform.localScale = Vector3.one;
                touchMoveEffectNode.transform.rotation = Quaternion.identity;
                ParticleSystem ps = touchMoveEffectNode.GetComponentInChildren<ParticleSystem>();
                ps.Play(true);
                touchMoveEffectNode.transform.localPosition = pos;
            }
            else
            {
                if(touchMoveEffectNode == null) return;
                touchMoveEffectNode.transform.localPosition = pos;
            }
        }

        private void OnDestroy()
        {
            if(touchMoveEffectNode != null)
                AssetsManager.ReleaseInstance(touchMoveEffectNode);
        }
    }
}
