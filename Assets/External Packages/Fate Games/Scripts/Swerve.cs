using UnityEngine;
using UnityEngine.Events;
namespace FateGames
{
    public class Swerve : MonoBehaviour
    {
        private Vector2 anchor = Vector2.zero;
        private float rate = 0;
        private float xRate = 0;
        private float yRate = 0;
        private Vector2 difference = Vector2.zero;
        [SerializeField] private float range = Screen.width * 0.4f;
        [SerializeField] private bool isFloating = true;
        [SerializeField] private UnityEvent onStart;
        [SerializeField] private UnityEvent onSwerve;
        [SerializeField] private UnityEvent onRelease;
        private bool isActive = false;

        public float Rate { get => rate; }
        public float XRate { get => xRate; }
        public float YRate { get => yRate; }
        public Vector2 Difference { get => difference; }
        public UnityEvent OnStart { get => onStart; }
        public UnityEvent OnSwerve { get => onSwerve; }
        public UnityEvent OnRelease { get => onRelease; }
        public bool IsActive { get => isActive; }
        public Vector2 Anchor { get => anchor; }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Reset();
                onStart.Invoke();
                isActive = true;
            }
            else if (Input.GetMouseButton(0))
            {
                Vector2 difference = (Vector2)Input.mousePosition - anchor;
                if (difference.magnitude > range && isFloating)
                {
                    anchor = (Vector2)Input.mousePosition - difference.normalized * range;
                    difference = (Vector2)Input.mousePosition - anchor;
                }
                this.difference = difference.normalized;
                rate = Mathf.Clamp(difference.magnitude / range, 0, 1);
                xRate = Mathf.Clamp(difference.x / range, -1, 1);
                yRate = Mathf.Clamp(difference.y / range, -1, 1);
                onSwerve.Invoke();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isActive = false;
                onRelease.Invoke();
                Reset();
            }

        }
        public void Reset()
        {
            anchor = Input.mousePosition;
            difference = Vector2.zero;
            rate = 0;
        }

    }

}
