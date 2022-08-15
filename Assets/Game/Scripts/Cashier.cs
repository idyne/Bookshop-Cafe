using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cashier : MonoBehaviour
{
    
    private Checkout checkout;
    private float lastTime = float.MinValue;
    private float cooldown = 0.5f;

    

    private void Update()
    {
        if (checkout && lastTime + cooldown <= Time.time)
        {
            Customer customer = checkout.Peek();
            if (customer && customer.State == customer.InCheckout)
            {
                lastTime = Time.time;
                customer.InCheckout.Pay();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkout"))
            checkout = other.GetComponent<Checkout>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Checkout"))
        {
            Checkout otherCheckout = other.GetComponent<Checkout>();
            if (otherCheckout == checkout)
                checkout = null;
        }
    }

    
}
