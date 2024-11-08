using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DH.UIFramework
{
    public class TMP_TextEventHandler : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
    {
        [Serializable]
        public class CharacterSelectionEvent : UnityEvent<char, int>
        {
        }
        
        [Serializable]
        public class SpriteSelectionEvent : UnityEvent<char, int>
        {
        }

        [Serializable]
        public class WordSelectionEvent : UnityEvent<string, int, int>
        {
        }


        [Serializable]
        public class LineSelectionEvent : UnityEvent<string, int, int>
        {
        }
        
        [Serializable]
        public class LinkSelectionEvent : UnityEvent<string, string, int>
        {
        }


        /// <summary>
        /// Event delegate triggered when pointer is over a character.
        /// </summary>
        public CharacterSelectionEvent onCharacterSelection
        {
            get { return m_OnCharacterSelection; }
            set { m_OnCharacterSelection = value; }
        }

        [SerializeField] private CharacterSelectionEvent m_OnCharacterSelection = new CharacterSelectionEvent();


        /// <summary>
        /// Event delegate triggered when pointer is over a sprite.
        /// </summary>
        public SpriteSelectionEvent onSpriteSelection
        {
            get { return m_OnSpriteSelection; }
            set { m_OnSpriteSelection = value; }
        }

        [SerializeField] private SpriteSelectionEvent m_OnSpriteSelection = new SpriteSelectionEvent();
        
        /// <summary>
        /// Event delegate triggered when pointer is over a link.
        /// </summary>
        public LinkSelectionEvent onLinkSelection
        {
            get { return m_OnLinkSelection; }
            set { m_OnLinkSelection = value; }
        }

        [SerializeField] private LinkSelectionEvent m_OnLinkSelection = new LinkSelectionEvent();


        private TMP_Text m_TextComponent;

        private Camera m_Camera;
        private Canvas m_Canvas;

        private int m_selectedLink = -1;
        private int m_lastCharIndex = -1;
        private int m_lastWordIndex = -1;
        private int m_lastLineIndex = -1;
        private float m_pressDownTime = 0f;

        void Start()
        {
            // Get a reference to the text component.
            m_TextComponent = gameObject.GetComponent<TMP_Text>();

            if (m_TextComponent == null)
            {
                TMP_InputField inputField = gameObject.GetComponent<TMP_InputField>();

                if (inputField != null)
                {
                    m_TextComponent = inputField.textComponent;
                }
            }

            // Get a reference to the camera rendering the text taking into consideration the text component type.
            if (m_TextComponent != null && m_TextComponent.GetType() == typeof(TextMeshProUGUI))
            {
                m_Canvas = gameObject.GetComponentInParent<Canvas>();
                if (m_Canvas != null)
                {
                    if (m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                        m_Camera = null;
                    else
                        m_Camera = m_Canvas.worldCamera;
                }
            }
            else
            {
                m_Camera = Camera.main;
            }
        }

        private void SendOnCharacterSelection(char character, int characterIndex)
        {
            if (onCharacterSelection != null)
                onCharacterSelection.Invoke(character, characterIndex);
        }

        private void SendOnSpriteSelection(char character, int characterIndex)
        {
            if (onSpriteSelection != null)
                onSpriteSelection.Invoke(character, characterIndex);
        }
        
        private void SendOnLinkSelection(string linkID, string linkText, int linkIndex)
        {
            if (onLinkSelection != null)
                onLinkSelection.Invoke(linkID, linkText, linkIndex);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (m_pressDownTime > 0 && Time.time - m_pressDownTime > 0.5f)
            {
                return;
            }
            
            if (!m_TextComponent)
            {
                return;
            }
            
            if (TMP_TextUtilities.IsIntersectingRectTransform(m_TextComponent.rectTransform, eventData.pressPosition,
                m_Camera))
            {
                #region Example of Character or Sprite Selection

                int charIndex =
                    TMP_TextUtilities.FindIntersectingCharacter(m_TextComponent, eventData.pressPosition, m_Camera,
                        true);
                if (charIndex != -1 && charIndex != m_lastCharIndex)
                {
                    m_lastCharIndex = charIndex;

                    TMP_TextElementType elementType = m_TextComponent.textInfo.characterInfo[charIndex].elementType;

                    // Send event to any event listeners depending on whether it is a character or sprite.
                    if (elementType == TMP_TextElementType.Character)
                        SendOnCharacterSelection(m_TextComponent.textInfo.characterInfo[charIndex].character,
                            charIndex);
                    else if (elementType == TMP_TextElementType.Sprite)
                        SendOnSpriteSelection(m_TextComponent.textInfo.characterInfo[charIndex].character, charIndex);
                }

                #endregion
                
                #region Example of Link Handling
                // Check if mouse intersects with any links.
                int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextComponent, eventData.pressPosition, m_Camera);

                // Handle new Link selection.
                if (linkIndex != -1)
                {
                    m_selectedLink = linkIndex;

                    // Get information about the link.
                    TMP_LinkInfo linkInfo = m_TextComponent.textInfo.linkInfo[linkIndex];

                    // Send the event to any listeners. 
                    SendOnLinkSelection(linkInfo.GetLinkID(), linkInfo.GetLinkText(), linkIndex);
                }
                #endregion
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_pressDownTime = Time.time;
        }
    }
}