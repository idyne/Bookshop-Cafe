using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateGames
{
    public class CustomItemStack : MonoBehaviour
    {
        private List<Transform> slots;
        private Transform _transform;
        private List<Transform> collection = new List<Transform>();

        private void Awake()
        {
            _transform = transform;
            slots = GetChildrenWithTag(_transform, "Item Slot");
        }

        private List<Transform> GetChildrenWithTag(Transform parent, string tag)
        {
            List<Transform> result = new List<Transform>();
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.CompareTag(tag)) result.Add(child);
            }
            return result;
        }

        // Adds the item to the end of the stack.
        public void Push(Transform newItem)
        {
            if (collection.Count >= slots.Count)
            {
                Debug.LogError("Stack is full!", this);
                return;
            }
            Transform slot = slots[collection.Count];
            collection.Add(newItem);
            newItem.parent = slot;
            newItem.LeanMoveLocal(Vector3.zero, 0.2f);
            float delta = 0;
            LeanTween.value(newItem.gameObject, (val) =>
            {
                newItem.localRotation = Quaternion.Lerp(newItem.localRotation, Quaternion.identity, delta);
                delta = val - delta;
            }, 0, 1, 0.2f);
        }

        private Transform RemoveAt(int index)
        {
            if (collection.Count == 0)
            {
                Debug.LogError("Cannot remove from empty stack!", this);
                return null;
            }
            Transform objectToPop = collection[index];
            collection.RemoveAt(index);
            objectToPop.transform.parent = null;
            return objectToPop;
        }

        public Transform Pop()
        {
            return RemoveAt(collection.Count - 1);
        }

    }

}
