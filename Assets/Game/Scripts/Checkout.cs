using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Checkout : MonoBehaviour
{
    [SerializeField] private MoneyPile moneyPile;
    private List<Transform> interactionPointTransforms;
    private List<Customer> customersInQueue = new List<Customer>();
    private int limit = 0;

    public MoneyPile MoneyPile { get => moneyPile; }
    public int Limit { get => limit; }
    public bool IsFull { get => customersInQueue.Count == limit; }

    private void Awake()
    {
        interactionPointTransforms = FindChildrenTransformsWithTag("Checkout Queue Point", transform);
        limit = interactionPointTransforms.Count;
    }

    public Transform Enqueue(Customer customer)
    {
        Transform result = interactionPointTransforms[customersInQueue.Count];
        customersInQueue.Add(customer);
        return result;
    }

    public void Dequeue(Customer customer)
    {
        customersInQueue.Remove(customer);
        RearrangeQueue();
    }

    public Customer Peek()
    {
        if (customersInQueue.Count == 0) return null;
        return customersInQueue[0];
    }

    private void RearrangeQueue()
    {
        int count = customersInQueue.Count;
        int i = 0;
        while (i++ < count)
        {
            Customer customer = customersInQueue[0];
            customersInQueue.RemoveAt(0);
            customer.ChangeState(customer.GoingToCheckout);
        }
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
