using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace FateGames
{
    public class ItemStack : MonoBehaviour
    {
        private Dictionary<string, int> collectionRegistry = new Dictionary<string, int>();
        private List<Stackable> collection = new List<Stackable>();
        private Transform _transform;
        private UnityEvent<Stackable> onAdd = new UnityEvent<Stackable>();
        private UnityEvent onRemove = new UnityEvent();
        public float itemAdjustingSpeed = 40;
        public float itemAdjustingRotatingSpeed = 40;
        [SerializeField] private Swerve swerve;
        [SerializeField] private float _spring = 0.005f;
        private float spring = 0.00f;
        private AnimationCurve curve = new AnimationCurve();
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private bool wave = false;
        [SerializeField] private bool immobile = false;
        private Tween audioPitchResetTween = null;
        private Tween springTween = null;
        [SerializeField] private float pushPeriod = 0.2f;
        private float lastPushTime = -1f;
        public int Limit = -1;
        public bool CanPush { get => Time.time >= lastPushTime + pushPeriod && ((Limit >= 0 && collection.Count < Limit) || Limit < 0); }
        public int Count { get => collection.Count; }
        public Dictionary<string, int> CollectionRegistry { get => collectionRegistry; }
        public UnityEvent<Stackable> OnAdd { get => onAdd; }
        public UnityEvent OnRemove { get => onRemove; }
        public float PushPeriod { get => pushPeriod; }

        private void Awake()
        {
            _transform = transform;
            onAdd.AddListener((newItem) =>
            {
                if (!collectionRegistry.ContainsKey(newItem.StackableTag)) collectionRegistry.Add(newItem.StackableTag, 1);
                else collectionRegistry[newItem.StackableTag]++;
            });
            InitializeCurve();
            SetAudioSource();
            SetSwerve();
        }
        [SerializeField] private Quaternion itemRotation;

        #region Swerve
        private void SetSwerve()
        {
            swerve?.OnStart.AddListener(() =>
            {
                if (springTween != null)
                {
                    springTween.Kill();
                    springTween = null;
                }
                spring = _spring;
            });
            swerve?.OnRelease.AddListener(() =>
            {
                springTween = DOTween.To((val) =>
                {
                    spring = val < 0 ? val * 3 : val;
                }, _spring, 0, 1f).SetEase(Ease.OutElastic);
            });
        }
        #endregion

        #region Audio
        private void SetAudioSource()
        {
            audioSource = GetComponent<AudioSource>();
            if (!audioSource) return;
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        private void ResetAudioPitch()
        {
            if (audioPitchResetTween != null)
                audioPitchResetTween.Kill();
            audioPitchResetTween = DOVirtual.DelayedCall(0.3f, () =>
            {
                audioSource.pitch = 1;
                audioPitchResetTween = null;
            });
        }

        private void PlayAddingAudio()
        {
            if (!audioSource) return;
            audioSource.Play();
            audioSource.pitch += 0.05f;
            ResetAudioPitch();
        }

        #endregion

        #region Curve

        private void InitializeCurve()
        {
            curve.AddKey(0, 0);
            curve.AddKey(0.5f, 1.5f);
            curve.AddKey(1, 0);
        }
        private void SetCurve()
        {
            Keyframe newKey = curve.keys[1];
            newKey.value = 1.5f + collection.Count * 0.2f;
            curve.MoveKey(1, newKey);
        }
        #endregion

        private void FixedUpdate()
        {
            if (immobile) return;
            if (collection.Count == 0) return;
            Stackable firstItem = collection[0];
            firstItem.Transform.position = Vector3.Lerp(firstItem.Transform.position, _transform.position, itemAdjustingSpeed * Time.fixedDeltaTime);
            firstItem.Transform.rotation = Quaternion.Lerp(firstItem.Transform.rotation, _transform.rotation, itemAdjustingRotatingSpeed * Time.fixedDeltaTime);
            for (int i = 1; i < collection.Count; i++)
            {
                Stackable previousItem = collection[i - 1];
                Stackable currentItem = collection[i];
                currentItem.Transform.position = Vector3.Lerp(currentItem.Transform.position, -_transform.forward * spring * i + previousItem.Transform.position + Vector3.up * (previousItem.StackableTag == currentItem.StackableTag ? previousItem.IdenticalMargin : previousItem.DifferentMargin), Time.fixedDeltaTime * itemAdjustingSpeed);
                currentItem.Transform.rotation = Quaternion.Lerp(currentItem.Transform.rotation, previousItem.Transform.rotation, Time.fixedDeltaTime * itemAdjustingRotatingSpeed);
            }
        }



        // Adds the item to the end of the stack.
        public void Push(Stackable newItem, bool audio = false, bool overrideWave = false, bool haptic = false)
        {
            if (!newItem || !CanPush) return;
            newItem.soCalledStack = this;
            lastPushTime = Time.time;
            if (audio)
                PlayAddingAudio();
            SetCurve();
            Vector3 start = newItem.Transform.position;
            if (haptic) HapticManager.DoHaptic();

            void PushAnimation(float val)
            {
                Vector3 end;
                Stackable previousItem = null;
                if (collection.Count == 0)
                    end = _transform.position;
                else
                {
                    previousItem = collection[collection.Count - 1];
                    end = previousItem.Transform.position + Vector3.up * (previousItem.StackableTag == newItem.StackableTag ? previousItem.IdenticalMargin : previousItem.DifferentMargin);
                }
                Vector3 pos = Vector3.Lerp(start, end, val);
                pos.y += curve.Evaluate(val);
                newItem.Transform.position = pos;
                if (previousItem)
                    newItem.Transform.rotation = Quaternion.Lerp(newItem.Transform.rotation, previousItem.Transform.rotation, Time.fixedDeltaTime * itemAdjustingRotatingSpeed);
                else
                    newItem.Transform.rotation = Quaternion.Lerp(newItem.Transform.rotation, _transform.rotation, Time.fixedDeltaTime * itemAdjustingRotatingSpeed);

            }

            void OnAnimationComplete()
            {
                collection.Add(newItem);
                onAdd.Invoke(newItem);
                newItem.previousStack = newItem.currentStack;
                newItem.currentStack = this;
                newItem.soCalledStack = null;
                if (overrideWave || wave)
                    StartWave();
            }

            DOTween.To(PushAnimation, 0, 1f, pushPeriod - 0.01f)
                .OnComplete(OnAnimationComplete);

        }
        private void StartWave()
        {
            StartCoroutine(_Wave(collection.Count - 1));
        }

        private IEnumerator _Wave(int index)
        {
            if (index < 0 || index >= collection.Count) yield break;
            Stackable item = collection[index];
            if (item.WaveScaleTween != null)
            {
                item.WaveScaleTween.Kill();
                item.Transform.localScale = Vector3.one;
            }
            item.WaveScaleTween = item.Transform.DOScale(1.35f, 0.05f).SetLoops(2, LoopType.Yoyo);
            yield return new WaitForSeconds(0.03f);
            StartCoroutine(_Wave(index - 1));
        }

        // Adds the item to the front of the stack.
        public void Insert(Stackable newItem)
        {
            newItem.Transform.parent = _transform;
            collection.Insert(0, newItem);
            newItem.Transform.localRotation = itemRotation;

            if (!collectionRegistry.ContainsKey(newItem.StackableTag)) collectionRegistry.Add(newItem.StackableTag, 1);
            else collectionRegistry[newItem.StackableTag]++;
            onAdd.Invoke(newItem);
            //newItem.Deactivate();
        }

        public void Clear()
        {
            while (collection.Count > 0)
                RemoveAt(0).gameObject.SetActive(false);
        }

        public Stackable Pop()
        {
            return RemoveAt(collection.Count - 1);
        }

        public Stackable Dequeue()
        {
            Stackable result = RemoveAt(0);
            return result;
        }

        public Stackable RemoveAt(int index)
        {
            if (collection.Count == 0)
            {
                Debug.LogError("Cannot remove from empty stack!", this);
                return null;
            }
            Stackable objectToPop = collection[index];
            collection.RemoveAt(index);
            objectToPop.Transform.parent = null;
            collectionRegistry[objectToPop.StackableTag]--;
            onRemove.Invoke();
            PlayAddingAudio();
            objectToPop.previousStack = this;
            objectToPop.currentStack = null;
            return objectToPop;
        }

        public bool Remove(Stackable item)
        {
            if (collection.Count == 0)
            {
                Debug.LogError("Cannot remove from empty stack!", this);
                return false;
            }
            bool result = collection.Remove(item);
            if (result)
                item.transform.parent = null;
            return result;
        }

        public Stackable PopItemWithTag(string tag)
        {
            for (int i = collection.Count - 1; i >= 0; --i)
            {
                Stackable item = collection[i];
                if (item.StackableTag == tag)
                {
                    Stackable result = RemoveAt(i);
                    return result;
                }
            }
            return null;
        }

        public Stackable DequeueItemWithTag(string tag)
        {
            for (int i = 0; i < collection.Count; ++i)
            {
                Stackable item = collection[i];
                if (item.StackableTag == tag)
                {
                    Stackable result = RemoveAt(i);
                    return result;
                }
            }
            return null;
        }
    }

}
