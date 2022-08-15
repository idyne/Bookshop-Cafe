using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FateGames;
using DG.Tweening;

[RequireComponent(typeof(ItemHolder))]
public class Bookshelf : MonoBehaviour
{
    [SerializeField] private int id = -1;
    [SerializeField] private Transform interactionPointTransform;
    [SerializeField] private Transform bookContainer;
    [SerializeField] private GameObject dangerImage;
    [SerializeField] private Transform bookDestinationPointTransform;
    private int numberOfActiveBooks = 0;
    private ItemHolder itemHolder;
    private PlayerData.ItemHolderData data;
    private List<Transform> books = new List<Transform>();
    private int? eventID = -1;
    public bool IsMissionAssigned = false;
    private Transform _transform;
    public PlayerData.ItemHolderData Data
    {
        get => data;
        set
        {
            data = value;
            if (!itemHolder) Debug.Log(name, this);
            itemHolder.NumberOfItems = data.NumberOfItems;
            itemHolder.OnChange.Remove((int)eventID);
            eventID = itemHolder.OnChange?.Bind((args) => { data.NumberOfItems = itemHolder.NumberOfItems; });
        }
    }

    public int ID { get => id; }
    public Transform InteractionPointTransform { get => interactionPointTransform; }
    public ItemHolder ItemHolder { get => itemHolder; }
    public Transform Transform { get => _transform; }
    public Transform BookDestinationPointTransform { get => bookDestinationPointTransform; }

    private void Awake()
    {
        _transform = transform;
        itemHolder = GetComponent<ItemHolder>();
        for (int i = 0; i < bookContainer.childCount; i++)
        {
            books.Add(bookContainer.GetChild(i));
        }
        itemHolder.OnChange.Bind((args) => { ArrangeBooks(((bool)args[2])); });
        itemHolder.OnChange.Bind((args) => { dangerImage.SetActive((int)args[1] == 0); });
        ArrangeBooks(false);
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

    private void ArrangeBooks(bool tween = true)
    {
        float ratio = itemHolder.NumberOfItems / (float)itemHolder.Size;
        int numberOfBooks = Mathf.CeilToInt(books.Count * ratio);
        if (numberOfBooks > numberOfActiveBooks)
        {
            for (int i = numberOfActiveBooks; i < numberOfBooks; i++)
            {
                Transform book = books[i];
                DOTween.Kill(book);
                if (tween)
                {
                    book.localScale = Vector3.zero;
                    book.gameObject.SetActive(true);
                    book.DOScale(1, 0.1f);
                }
                else
                {
                    book.localScale = Vector3.one;
                    book.gameObject.SetActive(true);
                }

            }
        }
        else
        {
            for (int i = numberOfActiveBooks - 1; i >= numberOfBooks; i--)
            {
                Transform book = books[i];
                if (tween)
                {
                    DOTween.Kill(book);
                    book.localScale = Vector3.one;
                    book.DOScale(0, 0.1f).OnComplete(() => { book.gameObject.SetActive(false); });
                }
                else
                {
                    book.localScale = Vector3.zero;
                    book.gameObject.SetActive(false);
                }

            }
        }
        numberOfActiveBooks = numberOfBooks;

    }

}

public enum ItemType
{
    BOOK,
    EMPTY_CUP,
    NULL
}