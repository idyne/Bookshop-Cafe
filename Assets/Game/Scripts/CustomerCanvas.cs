using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerCanvas : MonoBehaviour
{
    private Transform _transform;
    private Quaternion initialRotation;
    [SerializeField] private GameObject bookExpression, tableExpression, coffeeExpression, checkoutExpression, happyExpression;
    private GameObject currentExpression = null;

    private void Awake()
    {
        _transform = transform;
        initialRotation = _transform.rotation;
    }

    private void Update()
    {
        _transform.rotation = initialRotation;
    }

    public void ActivateExpression(ExpressionType type)
    {
        if (currentExpression)
            currentExpression.SetActive(false);
        transform.localScale = Vector3.one;
        switch (type)
        {
            case ExpressionType.BOOK:
                bookExpression.SetActive(true);
                currentExpression = bookExpression;
                break;
            case ExpressionType.TABLE:
                tableExpression.SetActive(true);
                currentExpression = tableExpression;
                break;
            case ExpressionType.COFFEE:
                coffeeExpression.SetActive(true);
                currentExpression = coffeeExpression;
                break;
            case ExpressionType.CHECKOUT:
                checkoutExpression.SetActive(true);
                currentExpression = checkoutExpression;
                break;
            case ExpressionType.HAPPY:
                happyExpression.SetActive(true);
                currentExpression = happyExpression;
                break;
            case ExpressionType.NONE:
                transform.localScale = Vector3.zero;
                currentExpression = null;
                break;
        }

    }

    public enum ExpressionType
    {
        BOOK,
        TABLE,
        COFFEE,
        CHECKOUT,
        HAPPY,
        NONE
    }
}
