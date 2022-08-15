using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;

[RequireComponent(typeof(ItemHolder))]
public class CoffeeStand : MonoBehaviour
{
    [SerializeField] private ItemHolder itemHolder;
    private List<Transform> coffeePointTransforms;
    private Stackable[] coffees;
    private Transform _transform;


    public ItemHolder ItemHolder { get => itemHolder; }

    private void Awake()
    {
        _transform = transform;
        coffees = new Stackable[itemHolder.Size];
        coffeePointTransforms = FindChildrenTransformsWithTag("Coffee Point", _transform);
        for (int i = 0; i < coffeePointTransforms.Count; i++)
            Destroy(coffeePointTransforms[i].GetChild(0).gameObject);
    }

    public Stackable GetCoffee()
    {
        if (itemHolder.IsEmpty) return null;
        itemHolder.Remove();
        Stackable coffee = coffees[itemHolder.NumberOfItems];
        coffee.Transform.parent = null;
        return coffee;
    }

    public void AddCoffee()
    {
        if (itemHolder.IsFull) return;
        int index = itemHolder.NumberOfItems;
        Transform parent = coffeePointTransforms[index];
        Stackable coffee = ObjectPooler.Instance.SpawnFromPool("Coffee", parent.position, parent.rotation).GetComponent<Stackable>();
        coffee.Transform.parent = parent;
        coffees[index] = coffee;
        itemHolder.Add();
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

}
