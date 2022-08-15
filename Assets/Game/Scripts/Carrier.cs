using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using UnityEngine.Events;

[RequireComponent(typeof(EventDispatcher))]
public class Carrier : MonoBehaviour
{
    public int Limit { get => itemStack.Limit; set => itemStack.Limit = value; }
    private float cooldown { get => itemStack.PushPeriod; }
    [SerializeField] private EventDispatcher onPush;
    [SerializeField] private bool isWaitress = false;
    [SerializeField] private bool isCarrier = true;
    public TakeType TakingItemType = TakeType.ALL;
    private Transform _transform;
    private float lastDropTime = float.MinValue;
    [SerializeField] private ItemStack itemStack;
    private UnityEvent onDrop = new UnityEvent();

    private Transform overlappedBookSupplier = null;
    private Transform overlappedCupSupplier = null;
    private Transform overlappedDump = null;
    private List<Bookshelf> overlappedBookShelves = new List<Bookshelf>();
    private List<CoffeeVendingMachine> overlappedCoffeeVendingMachines = new List<CoffeeVendingMachine>();
    private List<CoffeeStand> overlappedCoffeeStands = new List<CoffeeStand>();
    private List<Table> overlappedTables = new List<Table>();

    public bool IsFull { get => Limit <= itemStack.Count; }
    public bool IsEmpty { get => itemStack.Count == 0; }
    public bool CanTakeItem { get => itemStack.CanPush; }
    public bool CanDropItem { get => lastDropTime + cooldown <= Time.time && itemStack.Count > 0; }
    public EventDispatcher OnPush { get => onPush; }
    public ItemStack ItemStack { get => itemStack; }
    public UnityEvent OnDrop { get => onDrop; }

    private void Awake()
    {
        _transform = transform;
        if (!itemStack)
            itemStack = GetComponentInChildren<ItemStack>();
    }

    private void Update()
    {
        if ((TakingItemType == TakeType.ALL || TakingItemType == TakeType.BOOK) && isCarrier && overlappedBookSupplier && CanTakeItem)
        {
            Stackable book = ObjectPooler.Instance.SpawnFromPool("Book", overlappedBookSupplier.position, Quaternion.identity).GetComponent<Stackable>();
            PushToStack(book);
        }
        else if ((TakingItemType == TakeType.ALL || TakingItemType == TakeType.CUP) && isCarrier && overlappedCupSupplier && CanTakeItem)
        {
            Stackable cup = ObjectPooler.Instance.SpawnFromPool("Empty Cup", overlappedCupSupplier.position, Quaternion.identity).GetComponent<Stackable>();
            PushToStack(cup);
        }
        else if (overlappedDump && itemStack.Count > 0 && CanDropItem)
        {
            Stackable item = itemStack.Pop();
            item.Transform.LeanMove(overlappedDump.position, cooldown).setOnComplete(() =>
            {
                item.gameObject.SetActive(false);
            });
            lastDropTime = Time.time;
            onDrop.Invoke();
        }
        else if (isCarrier && overlappedBookShelves.Count > 0)
        {
            for (int i = 0; i < overlappedBookShelves.Count; i++)
            {
                Bookshelf bookshelf = overlappedBookShelves[i];
                if (CanDropItem && !bookshelf.ItemHolder.IsFull)
                {
                    Stackable book = itemStack.PopItemWithTag("Book");
                    if (book)
                    {
                        bookshelf.ItemHolder.Add();
                        book.Transform.LeanMove(bookshelf.BookDestinationPointTransform.position, cooldown).setOnComplete(() =>
                        {
                            book.gameObject.SetActive(false);
                        });
                        lastDropTime = Time.time;
                        onDrop.Invoke();
                    }
                }
            }
        }
        else if (isWaitress && overlappedTables.Count > 0)
        {
            for (int i = 0; i < overlappedTables.Count; i++)
            {
                Table table = overlappedTables[i];
                if (CanDropItem && table.Customer && table.Customer.State == table.Customer.WaitingAtTable)
                {
                    if (table.Customer.CanTakeItem)
                    {
                        Stackable coffee = itemStack.PopItemWithTag("Coffee");
                        if (coffee)
                        {
                            table.GetServed(coffee);
                            lastDropTime = Time.time;
                            onDrop.Invoke();
                        }
                    }
                    
                }
            }
        }
        else
        {
            for (int i = 0; i < overlappedCoffeeVendingMachines.Count; i++)
            {
                CoffeeVendingMachine coffeeVendingMachine = overlappedCoffeeVendingMachines[i];

                bool isCoffeeStandCloser = Vector3.SqrMagnitude(_transform.position - coffeeVendingMachine.CoffeeStandPoint.position) < Vector3.SqrMagnitude(_transform.position - coffeeVendingMachine.EmptyCupPoint.position);
                if (isCarrier && (!isWaitress || !isCoffeeStandCloser))
                {

                    if (CanDropItem && !coffeeVendingMachine.ItemHolder.IsFull)
                    {
                        Stackable cup = itemStack.PopItemWithTag("Cup");
                        if (cup)
                        {
                            coffeeVendingMachine.ItemHolder.Add();
                            cup.Transform.LeanMove(coffeeVendingMachine.CupDestinationPoint.position, cooldown).setOnComplete(() =>
                            {
                                cup.gameObject.SetActive(false);
                            });
                            lastDropTime = Time.time;
                            onDrop.Invoke();
                        }
                    }
                }
                if ((!isCarrier || isCoffeeStandCloser) && (TakingItemType == TakeType.ALL || TakingItemType == TakeType.COFFEE) && isWaitress && overlappedCoffeeStands.Count > 0 && CanTakeItem)
                {
                    CoffeeStand coffeeStand = overlappedCoffeeStands[i];
                    Stackable coffee = coffeeStand.GetCoffee();
                    if (coffee)
                        PushToStack(coffee);
                }
            }
        }
    }

    public void PushToStack(Stackable item)
    {
        itemStack.Push(item, true);
        onPush?.Call();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Book Supplier"))
            overlappedBookSupplier = other.transform;
        else if (other.CompareTag("Cup Supplier"))
            overlappedCupSupplier = other.transform;
        else if (other.CompareTag("Dump"))
            overlappedDump = other.transform;
        else if (other.CompareTag("Book Shelf"))
            overlappedBookShelves.Add(other.GetComponent<Bookshelf>());
        else if (other.CompareTag("Coffee Vending Machine"))
        {
            overlappedCoffeeVendingMachines.Add(other.GetComponent<CoffeeVendingMachine>());
            if (isWaitress)
                overlappedCoffeeStands.Add(other.GetComponent<CoffeeStand>());
        }
        else if (other.CompareTag("Table"))
            overlappedTables.Add(other.GetComponent<Table>());
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Book Supplier") && other.transform == overlappedBookSupplier)
            overlappedBookSupplier = null;
        else if (other.CompareTag("Cup Supplier") && other.transform == overlappedCupSupplier)
            overlappedCupSupplier = null;
        else if (other.CompareTag("Dump") && other.transform == overlappedDump)
            overlappedDump = null;
        else if (other.CompareTag("Book Shelf"))
            overlappedBookShelves.Remove(other.GetComponent<Bookshelf>());
        else if (other.CompareTag("Coffee Vending Machine"))
        {
            overlappedCoffeeVendingMachines.Remove(other.GetComponent<CoffeeVendingMachine>());
            if (isWaitress)
                overlappedCoffeeStands.Remove(other.GetComponent<CoffeeStand>());
        }
        else if (other.CompareTag("Table"))
            overlappedTables.Remove(other.GetComponent<Table>());
    }

    public enum TakeType { BOOK, COFFEE, CUP, ALL, NONE }
}
