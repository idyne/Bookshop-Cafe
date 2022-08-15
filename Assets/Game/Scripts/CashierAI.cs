using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Cashier))]
public class CashierAI : MonoBehaviour
{
    [SerializeField] private int id = -1;

    public int ID { get => id; }
    private void Awake()
    {
        Deactivate();
    }
    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
