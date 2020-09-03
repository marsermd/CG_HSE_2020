using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Projections
{
    public class Point2D: MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        private float2 _originalPosition;
        private Transform _transform;
        private TransformMatrix2DView _transformMatrix;
        
        public void Awake()
        {
            Init();
            _transformMatrix.OnUpdate.AddListener(OnTransformMatrixUpdate);
            OnTransformMatrixUpdate();
        }

        private void Init()
        {
            _transform = transform;
            _originalPosition = new float2(_transform.position.x, _transform.position.y);
            _transformMatrix = FindObjectOfType<TransformMatrix2DView>();
        }
        
        private void OnTransformMatrixUpdate()
        {
            float2 pos = math.mul(_transformMatrix.Matrix, _originalPosition);
            transform.position = new Vector3(pos.x, pos.y, 0);
        }
        
        private void OnValidate()
        {
            Init();
        }

        public void OnSelect(BaseEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            throw new System.NotImplementedException();
        }
    }
}