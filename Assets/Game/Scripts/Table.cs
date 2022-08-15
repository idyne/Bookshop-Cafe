using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using UnityEngine.Events;

public class Table : MonoBehaviour
{
    [SerializeField] private int id = -1;
    [SerializeField] private Transform interactionPointTransform;
    [SerializeField] private EventDispatcher onLeave;
    private bool isServed = false;
    private UnityEvent onServe = new UnityEvent();
    private UnityEvent onSit = new UnityEvent();
    public int ID { get => id; }
    public Transform InteractionPointTransform { get => interactionPointTransform; }
    public EventDispatcher OnLeave { get => onLeave; }
    public UnityEvent OnServe { get => onServe; }
    public UnityEvent OnSit { get => onSit; }
    public bool IsServed { get => isServed; }

    public Customer Customer = null;

    private void Awake()
    {
        Deactivate();
        onSit.AddListener(() => { isServed = false; });
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void GetServed(Stackable coffee)
    {
        if (Customer)
        {
            Customer.StayingAtTable.Table = this;
            Customer.PushToStack(coffee);
            Customer.ChangeState(Customer.StayingAtTable);
            onServe.Invoke();
            isServed = true;
        }
    }
}
