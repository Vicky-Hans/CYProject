using DG.Tweening;
using DH.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using AudioType = DH.Game.AudioType;

namespace Extend
{
    public class DhButton:Button
    {
        [SerializeField]
        public AudioType audioType = AudioType.ButtonClick;
        public bool scaleEffect;
        public GameObject scaleEffectParent;
        private bool isGray;
        // [SerializeField]
        // private int m_event_to_next = 0;
        private Tweener scaleTweener;
        protected Vector3 CurScale;
        public Vector2 ClickPoint { get; set; }
        public bool IsGray
        {
            get => isGray;
            set
            {
                isGray = value;
                SetGrayActive(isGray);
            }
        }
        protected override void Awake()
        {
            CurScale = transform.localScale;
            if (transition == Transition.ColorTint)
            {
                var tempColors = colors;
                var tempColor = colors.disabledColor;
                tempColor.a = 1.0f;
                tempColors.disabledColor = tempColor;
                colors = tempColors;
            }

            var moveTf = scaleEffectParent == null ? transform : scaleEffectParent.transform;
            CurScale = moveTf.localScale;
            base.Awake();
        }

        public void SetInitScale( Vector3 _scale )
        {
            CurScale = _scale;
        }

        public override void OnPointerClick( PointerEventData eventData )
        {
            if( audioType != AudioType.None)
            {
                AudioManager.Instance.Play(audioType);
            }
            ClickPoint = eventData.position;
            base.OnPointerClick( eventData );
            // if( m_event_to_next > 0 )
            // {
            //     List<RaycastResult> results = new List<RaycastResult>();
            //     EventSystem.current.RaycastAll( eventData, results );
            //     GameObject current = eventData.pointerCurrentRaycast.gameObject;
            //     for( int i = 0; i < results.Count; i++ )
            //     {
            //         if( current != results[i].gameObject )
            //         {
            //             ExecuteEvents.Execute( results[i].gameObject, eventData, ExecuteEvents.pointerClickHandler );
            //             if(  i >= m_event_to_next )
            //             {
            //                 break;
            //             }
            //         }
            //     }
            // }
        }

        public override void OnPointerDown( PointerEventData eventData )
        {
            if( scaleEffect )
            {
                if( null != scaleTweener )
                    scaleTweener.Complete();

                var moveTf = scaleEffectParent == null ? transform : scaleEffectParent.transform;
                scaleTweener = moveTf.DOScale( CurScale * 0.8f, 0.1f );
            }
            base.OnPointerDown( eventData );
        }

        public override void OnPointerUp( PointerEventData eventData )
        {
            if( scaleEffect )
            {
                if( null != scaleTweener )
                    scaleTweener.Complete();
                var moveTf = scaleEffectParent == null ? transform : scaleEffectParent.transform;
                scaleTweener = moveTf.DOScale( CurScale, 0.1f );
            }
            base.OnPointerUp( eventData );
        }

        public void SetGrayActive( bool gray, bool recursive = true, bool purple = false)
        {
            UIHelper.SetGray(gameObject, gray, recursive,purple);
        }
        public void SetSound( AudioType type )
        {
            audioType = type;
        }
        
    }
}