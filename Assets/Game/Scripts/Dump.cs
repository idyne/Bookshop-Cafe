using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dump : MonoBehaviour
{
    [SerializeField] private Transform interactionPointTransform;

    public Transform InteractionPointTransform { get => interactionPointTransform; }
}
