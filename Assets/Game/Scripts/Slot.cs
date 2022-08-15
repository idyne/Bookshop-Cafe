using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using System;

public class Slot : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI moneyText;
    [SerializeField] protected int id = -1;
    [SerializeField] protected int level = 0;
    [SerializeField] protected int price = 0;
    [SerializeField] protected List<int> prerequisites;
    public Action<Slot> OnBuy;
    protected Transform _transform;
    protected PlayerData.SlotData data;
    public PlayerData.SlotData Data
    {
        get => data;
        set
        {
            data = value;
            Price = data.Price;
        }
    }

    public int ID { get => id; }
    public int Level { get => level; }
    public virtual int Price
    {
        get => price;
        set
        {
            price = value;
            Data.Price = price;
            UpdateMoneyText();
            if (price <= 0) Buy();
        }
    }

    public Transform Transform { get => _transform; set => _transform = value; }

    public void BindToOnSlotActivity()
    {
        bool arePrerequisitesMet = ArePrerequisitesMet();
        //print(name + ": " + arePrerequisitesMet);
        if (price > 0 && arePrerequisitesMet)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    private void Awake()
    {
        _transform = transform;
        BindToOnSlotActivity();
    }

    protected void Start()
    {
        UpdateMoneyText();
    }

    protected void UpdateMoneyText()
    {
        moneyText.text = "$" + Price.ToString();
    }

    protected void Buy()
    {
        OnBuy(this);
        Bookshop.Instance.AddBoughtSlot(this);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public bool ArePrerequisitesMet()
    {
        if (prerequisites.Count == 0) return true;
        bool result = false;
        for (int i = 0; i < prerequisites.Count; i++)
        {
            int id = prerequisites[i];
            for (int j = 0; j < Bookshop.Instance.BoughtSlots.Count; j++)
            {
                Slot slot = Bookshop.Instance.BoughtSlots[j];
                if (id == slot.ID)
                {
                    result = true;
                    break;
                }
            }
            if (!result)
                break;
            else if (i != prerequisites.Count - 1)
                result = false;
        }
        return result;
    }

}
