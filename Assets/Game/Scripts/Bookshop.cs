using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using System.Linq;
using UnityEngine.AI;
using UnityEngine.Events;

public class Bookshop : MonoBehaviour
{
    [SerializeField] private Transform customerSpawnPointTransform;
    [SerializeField] private Transform customerLeavePointTransform;
    [SerializeField] private Checkout checkout;
    [SerializeField] private Dump dump;
    [SerializeField] private NavMeshSurface navMeshSurface;
    private static Bookshop instance;
    private Transform _transform;
    private Dictionary<int, Slot> bookShelfSlotDictionary = new Dictionary<int, Slot>();
    private Dictionary<int, Slot> vendingMachineSlotDictionary = new Dictionary<int, Slot>();
    private Dictionary<int, Slot> tableSlotDictionary = new Dictionary<int, Slot>();
    private Dictionary<int, ExpandSlot> expandSlotDictionary = new Dictionary<int, ExpandSlot>();
    private Dictionary<int, Slot> carrierHiringSlotDictionary = new Dictionary<int, Slot>();
    private Dictionary<int, Slot> waitressHiringSlotDictionary = new Dictionary<int, Slot>();
    private Dictionary<int, Slot> cashierHiringSlotDictionary = new Dictionary<int, Slot>();
    private Dictionary<int, Slot> playerLimitSlotDictionary = new Dictionary<int, Slot>();
    private Dictionary<int, Slot> playerSpeedSlotDictionary = new Dictionary<int, Slot>();
    private Dictionary<int, Slot> carrierLimitSlotDictionary = new Dictionary<int, Slot>();
    private Dictionary<int, Slot> carrierSpeedSlotDictionary = new Dictionary<int, Slot>();
    private Dictionary<int, Slot> waitressLimitSlotDictionary = new Dictionary<int, Slot>();
    private Dictionary<int, Slot> waitressSpeedSlotDictionary = new Dictionary<int, Slot>();
    private Dictionary<int, Bookshelf> bookShelfDictionary = new Dictionary<int, Bookshelf>();
    private Dictionary<int, CoffeeVendingMachine> vendingMachineDictionary = new Dictionary<int, CoffeeVendingMachine>();
    private Dictionary<int, Table> tableDictionary = new Dictionary<int, Table>();
    private Dictionary<int, CarrierAI> carrierDictionary = new Dictionary<int, CarrierAI>();
    private Dictionary<int, CarrierAI> waitressDictionary = new Dictionary<int, CarrierAI>();
    private Dictionary<int, CashierAI> cashierDictionary = new Dictionary<int, CashierAI>();
    private PlayerData data;
    private Transform meshContainer;
    private List<Customer> customers = new List<Customer>();
    private int numberOfAvaliableBooks = 0;
    private List<Bookshelf> avaliableBookshelfs = new List<Bookshelf>();
    private List<Table> avaliableTables = new List<Table>();

    private int expandCount = 0;

    private List<Slot> boughtSlots = new List<Slot>();
    private UnityEvent onSlotActivity = new UnityEvent();
    private List<Bookshelf> boughtBookshelfs = new List<Bookshelf>();
    private List<Table> boughtTables = new List<Table>();

    private Player player;

    private List<Supplier> bookSuppliers;
    private List<Supplier> cupSuppliers;
    private List<Supplier> coffeeSuppliers;

    private HashSet<Bookshelf> incompleteBookshelfs = new HashSet<Bookshelf>();
    private HashSet<CoffeeVendingMachine> incompleteVendingMachines = new HashSet<CoffeeVendingMachine>();
    private HashSet<Table> incompleteTables = new HashSet<Table>();

    private UnityEvent onPlayerLimitSlotBuy = new UnityEvent();
    private UnityEvent onPlayerSpeedSlotBuy = new UnityEvent();
    private UnityEvent onCarrierLimitSlotBuy = new UnityEvent();
    private UnityEvent onCarrierSpeedSlotBuy = new UnityEvent();
    private UnityEvent onWaitressLimitSlotBuy = new UnityEvent();
    private UnityEvent onWaitressSpeedSlotBuy = new UnityEvent();

    public static Bookshop Instance { get => instance; }
    public Checkout Checkout { get => checkout; }
    public Transform CustomerLeavePointTransform { get => customerLeavePointTransform; }
    public List<Supplier> BookSuppliers { get => bookSuppliers; }
    public List<Supplier> CupSuppliers { get => cupSuppliers; }
    public HashSet<Bookshelf> IncompleteBookshelfs { get => incompleteBookshelfs; }
    public HashSet<CoffeeVendingMachine> IncompleteVendingMachines { get => incompleteVendingMachines; }
    public Dump Dump { get => dump; }
    public HashSet<Table> IncompleteTables { get => incompleteTables; }
    public List<Supplier> CoffeeSuppliers { get => coffeeSuppliers; }
    public UnityEvent OnPlayerLimitSlotBuy { get => onPlayerLimitSlotBuy; }
    public UnityEvent OnPlayerSpeedSlotBuy { get => onPlayerSpeedSlotBuy; }
    public UnityEvent OnCarrierLimitSlotBuy { get => onCarrierLimitSlotBuy; }
    public UnityEvent OnCarrierSpeedSlotBuy { get => onCarrierSpeedSlotBuy; }
    public UnityEvent OnWaitressLimitSlotBuy { get => onWaitressLimitSlotBuy; }
    public UnityEvent OnWaitressSpeedSlotBuy { get => onWaitressSpeedSlotBuy; }
    public List<Slot> BoughtSlots { get => boughtSlots; }
    public UnityEvent OnSlotActivity { get => onSlotActivity; }

    #region Initializing

    private void Awake()
    {
        // Singleton
        if (instance)
        {
            DestroyImmediate(gameObject);
            return;
        }
        instance = this;

        data = PlayerProgression.PlayerData;
        player = FindObjectOfType<Player>();
        _transform = transform;
        meshContainer = _transform.Find("Mesh Container");

        onPlayerLimitSlotBuy.AddListener(() => {
            player.Carrier.Limit++;   
        });
        onPlayerSpeedSlotBuy.AddListener(() => { player.Movement.speed += 1; });

        // Find all book suppliers by their tag
        bookSuppliers = FindChildrenWithTag<Supplier>("Book Supplier");

        // Find all cup suppliers by their tag
        cupSuppliers = FindChildrenWithTag<Supplier>("Cup Supplier");

        // Find all cup suppliers by their tag
        coffeeSuppliers = FindChildrenWithTag<Supplier>("Coffee Vending Machine");

        // Find all bookshelf slots by their tag
        List<Slot> bookShelfSlots = FindChildrenWithTag<Slot>("Book Shelf Slot");
        for (int i = 0; i < bookShelfSlots.Count; i++)
        {
            Slot slot = bookShelfSlots[i];
            bookShelfSlotDictionary.Add(slot.ID, slot);
            onSlotActivity.AddListener(slot.BindToOnSlotActivity);
        }

        // Find all coffee vending machine slots by their tag
        List<Slot> vendingMachineSlots = FindChildrenWithTag<Slot>("Coffee Vending Machine Slot");
        for (int i = 0; i < vendingMachineSlots.Count; i++)
        {
            Slot slot = vendingMachineSlots[i];
            vendingMachineSlotDictionary.Add(slot.ID, slot);
            onSlotActivity.AddListener(slot.BindToOnSlotActivity);
        }

        // Find all coffee table slots by their tag
        List<Slot> tableSlots = FindChildrenWithTag<Slot>("Table Slot");
        for (int i = 0; i < tableSlots.Count; i++)
        {
            Slot slot = tableSlots[i];
            tableSlotDictionary.Add(slot.ID, slot);
            onSlotActivity.AddListener(slot.BindToOnSlotActivity);
        }

        // Find all expand slots by their tag
        List<ExpandSlot> expandSlots = FindChildrenWithTag<ExpandSlot>("Expand Slot");
        for (int i = 0; i < expandSlots.Count; i++)
        {
            ExpandSlot slot = expandSlots[i];
            expandSlotDictionary.Add(slot.ID, slot);
            onSlotActivity.AddListener(slot.BindToOnSlotActivity);
        }

        // Find all carrier hiring slots by their tag
        List<Slot> carrierHiringSlots = FindChildrenWithTag<Slot>("Carrier Hiring Slot");
        for (int i = 0; i < carrierHiringSlots.Count; i++)
        {
            Slot slot = carrierHiringSlots[i];
            carrierHiringSlotDictionary.Add(slot.ID, slot);
            onSlotActivity.AddListener(slot.BindToOnSlotActivity);
        }

        // Find all waitress hiring slots by their tag
        List<Slot> waitressHiringSlots = FindChildrenWithTag<Slot>("Waitress Hiring Slot");
        for (int i = 0; i < waitressHiringSlots.Count; i++)
        {
            Slot slot = waitressHiringSlots[i];
            waitressHiringSlotDictionary.Add(slot.ID, slot);
            onSlotActivity.AddListener(slot.BindToOnSlotActivity);
        }

        // Find all cashier hiring slots by their tag
        List<Slot> cashierHiringSlots = FindChildrenWithTag<Slot>("Cashier Hiring Slot");
        for (int i = 0; i < cashierHiringSlots.Count; i++)
        {
            Slot slot = cashierHiringSlots[i];
            cashierHiringSlotDictionary.Add(slot.ID, slot);
            onSlotActivity.AddListener(slot.BindToOnSlotActivity);
        }

        // Find all player limit slots by their tag
        List<Slot> playerLimitSlots = FindChildrenWithTag<Slot>("Player Limit Slot");
        for (int i = 0; i < playerLimitSlots.Count; i++)
        {
            Slot slot = playerLimitSlots[i];
            playerLimitSlotDictionary.Add(slot.ID, slot);
            onSlotActivity.AddListener(slot.BindToOnSlotActivity);
        }

        // Find all player speed slots by their tag
        List<Slot> playerSpeedSlots = FindChildrenWithTag<Slot>("Player Speed Slot");
        for (int i = 0; i < playerSpeedSlots.Count; i++)
        {
            Slot slot = playerSpeedSlots[i];
            playerSpeedSlotDictionary.Add(slot.ID, slot);
            onSlotActivity.AddListener(slot.BindToOnSlotActivity);
        }

        // Find all carrier limit slots by their tag
        List<Slot> carrierLimitSlots = FindChildrenWithTag<Slot>("Carrier Limit Slot");
        for (int i = 0; i < carrierLimitSlots.Count; i++)
        {
            Slot slot = carrierLimitSlots[i];
            carrierLimitSlotDictionary.Add(slot.ID, slot);
            onSlotActivity.AddListener(slot.BindToOnSlotActivity);
        }

        // Find all carrier speed slots by their tag
        List<Slot> carrierSpeedSlots = FindChildrenWithTag<Slot>("Carrier Speed Slot");
        for (int i = 0; i < carrierSpeedSlots.Count; i++)
        {
            Slot slot = carrierSpeedSlots[i];
            carrierSpeedSlotDictionary.Add(slot.ID, slot);
            onSlotActivity.AddListener(slot.BindToOnSlotActivity);
        }

        // Find all waitress limit slots by their tag
        List<Slot> waitressLimitSlots = FindChildrenWithTag<Slot>("Waitress Limit Slot");
        for (int i = 0; i < waitressLimitSlots.Count; i++)
        {
            Slot slot = waitressLimitSlots[i];
            waitressLimitSlotDictionary.Add(slot.ID, slot);
            onSlotActivity.AddListener(slot.BindToOnSlotActivity);
        }

        // Find all waitress speed slots by their tag
        List<Slot> waitressSpeedSlots = FindChildrenWithTag<Slot>("Waitress Speed Slot");
        for (int i = 0; i < waitressSpeedSlots.Count; i++)
        {
            Slot slot = waitressSpeedSlots[i];
            waitressSpeedSlotDictionary.Add(slot.ID, slot);
            onSlotActivity.AddListener(slot.BindToOnSlotActivity);
        }

        // Find all bookshelfs by their tag
        List<Bookshelf> bookShelfs = FindChildrenWithTag<Bookshelf>("Book Shelf");
        for (int i = 0; i < bookShelfs.Count; i++)
        {
            Bookshelf bookshelf = bookShelfs[i];
            bookShelfDictionary.Add(bookshelf.ID, bookshelf);
        }

        // Bind event on bookshelfs' item holder's OnChange. It adds the difference to the number of avaliable books.
        for (int i = 0; i < bookShelfs.Count; i++)
        {
            Bookshelf bookshelf = bookShelfs[i];
            bookshelf.ItemHolder.OnChange.Bind((args) =>
            {
                int previousNumberOfBooks = (int)args[0];
                int numberOfBooks = (int)args[1];
                int difference = numberOfBooks - previousNumberOfBooks;

                if (bookshelf.gameObject.activeSelf)
                {
                    if (!bookshelf.ItemHolder.IsFull)
                        incompleteBookshelfs.Add(bookshelf);
                    else
                        incompleteBookshelfs.Remove(bookshelf);
                }


                if (difference > 0)
                    numberOfAvaliableBooks += difference;
                if (previousNumberOfBooks == 0 && numberOfBooks > 0)
                    avaliableBookshelfs.Add(bookshelf);
                else if (previousNumberOfBooks > 0 && numberOfBooks == 0)
                    avaliableBookshelfs.Remove(bookshelf);
            });
        }


        // Find all vending machines by their tag
        List<CoffeeVendingMachine> vendingMachines = FindChildrenWithTag<CoffeeVendingMachine>("Coffee Vending Machine");
        for (int i = 0; i < vendingMachines.Count; i++)
        {
            CoffeeVendingMachine vendingMachine = vendingMachines[i];
            vendingMachineDictionary.Add(vendingMachine.ID, vendingMachine);
        }

        // Bind event on vending machines' item holder's OnChange. 
        for (int i = 0; i < vendingMachines.Count; i++)
        {
            CoffeeVendingMachine vendingMachine = vendingMachines[i];
            vendingMachine.ItemHolder.OnChange.Bind((args) =>
            {
                if (vendingMachine.gameObject.activeSelf)
                {
                    if (!vendingMachine.ItemHolder.IsFull)
                        incompleteVendingMachines.Add(vendingMachine);
                    else
                        incompleteVendingMachines.Remove(vendingMachine);
                }

            });
        }

        // Find all tables by their tag
        List<Table> tables = FindChildrenWithTag<Table>("Table");
        for (int i = 0; i < tables.Count; i++)
        {
            Table table = tables[i];
            tableDictionary.Add(table.ID, table);
        }

        // Find all carriers by their tag
        List<CarrierAI> carriers = FindChildrenWithTag<CarrierAI>("Carrier");
        for (int i = 0; i < carriers.Count; i++)
        {
            CarrierAI carrier = carriers[i];
            carrierDictionary.Add(carrier.ID, carrier);
        }

        // Bind carrier
        for (int i = 0; i < carriers.Count; i++)
        {
            CarrierAI carrier = carriers[i];
            onCarrierLimitSlotBuy.AddListener(() => { carrier.Carrier.Limit++; });
            onCarrierSpeedSlotBuy.AddListener(() => { carrier.Agent.speed++; });
        }

        // Find all waitresses by their tag
        List<CarrierAI> waitresses = FindChildrenWithTag<CarrierAI>("Waitress");
        for (int i = 0; i < waitresses.Count; i++)
        {
            CarrierAI waitress = waitresses[i];
            waitressDictionary.Add(waitress.ID, waitress);
        }

        // Bind waitress
        for (int i = 0; i < waitresses.Count; i++)
        {
            CarrierAI waitress = waitresses[i];
            onWaitressLimitSlotBuy.AddListener(() => { waitress.Carrier.Limit++; });
            onWaitressSpeedSlotBuy.AddListener(() => { waitress.Agent.speed++; });
        }

        // Find all cashiers by their tag
        List<CashierAI> cashiers = FindChildrenWithTag<CashierAI>("Cashier");
        for (int i = 0; i < cashiers.Count; i++)
        {
            CashierAI cashier = cashiers[i];
            cashierDictionary.Add(cashier.ID, cashier);
        }

        // Bind event on tables' OnServe. 
        for (int i = 0; i < tables.Count; i++)
        {
            Table table = tables[i];
            table.OnServe.AddListener(() =>
            {
                if (table.gameObject.activeSelf)
                    incompleteTables.Remove(table);
            });
            table.OnSit.AddListener(() =>
            {
                if (table.gameObject.activeSelf)
                    incompleteTables.Add(table);
            });
        }

        Initialize();

        int count = 0;
        foreach (Bookshelf bookshelf in bookShelfDictionary.Values)
            count += bookshelf.ItemHolder.NumberOfItems;
        numberOfAvaliableBooks = count;
        avaliableTables = tableDictionary.Values.Where((table) => table.gameObject.activeSelf).ToList();
    }


    public void Initialize()
    {
        PlayerData data = PlayerProgression.PlayerData;

        // Bind events to bookshelf slots OnBuy
        foreach (Slot slot in bookShelfSlotDictionary.Values)
            slot.OnBuy = SpawnBookShelf;

        // Bind events to vending machine slots OnBuy
        foreach (Slot slot in vendingMachineSlotDictionary.Values)
            slot.OnBuy = SpawnVendingMachine;

        // Bind events to expand slots OnBuy
        foreach (ExpandSlot slot in expandSlotDictionary.Values)
            slot.OnBuy = Expand;

        // Bind events to table slots OnBuy
        foreach (Slot slot in tableSlotDictionary.Values)
            slot.OnBuy = SpawnTable;

        // Bind events to carrier hiring slots OnBuy
        foreach (Slot slot in carrierHiringSlotDictionary.Values)
            slot.OnBuy = HireCarrier;

        // Bind events to waitress hiring slots OnBuy
        foreach (Slot slot in waitressHiringSlotDictionary.Values)
            slot.OnBuy = HireWaitress;

        // Bind events to cashier hiring slots OnBuy
        foreach (Slot slot in cashierHiringSlotDictionary.Values)
            slot.OnBuy = HireCashier;

        // Bind events to player limit slots OnBuy
        foreach (Slot slot in playerLimitSlotDictionary.Values)
            slot.OnBuy = IncreasePlayerLimit;

        // Bind events to player speed slots OnBuy
        foreach (Slot slot in playerSpeedSlotDictionary.Values)
            slot.OnBuy = IncreasePlayerSpeed;

        // Bind events to carrier limit slots OnBuy
        foreach (Slot slot in carrierLimitSlotDictionary.Values)
            slot.OnBuy = IncreaseCarrierLimit;

        // Bind events to carrier speed slots OnBuy
        foreach (Slot slot in carrierSpeedSlotDictionary.Values)
            slot.OnBuy = IncreaseCarrierSpeed;

        // Bind events to waitress limit slots OnBuy
        foreach (Slot slot in waitressLimitSlotDictionary.Values)
            slot.OnBuy = IncreaseWaitressLimit;

        // Bind events to waitress speed slots OnBuy
        foreach (Slot slot in waitressSpeedSlotDictionary.Values)
            slot.OnBuy = IncreaseWaitressSpeed;

        if (!data.IsInitialized)
        {
            foreach (Slot slot in bookShelfSlotDictionary.Values)
                data.BookShelfSlotData.Add(slot.ID, new PlayerData.SlotData(slot.ID, slot.Price));
            foreach (Bookshelf bookshelf in bookShelfDictionary.Values)
                data.BookShelfData.Add(bookshelf.ID, new PlayerData.ItemHolderData(bookshelf.ID, 0));

            foreach (Slot slot in vendingMachineSlotDictionary.Values)
                data.VendingMachineSlotData.Add(slot.ID, new PlayerData.SlotData(slot.ID, slot.Price));
            foreach (CoffeeVendingMachine vendingMachine in vendingMachineDictionary.Values)
                data.VendingMachineData.Add(vendingMachine.ID, new PlayerData.ItemHolderData(vendingMachine.ID, 0));

            foreach (Slot slot in tableSlotDictionary.Values)
                data.TableSlotData.Add(slot.ID, new PlayerData.SlotData(slot.ID, slot.Price));

            foreach (Slot slot in expandSlotDictionary.Values)
                data.ExpandSlotData.Add(slot.ID, new PlayerData.SlotData(slot.ID, slot.Price));

            foreach (Slot slot in carrierHiringSlotDictionary.Values)
                data.CarrierHiringSlotData.Add(slot.ID, new PlayerData.SlotData(slot.ID, slot.Price));

            foreach (Slot slot in waitressHiringSlotDictionary.Values)
                data.WaitressHiringSlotData.Add(slot.ID, new PlayerData.SlotData(slot.ID, slot.Price));

            foreach (Slot slot in cashierHiringSlotDictionary.Values)
                data.CashierHiringSlotData.Add(slot.ID, new PlayerData.SlotData(slot.ID, slot.Price));

            foreach (Slot slot in playerLimitSlotDictionary.Values)
                data.PlayerLimitSlotData.Add(slot.ID, new PlayerData.SlotData(slot.ID, slot.Price));

            foreach (Slot slot in playerSpeedSlotDictionary.Values)
                data.PlayerSpeedSlotData.Add(slot.ID, new PlayerData.SlotData(slot.ID, slot.Price));

            foreach (Slot slot in carrierLimitSlotDictionary.Values)
                data.CarrierLimitSlotData.Add(slot.ID, new PlayerData.SlotData(slot.ID, slot.Price));

            foreach (Slot slot in carrierSpeedSlotDictionary.Values)
                data.CarrierSpeedSlotData.Add(slot.ID, new PlayerData.SlotData(slot.ID, slot.Price));

            foreach (Slot slot in waitressLimitSlotDictionary.Values)
                data.WaitressLimitSlotData.Add(slot.ID, new PlayerData.SlotData(slot.ID, slot.Price));

            foreach (Slot slot in waitressSpeedSlotDictionary.Values)
                data.WaitressSpeedSlotData.Add(slot.ID, new PlayerData.SlotData(slot.ID, slot.Price));
            SpawnBookshopMesh(0);
            data.IsInitialized = true;
        }
        else
        {
            // Spawn shop mesh
            //expandCount = data.BookshopLevel;
            SpawnBookshopMesh(data.BookshopLevel);
            onSlotActivity.Invoke();
        }

        // Set bookshelf slot data
        foreach (PlayerData.SlotData slotData in data.BookShelfSlotData.Values)
            SetBookShelfSlotData(slotData);

        // Set bookshelf data
        foreach (PlayerData.ItemHolderData itemHolderData in data.BookShelfData.Values)
            SetBookShelfData(itemHolderData);

        // Set vending machine slot data
        foreach (PlayerData.SlotData slotData in data.VendingMachineSlotData.Values)
            SetVendingMachineSlotData(slotData);

        // Set vending machine data
        foreach (PlayerData.ItemHolderData itemHolderData in data.VendingMachineData.Values)
            SetVendingMachineData(itemHolderData);

        // Set table slot data
        foreach (PlayerData.SlotData slotData in data.TableSlotData.Values)
            SetTableSlotData(slotData);

        // Set expand slot data
        foreach (PlayerData.SlotData slotData in data.ExpandSlotData.Values)
            SetExpandSlotData(slotData);

        // Set carrier hiring slot data
        foreach (PlayerData.SlotData slotData in data.CarrierHiringSlotData.Values)
            SetCarrierHiringSlotData(slotData);

        // Set waitress hiring slot data
        foreach (PlayerData.SlotData slotData in data.WaitressHiringSlotData.Values)
            SetWaitressHiringSlotData(slotData);

        // Set cashier hiring slot data
        foreach (PlayerData.SlotData slotData in data.CashierHiringSlotData.Values)
            SetCashierHiringSlotData(slotData);

        // Set player limit slot data
        foreach (PlayerData.SlotData slotData in data.PlayerLimitSlotData.Values)
            SetPlayerLimitSlotData(slotData);

        // Set player speed slot data
        foreach (PlayerData.SlotData slotData in data.PlayerSpeedSlotData.Values)
            SetPlayerSpeedSlotData(slotData);

        // Set carrier limit slot data
        foreach (PlayerData.SlotData slotData in data.CarrierLimitSlotData.Values)
            SetCarrierLimitSlotData(slotData);

        // Set carrier speed slot data
        foreach (PlayerData.SlotData slotData in data.CarrierSpeedSlotData.Values)
            SetCarrierSpeedSlotData(slotData);

        // Set waitress limit slot data
        foreach (PlayerData.SlotData slotData in data.WaitressLimitSlotData.Values)
            SetWaitressLimitSlotData(slotData);

        // Set waitress speed slot data
        foreach (PlayerData.SlotData slotData in data.WaitressSpeedSlotData.Values)
            SetWaitressSpeedSlotData(slotData);

        navMeshSurface.BuildNavMesh();
    }

    private void SpawnBookshopMesh(int level)
    {
        ClearMeshContainer();
        Instantiate(PrefabManager.Instance.GetPrefab("Bookshop Mesh " + level), meshContainer);
        navMeshSurface.BuildNavMesh();
    }

    private void ClearMeshContainer()
    {
        for (int i = 0; i < meshContainer.childCount; i++)
            Destroy(meshContainer.GetChild(0).gameObject);
    }

    private void SpawnBookShelf(Slot slot)
    {
        Bookshelf bookshelf = bookShelfDictionary[slot.ID];
        slot.Deactivate();
        bookshelf.Activate();
        incompleteBookshelfs.Add(bookshelf);
        boughtBookshelfs.Add(bookshelf);
        navMeshSurface.BuildNavMesh();
    }

    private void SetBookShelfSlotData(PlayerData.SlotData slotData)
    {
        Slot slot = bookShelfSlotDictionary[slotData.ID];
        slot.Data = slotData;
    }

    private void SetCarrierHiringSlotData(PlayerData.SlotData slotData)
    {
        Slot slot = carrierHiringSlotDictionary[slotData.ID];
        slot.Data = slotData;
    }

    private void SetWaitressHiringSlotData(PlayerData.SlotData slotData)
    {
        Slot slot = waitressHiringSlotDictionary[slotData.ID];
        slot.Data = slotData;
    }

    private void SetCashierHiringSlotData(PlayerData.SlotData slotData)
    {
        Slot slot = cashierHiringSlotDictionary[slotData.ID];
        slot.Data = slotData;
    }

    private void SetPlayerLimitSlotData(PlayerData.SlotData slotData)
    {
        Slot slot = playerLimitSlotDictionary[slotData.ID];
        slot.Data = slotData;
    }

    private void SetPlayerSpeedSlotData(PlayerData.SlotData slotData)
    {
        Slot slot = playerSpeedSlotDictionary[slotData.ID];
        slot.Data = slotData;
    }

    private void SetCarrierLimitSlotData(PlayerData.SlotData slotData)
    {
        Slot slot = carrierLimitSlotDictionary[slotData.ID];
        slot.Data = slotData;
    }

    private void SetCarrierSpeedSlotData(PlayerData.SlotData slotData)
    {
        Slot slot = carrierSpeedSlotDictionary[slotData.ID];
        slot.Data = slotData;
    }

    private void SetWaitressLimitSlotData(PlayerData.SlotData slotData)
    {
        Slot slot = waitressLimitSlotDictionary[slotData.ID];
        slot.Data = slotData;
    }

    private void SetWaitressSpeedSlotData(PlayerData.SlotData slotData)
    {
        Slot slot = waitressSpeedSlotDictionary[slotData.ID];
        slot.Data = slotData;
    }


    private void SetBookShelfData(PlayerData.ItemHolderData itemHolderData)
    {
        Bookshelf bookshelf = bookShelfDictionary[itemHolderData.ID];
        bookshelf.Data = itemHolderData;
    }

    private void SpawnVendingMachine(Slot slot)
    {
        CoffeeVendingMachine vendingMachine = vendingMachineDictionary[slot.ID];
        slot.Deactivate();
        vendingMachine.Activate();
        incompleteVendingMachines.Add(vendingMachine);
        navMeshSurface.BuildNavMesh();
    }

    private void SpawnTable(Slot slot)
    {
        Table table = tableDictionary[slot.ID];
        slot.Deactivate();
        table.Activate();
        avaliableTables.Add(table);
        table.OnLeave.Bind((args) => { avaliableTables.Add(table); });
        boughtTables.Add(table);
        navMeshSurface.BuildNavMesh();
    }

    private void HireCarrier(Slot slot)
    {
        CarrierAI carrier = carrierDictionary[slot.ID];
        slot.Deactivate();
        carrier.Activate();
    }

    private void HireWaitress(Slot slot)
    {
        CarrierAI waitress = waitressDictionary[slot.ID];
        slot.Deactivate();
        waitress.Activate();
    }

    private void HireCashier(Slot slot)
    {
        CashierAI cashier = cashierDictionary[slot.ID];
        slot.Deactivate();
        cashier.Activate();
    }

    private void IncreasePlayerLimit(Slot slot)
    {
        slot.Deactivate();
        onPlayerLimitSlotBuy.Invoke();
    }

    private void IncreasePlayerSpeed(Slot slot)
    {
        slot.Deactivate();
        onPlayerSpeedSlotBuy.Invoke();
    }

    private void IncreaseCarrierLimit(Slot slot)
    {
        slot.Deactivate();
        onCarrierLimitSlotBuy.Invoke();
    }

    private void IncreaseCarrierSpeed(Slot slot)
    {
        slot.Deactivate();
        onCarrierSpeedSlotBuy.Invoke();
    }

    private void IncreaseWaitressLimit(Slot slot)
    {
        slot.Deactivate();
        onWaitressLimitSlotBuy.Invoke();
    }

    private void IncreaseWaitressSpeed(Slot slot)
    {
        slot.Deactivate();
        onWaitressSpeedSlotBuy.Invoke();
    }

    private void SetVendingMachineSlotData(PlayerData.SlotData slotData)
    {
        Slot slot = vendingMachineSlotDictionary[slotData.ID];
        slot.Data = slotData;
    }

    private void SetVendingMachineData(PlayerData.ItemHolderData itemHolderData)
    {
        CoffeeVendingMachine vendingMachine = vendingMachineDictionary[itemHolderData.ID];
        vendingMachine.Data = itemHolderData;
    }

    private void SetTableSlotData(PlayerData.SlotData slotData)
    {
        Slot slot = tableSlotDictionary[slotData.ID];
        slot.Data = slotData;
    }

    private void SetExpandSlotData(PlayerData.SlotData slotData)
    {
        ExpandSlot slot = expandSlotDictionary[slotData.ID];
        slot.Data = slotData;
        //slot.IsInitialized = true;
    }

    private void Expand(Slot expandSlot = null)
    {
        if (expandSlot) expandSlot.Deactivate();
        PlayerData data = PlayerProgression.PlayerData;
        if (expandCount++ >= data.BookshopLevel)
        {
            data.BookshopLevel++;
            SpawnBookshopMesh(data.BookshopLevel);
        }
    }

    private List<T> FindChildrenWithTag<T>(string tag) where T : MonoBehaviour
    {
        List<T> result = new List<T>();
        List<Transform> transformsWithTag = FindChildrenTransformsWithTag(tag, _transform);
        for (int i = 0; i < transformsWithTag.Count; ++i)
            result.Add(transformsWithTag[i].GetComponent<T>());
        return result;
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

    #endregion

    private void Start()
    {
        StartCustomerSpawnCoroutine();
    }
    private void SpawnCustomer()
    {
        if (checkout.Limit <= customers.Count || checkout.IsFull) return;
        if (!(numberOfAvaliableBooks > 0 || avaliableTables.Count > 0)) return;
        if (customers.Count >= boughtTables.Count + boughtBookshelfs.Count) return;
        Customer customer = ObjectPooler.Instance.SpawnFromPool("Customer", customerSpawnPointTransform.position, customerSpawnPointTransform.rotation).GetComponent<Customer>();
        customers.Add(customer);
        customer.OnLeave.RemoveAllListeners();
        customer.OnLeave.AddListener(() => { customers.Remove(customer); });
        int maxNumberOfBooksToBuy = 5;
        /*customer.OnSit.Bind((args) =>
        {
            Table table = args[0] as Table;
            numberOfAvaliableTables--;
        });*/
        if (numberOfAvaliableBooks == 0)
        {
            // Buy only coffee
            customer.Aim = Customer.CustomerAim.COFFEE;
            customer.GoingToTable.Table = GetAvaliableTable();
            avaliableTables.Remove(customer.GoingToTable.Table);
            customer.ChangeState(customer.GoingToTable);
        }
        else if (avaliableTables.Count == 0)
        {
            // Buy only book
            customer.Aim = Customer.CustomerAim.BOOK;
            customer.DesiredNumberOfBooks = Mathf.Clamp(Random.Range(1, maxNumberOfBooksToBuy + 1), 1, numberOfAvaliableBooks);
            numberOfAvaliableBooks -= customer.DesiredNumberOfBooks;
            SendCustomerToBookshelf(customer);
        }
        else
        {
            // Buy book and coffee
            customer.Aim = Customer.CustomerAim.BOOK_AND_COFFEE;
            customer.GoingToTable.Table = GetAvaliableTable();
            avaliableTables.Remove(customer.GoingToTable.Table);
            customer.DesiredNumberOfBooks = Mathf.Clamp(Random.Range(1, maxNumberOfBooksToBuy + 1), 1, numberOfAvaliableBooks);
            numberOfAvaliableBooks -= customer.DesiredNumberOfBooks;
            SendCustomerToBookshelf(customer);
        }
    }

    public Bookshelf GetAvaliableBookshelf()
    {
        return avaliableBookshelfs[Random.Range(0, avaliableBookshelfs.Count)];
    }

    public Table GetAvaliableTable()
    {
        return avaliableTables[Random.Range(0, avaliableTables.Count)];
    }

    public void SendCustomerToBookshelf(Customer customer)
    {
        customer.GoingToBookshelf.Bookshelf = GetAvaliableBookshelf();
        customer.ChangeState(customer.GoingToBookshelf);
    }

    private void StartCustomerSpawnCoroutine()
    {
        InvokeRepeating("SpawnCustomer", 2, 2);
    }

    public void AddBoughtSlot(Slot slot)
    {
        boughtSlots.Add(slot);
        onSlotActivity.Invoke();
    }
}
