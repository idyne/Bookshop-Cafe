using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using DG.Tweening;

public class Stackable : MonoBehaviour
{
    [SerializeField] private string stackableTag;
    [SerializeField] private float identicalMargin, differentMargin;
    private Transform _transform;
    public ItemStack previousStack;
    public ItemStack currentStack;
    public ItemStack soCalledStack;
    public string StackableTag { get => stackableTag; }
    public float IdenticalMargin { get => identicalMargin; }
    public float DifferentMargin { get => differentMargin; }
    public Transform Transform { get => _transform; }
    public Tween WaveScaleTween = null;

    private void Awake()
    {
        _transform = transform;
    }



}
