using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Elements
{
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldSlider: MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerClickHandler, IEndDragHandler, IPointerUpHandler
    {
        public float Sensitivity;
        
        private float _prevX;
        private float _prev;
        private TMP_InputField _input;
        
        private void Awake()
        {
            _input = GetComponent<TMP_InputField>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _prevX = eventData.position.x;
            if (!float.TryParse(_input.text, out _prev))
            {
                _prev = float.NaN;
            }
            _input.ReleaseSelection();
            eventData.Use();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!float.IsNaN(_prev))
            {
                _prev += (eventData.position.x - _prevX) * Sensitivity / Screen.width;
                _input.text = _prev.ToString("0.00");
            }

            _prevX = eventData.position.x;
            _input.ReleaseSelection();
            eventData.Use();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            SelectAll();
        }

        private void SelectAll()
        {
            _input.selectionStringAnchorPosition = 0;
            _input.selectionStringFocusPosition = _input.text.Length;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            SelectAll();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            SelectAll();
        }
    }
}