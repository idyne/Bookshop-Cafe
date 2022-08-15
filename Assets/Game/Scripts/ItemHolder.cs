using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;

public class ItemHolder : MonoBehaviour
{
    public int Size = 5;
    [SerializeField] private EventDispatcher onChange;
    [SerializeField] private TMPro.TextMeshProUGUI capacityText;
    private bool isInitialized = false;
    private int numberOfItems = 0;
    public int NumberOfItems
    {
        get => numberOfItems;
        set
        {
            int previousNumberOfItems = numberOfItems;
            numberOfItems = value;
            onChange?.Call(previousNumberOfItems, value, isInitialized);
            isInitialized = true;
        }
    }

    public bool IsFull { get => numberOfItems >= Size; }
    public bool IsEmpty { get => numberOfItems == 0; }
    public EventDispatcher OnChange { get => onChange; }

    private void Start()
    {
        UpdateCapacityText();
    }

    public bool Add()
    {
        if (IsFull) return false;
        NumberOfItems++;
        UpdateCapacityText();
        return true;
    }

    public bool Remove()
    {
        if (IsEmpty) return false;
        NumberOfItems--;
        UpdateCapacityText();
        return true;
    }

    private void UpdateCapacityText()
    {
        if (capacityText)
            capacityText.text = numberOfItems + "/" + Size;
    }
}
