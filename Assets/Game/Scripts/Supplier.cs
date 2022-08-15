using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Supplier : MonoBehaviour
{
    [SerializeField] private Transform interactionPointTransform;
    private Transform _transform;

    public Transform Transform { get => _transform; }
    public Transform InteractionPointTransform { get => interactionPointTransform; }

    private void Awake()
    {
        _transform = transform;
    }
}
