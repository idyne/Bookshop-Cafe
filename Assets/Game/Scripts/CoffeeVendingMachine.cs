using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FateGames;
using System;

[RequireComponent(typeof(ItemHolder))]
[RequireComponent(typeof(CoffeeStand))]
public class CoffeeVendingMachine : MonoBehaviour
{
    [SerializeField] private int id = -1;
    [SerializeField] private float castDuration = 5;
    [SerializeField] private ItemHolder itemHolder;
    [SerializeField] private Transform interactionPointTransform;
    [SerializeField] private Transform cupDestinationPoint;
    [SerializeField] private Image castTimeImage;
    [SerializeField] private GameObject dangerImage;
    [SerializeField] private Transform coffeeStandPoint, emptyCupPoint;
    private PlayerData.ItemHolderData data;
    private float lastSpawnTime = float.MinValue;
    private List<Transform> cupPoints;
    private CoffeeStand coffeeStand;
    private Queue<Cast> coffeeSpawnCasts = new Queue<Cast>();
    private Cast currentCast = null;
    private int eventID = -1;
    private Transform _transform;
    public PlayerData.ItemHolderData Data
    {
        get => data;
        set
        {
            data = value;
            itemHolder.NumberOfItems = data.NumberOfItems;
            itemHolder.OnChange?.Remove(eventID);
            eventID = itemHolder.OnChange ? itemHolder.OnChange.Bind((args) => { data.NumberOfItems = itemHolder.NumberOfItems; }) : -1;
            for (int i = 0; i < data.NumberOfItems - 1; i++)
            {
                Cast cast = new Cast(castDuration, SpawnCoffee);
                coffeeSpawnCasts.Enqueue(cast);
            }
        }
    }

    public int ID { get => id; }
    public ItemHolder ItemHolder { get => itemHolder; }
    public Transform InteractionPointTransform { get => interactionPointTransform; }
    public Transform Transform { get => _transform; }
    public Transform CupDestinationPoint { get => cupDestinationPoint; }
    public Transform EmptyCupPoint { get => emptyCupPoint; }
    public Transform CoffeeStandPoint { get => coffeeStandPoint; }

    private void Awake()
    {
        _transform = transform;
        coffeeStand = GetComponent<CoffeeStand>();
        Deactivate();
        cupPoints = FindChildrenTransformsWithTag("Cup Point", transform);
        itemHolder.OnChange.Bind((args) =>
        {
            int previousNumberOfItems = (int)args[0];
            int numberOfItems = (int)args[1];
            if (numberOfItems > previousNumberOfItems)
            {
                Cast cast = new Cast(castDuration, SpawnCoffee);
                coffeeSpawnCasts.Enqueue(cast);
            }
        });
        itemHolder.OnChange.Bind((args) =>
        {
            int previousNumberOfItems = (int)args[0];
            int numberOfItems = (int)args[1];
            if (previousNumberOfItems < numberOfItems)
            {
                for (int i = previousNumberOfItems; i < numberOfItems; i++)
                {
                    cupPoints[i].gameObject.SetActive(true);
                }
            }
            else if (previousNumberOfItems > numberOfItems)
            {
                for (int i = previousNumberOfItems - 1; i >= numberOfItems; i--)
                {
                    cupPoints[i].gameObject.SetActive(false);
                }
            }

        });
        itemHolder.OnChange.Bind((args) => { dangerImage.SetActive((int)args[1] == 0); });
    }

    private List<Transform> FindChildrenTransformsWithTag(string tag, Transform parent)
    {
        List<Transform> result = new List<Transform>();
        if (parent.childCount == 0) return result;
        for (int i = 0; i < parent.childCount; ++i)
        {
            Transform child = parent.GetChild(i);
            if (child.CompareTag(tag))
                result.Add(child);
            result.AddRange(FindChildrenTransformsWithTag(tag, child));
        }
        return result;
    }

    private void Update()
    {
        if (currentCast == null && coffeeSpawnCasts.Count > 0 && !coffeeStand.ItemHolder.IsFull)
        {
            currentCast = coffeeSpawnCasts.Dequeue();
            castTimeImage.gameObject.SetActive(true);
        }
        if (currentCast != null && currentCast.Continue())
        {
            currentCast = null;
            castTimeImage.gameObject.SetActive(false);
        }
        if (currentCast != null) castTimeImage.fillAmount = 1f - currentCast.RemainingTime / castDuration;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void SpawnCoffee()
    {
        itemHolder.Remove();
        coffeeStand.AddCoffee();
    }


    private class Cast
    {
        private float remainingTime;
        private Action onFinished;

        public Cast(float duration, Action onFinished)
        {
            remainingTime = duration;
            this.onFinished = onFinished;
        }

        public float RemainingTime { get => remainingTime; }

        public bool Continue()
        {
            bool result = false;
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0)
            {
                onFinished();
                result = true;
            }
            return result;
        }
    }
}
