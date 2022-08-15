using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using UnityEngine.AI;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Carrier))]
[RequireComponent(typeof(Rigidbody))]
public class CarrierAI : StateMachine, IPooledObject
{
    #region States
    private INACTIVE inactive;
    private WAITING waiting;
    private GOING_TO_SUPPLIER goingToSupplier;
    private TAKING_SUPPLY takingSupply;
    private GOING_TO_ITEM_HOLDER goingToItemHolder;
    private DROPPING_ITEM droppingItem;
    private GOING_TO_DUMP goingToDump;
    private DUMPING dumping;
    private RETURNING returning;

    public INACTIVE INACTIVE { get => inactive; }
    public WAITING Waiting { get => waiting; }
    public GOING_TO_SUPPLIER GoingToSupplier { get => goingToSupplier; }
    public TAKING_SUPPLY TakingSupply { get => takingSupply; }
    public GOING_TO_ITEM_HOLDER GoingToItemHolder { get => goingToItemHolder; }
    public DROPPING_ITEM DroppingItem { get => droppingItem; }
    public GOING_TO_DUMP GoingToDump { get => goingToDump; }
    public DUMPING Dumping { get => dumping; }
    public RETURNING Returning { get => returning; }

    #endregion

    private NavMeshAgent agent;
    private Carrier carrier;
    private Transform _transform;
    private ItemType currentItemType = ItemType.NULL;
    [SerializeField] private int id;
    [SerializeField] private bool isWaitress;
    [SerializeField] private Transform basePointTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private ItemStack itemStack;
    public NavMeshAgent Agent { get => agent; }
    public Transform Transform { get => _transform; }
    public Carrier Carrier { get => carrier; }
    public Transform BasePointTransform { get => basePointTransform; }
    public bool IsWaitress { get => isWaitress; }
    public int ID { get => id; }
    public Animator Animator { get => animator; }

    protected override void Awake()
    {
        base.Awake();
        _transform = transform;
        basePointTransform.parent = null;
        carrier = GetComponent<Carrier>();
        agent = GetComponent<NavMeshAgent>();
        state = inactive;
        Deactivate();
    }

    protected override void InitializeStates()
    {
        inactive = new INACTIVE(this);
        waiting = new WAITING(this);
        goingToSupplier = new GOING_TO_SUPPLIER(this);
        takingSupply = new TAKING_SUPPLY(this);
        goingToItemHolder = new GOING_TO_ITEM_HOLDER(this);
        droppingItem = new DROPPING_ITEM(this);
        goingToDump = new GOING_TO_DUMP(this);
        dumping = new DUMPING(this);
        returning = new RETURNING(this);
    }

    private void Start()
    {
        itemStack.OnAdd.AddListener((newItem) => { animator.SetInteger("ItemCount", itemStack.Count); });
        itemStack.OnRemove.AddListener(() => { animator.SetInteger("ItemCount", itemStack.Count); });
        ChangeState(waiting);
    }

    public void OnObjectSpawn()
    {
        state = inactive;
        ChangeState(waiting);
    }

    public void Activate()
    {
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public Table FindIncompleteTable()
    {
        if (Bookshop.Instance.IncompleteTables.Count > 0)
            return Bookshop.Instance.IncompleteTables.First();
        return null;
    }

    public Bookshelf FindIncompleteBookshelf()
    {
        if (Bookshop.Instance.IncompleteBookshelfs.Count > 0)
            return Bookshop.Instance.IncompleteBookshelfs.First();
        return null;
    }

    public CoffeeVendingMachine FindIncompleteVendingMachine()
    {
        if (Bookshop.Instance.IncompleteVendingMachines.Count > 0)
            return Bookshop.Instance.IncompleteVendingMachines.First();
        return null;
    }

    #region State Implementations

    public class WAITING : State
    {
        private CarrierAI carrierAI = null;
        public WAITING(CarrierAI stateMachine) : base(stateMachine)
        {
            carrierAI = stateMachine;
        }

        public override bool CanEnter()
        {
            return stateMachine.State.GetType() == typeof(INACTIVE) ||
                stateMachine.State.GetType() == typeof(RETURNING);
        }

        public override void OnEnter()
        {
            carrierAI.Animator.SetBool("Running", false);
        }

        public override void OnExit()
        {
        }

        public override void OnTriggerEnter(Collider other)
        {
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void OnUpdate()
        {
            if (!carrierAI.IsWaitress)
            {
                if (Random.value < 0.5f)
                {
                    Bookshelf bookshelf = carrierAI.FindIncompleteBookshelf();
                    if (bookshelf)
                    {
                        carrierAI.GoingToItemHolder.Bookshelf = bookshelf;
                        carrierAI.Carrier.TakingItemType = Carrier.TakeType.BOOK;
                        carrierAI.goingToSupplier.Supplier = Bookshop.Instance.BookSuppliers[0];
                        carrierAI.ChangeState(carrierAI.GoingToSupplier);
                    }
                    else
                    {
                        CoffeeVendingMachine vendingMachine = carrierAI.FindIncompleteVendingMachine();
                        if (vendingMachine)
                        {
                            carrierAI.GoingToItemHolder.VendingMachine = vendingMachine;
                            carrierAI.Carrier.TakingItemType = Carrier.TakeType.CUP;
                            carrierAI.goingToSupplier.Supplier = Bookshop.Instance.CupSuppliers[0];
                            carrierAI.ChangeState(carrierAI.GoingToSupplier);
                        }
                    }
                }
                else
                {
                    CoffeeVendingMachine vendingMachine = carrierAI.FindIncompleteVendingMachine();
                    if (vendingMachine)
                    {
                        carrierAI.GoingToItemHolder.VendingMachine = vendingMachine;
                        carrierAI.Carrier.TakingItemType = Carrier.TakeType.CUP;
                        carrierAI.goingToSupplier.Supplier = Bookshop.Instance.CupSuppliers[0];
                        carrierAI.ChangeState(carrierAI.GoingToSupplier);
                    }
                    else
                    {
                        Bookshelf bookshelf = carrierAI.FindIncompleteBookshelf();
                        if (bookshelf)
                        {
                            carrierAI.GoingToItemHolder.Bookshelf = bookshelf;
                            carrierAI.Carrier.TakingItemType = Carrier.TakeType.BOOK;
                            carrierAI.goingToSupplier.Supplier = Bookshop.Instance.BookSuppliers[0];
                            carrierAI.ChangeState(carrierAI.GoingToSupplier);
                        }
                    }
                }
            }
            else
            {
                Table table = carrierAI.FindIncompleteTable();
                if (table)
                {
                    carrierAI.GoingToItemHolder.Table = table;
                    List<Supplier> coffeeSuppliers = Bookshop.Instance.CoffeeSuppliers.Where((coffeeSupplier) => coffeeSupplier.gameObject.activeSelf).ToList();
                    Supplier minDistanceSupplier = coffeeSuppliers[0];
                    float minDistance = Vector3.SqrMagnitude(carrierAI.Transform.position - minDistanceSupplier.Transform.position);
                    for (int i = 1; i < coffeeSuppliers.Count; i++)
                    {
                        Supplier supplier = coffeeSuppliers[i];
                        float distance = Vector3.SqrMagnitude(carrierAI.Transform.position - supplier.Transform.position);
                        if (distance < minDistance)
                            minDistanceSupplier = supplier;
                    }
                    carrierAI.carrier.TakingItemType = Carrier.TakeType.COFFEE;
                    carrierAI.goingToSupplier.Supplier = minDistanceSupplier;
                    carrierAI.ChangeState(carrierAI.GoingToSupplier);
                }
            }
        }
    }
    public class GOING_TO_SUPPLIER : State
    {
        private CarrierAI carrierAI;
        public Supplier Supplier;
        private bool hasReached = false;
        public GOING_TO_SUPPLIER(CarrierAI stateMachine) : base(stateMachine)
        {
            carrierAI = stateMachine;
        }

        public override bool CanEnter()
        {
            System.Type type = stateMachine.State.GetType();
            return Supplier != null && (type == typeof(WAITING));
        }

        public override void OnEnter()
        {
            carrierAI.Agent.SetDestination(Supplier.InteractionPointTransform.position);
            carrierAI.Animator.SetBool("Running", true);
        }

        public override void OnExit()
        {
            hasReached = false;
            Supplier = null;
        }

        public override void OnTriggerEnter(Collider other)
        {
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void OnUpdate()
        {
            float dist = carrierAI.Agent.remainingDistance;
            if (!hasReached && dist != Mathf.Infinity && Vector3.Distance(carrierAI.Transform.position, Supplier.InteractionPointTransform.position) < 0.2f && carrierAI.Agent.pathStatus == NavMeshPathStatus.PathComplete && carrierAI.Agent.remainingDistance == 0)
                ReachToDestination();
        }

        private void ReachToDestination()
        {
            hasReached = true;
            carrierAI.ChangeState(carrierAI.TakingSupply);
        }
    }
    public class TAKING_SUPPLY : State
    {
        private CarrierAI carrierAI;
        public TAKING_SUPPLY(CarrierAI stateMachine) : base(stateMachine)
        {
            carrierAI = stateMachine;
        }

        public override bool CanEnter()
        {
            return stateMachine.State.GetType() == typeof(GOING_TO_SUPPLIER);
        }

        public override void OnEnter()
        {
            carrierAI.Animator.SetBool("Running", false);
        }

        public override void OnExit()
        {
        }

        public override void OnTriggerEnter(Collider other)
        {
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void OnUpdate()
        {
            if (carrierAI.Carrier.IsFull)
                carrierAI.ChangeState(carrierAI.GoingToItemHolder);
        }
    }
    public class GOING_TO_ITEM_HOLDER : State
    {
        public Bookshelf Bookshelf;
        public CoffeeVendingMachine VendingMachine;
        public Table Table;
        private Transform destinationPointTransform;
        private CarrierAI carrierAI;
        private bool hasReached = false;
        public GOING_TO_ITEM_HOLDER(CarrierAI stateMachine) : base(stateMachine)
        {
            carrierAI = stateMachine;
        }

        public override bool CanEnter()
        {
            return (Bookshelf != null || VendingMachine != null || Table != null) && (stateMachine.State.GetType() == typeof(WAITING) ||
                stateMachine.State.GetType() == typeof(TAKING_SUPPLY) ||
                stateMachine.State.GetType() == typeof(DROPPING_ITEM) ||
                stateMachine.State.GetType() == typeof(GOING_TO_ITEM_HOLDER));
        }

        public override void OnEnter()
        {
            if (!carrierAI.IsWaitress)
                destinationPointTransform = Bookshelf ? Bookshelf.InteractionPointTransform : VendingMachine.InteractionPointTransform;
            else
                destinationPointTransform = Table.InteractionPointTransform;
            carrierAI.Agent.SetDestination(destinationPointTransform.position);
            carrierAI.Animator.SetBool("Running", true);
        }

        public override void OnExit()
        {
            hasReached = false;
            Bookshelf = null;
            VendingMachine = null;
            Table = null;
            destinationPointTransform = null;
        }

        public override void OnTriggerEnter(Collider other)
        {
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void OnUpdate()
        {
            if (carrierAI.Carrier.IsEmpty)
            {
                carrierAI.ChangeState(carrierAI.Returning);
                return;
            }
            if (Bookshelf && Bookshelf.ItemHolder.IsFull)
            {
                Bookshelf bookshelf = carrierAI.FindIncompleteBookshelf();
                if (bookshelf)
                {
                    Bookshelf = bookshelf;
                    OnEnter();
                    return;
                }
                else
                {
                    carrierAI.ChangeState(carrierAI.GoingToDump);
                }
            }
            else if (VendingMachine && VendingMachine.ItemHolder.IsFull)
            {
                CoffeeVendingMachine vendingMachine = carrierAI.FindIncompleteVendingMachine();
                if (vendingMachine)
                {
                    VendingMachine = vendingMachine;
                    OnEnter();
                    return;
                }
                else
                {
                    carrierAI.ChangeState(carrierAI.GoingToDump);
                }
            }
            else if (Table && Table.IsServed)
            {
                Table table = carrierAI.FindIncompleteTable();
                if (table)
                {
                    Table = table;
                    OnEnter();
                    return;
                }
                else
                {
                    carrierAI.ChangeState(carrierAI.Returning);
                }
            }
            else
            {
                float dist = carrierAI.Agent.remainingDistance;
                if (!hasReached && dist != Mathf.Infinity && Vector3.Distance(carrierAI.Transform.position, destinationPointTransform.position) < 0.2f && carrierAI.Agent.pathStatus == NavMeshPathStatus.PathComplete && carrierAI.Agent.remainingDistance == 0)
                    ReachToDestination();
            }

        }

        private void ReachToDestination()
        {
            hasReached = true;
            carrierAI.DroppingItem.Bookshelf = Bookshelf;
            carrierAI.DroppingItem.Table = Table;
            carrierAI.DroppingItem.VendingMachine = VendingMachine;
            carrierAI.ChangeState(carrierAI.DroppingItem);
        }
    }
    public class DROPPING_ITEM : State
    {
        public Bookshelf Bookshelf;
        public CoffeeVendingMachine VendingMachine;
        public Table Table;
        private CarrierAI carrierAI;
        public DROPPING_ITEM(CarrierAI stateMachine) : base(stateMachine)
        {
            carrierAI = stateMachine;
        }

        public override bool CanEnter()
        {
            return stateMachine.State.GetType() == typeof(GOING_TO_ITEM_HOLDER);
        }

        public override void OnEnter()
        {
            carrierAI.Animator.SetBool("Running", false);
        }

        public override void OnExit()
        {
            Bookshelf = null;
            VendingMachine = null;
            Table = null;
        }

        public override void OnTriggerEnter(Collider other)
        {
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void OnUpdate()
        {
            if (carrierAI.Carrier.IsEmpty)
            {
                carrierAI.ChangeState(carrierAI.Returning);
                return;
            }
            if (Bookshelf && Bookshelf.ItemHolder.IsFull)
            {

                Bookshelf bookshelf = carrierAI.FindIncompleteBookshelf();
                if (bookshelf)
                {
                    carrierAI.GoingToItemHolder.Bookshelf = bookshelf;
                    carrierAI.ChangeState(carrierAI.GoingToItemHolder);
                    return;
                }
                else
                {
                    carrierAI.ChangeState(carrierAI.GoingToDump);
                }
            }
            else if (VendingMachine && VendingMachine.ItemHolder.IsFull)
            {
                CoffeeVendingMachine vendingMachine = carrierAI.FindIncompleteVendingMachine();
                if (vendingMachine)
                {
                    carrierAI.GoingToItemHolder.VendingMachine = vendingMachine;
                    carrierAI.ChangeState(carrierAI.GoingToItemHolder);
                    return;
                }
                else
                {
                    carrierAI.ChangeState(carrierAI.GoingToDump);
                }
            }
            else if (Table && Table.IsServed)
            {
                Table table = carrierAI.FindIncompleteTable();
                if (table)
                {
                    carrierAI.GoingToItemHolder.Table = table;
                    carrierAI.ChangeState(carrierAI.GoingToItemHolder);
                    return;
                }
                else
                {
                    carrierAI.ChangeState(carrierAI.Returning);
                }
            }
        }
    }
    public class GOING_TO_DUMP : State
    {
        private CarrierAI carrierAI;
        private bool hasReached = false;
        public GOING_TO_DUMP(CarrierAI stateMachine) : base(stateMachine)
        {
            carrierAI = stateMachine;
        }

        public override bool CanEnter()
        {
            return stateMachine.State.GetType() == typeof(GOING_TO_ITEM_HOLDER) ||
                stateMachine.State.GetType() == typeof(DROPPING_ITEM);
        }

        public override void OnEnter()
        {
            carrierAI.carrier.TakingItemType = Carrier.TakeType.NONE;
            carrierAI.Agent.SetDestination(Bookshop.Instance.Dump.InteractionPointTransform.position);
            carrierAI.Animator.SetBool("Running", true);
        }

        public override void OnExit()
        {
            hasReached = false;
        }

        public override void OnTriggerEnter(Collider other)
        {
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void OnUpdate()
        {
            float dist = carrierAI.Agent.remainingDistance;
            if (!hasReached && dist != Mathf.Infinity && Vector3.Distance(carrierAI.Transform.position, Bookshop.Instance.Dump.InteractionPointTransform.position) < 0.2f && carrierAI.Agent.pathStatus == NavMeshPathStatus.PathComplete && carrierAI.Agent.remainingDistance == 0)
                ReachToDestination();
        }

        private void ReachToDestination()
        {
            hasReached = true;
            carrierAI.ChangeState(carrierAI.Dumping);
        }
    }

    public class DUMPING : State
    {
        private CarrierAI carrierAI;
        public DUMPING(CarrierAI stateMachine) : base(stateMachine)
        {
            carrierAI = stateMachine;
        }

        public override bool CanEnter()
        {
            return stateMachine.State.GetType() == typeof(GOING_TO_DUMP);
        }

        public override void OnEnter()
        {
            carrierAI.Animator.SetBool("Running", false);
        }

        public override void OnExit()
        {
        }

        public override void OnTriggerEnter(Collider other)
        {
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void OnUpdate()
        {
            if (carrierAI.Carrier.IsEmpty)
                carrierAI.ChangeState(carrierAI.Returning);
        }

    }
    public class RETURNING : State
    {
        private CarrierAI carrierAI;
        private bool hasReached = false;
        public RETURNING(CarrierAI stateMachine) : base(stateMachine)
        {
            carrierAI = stateMachine;
        }

        public override bool CanEnter()
        {
            return stateMachine.State.GetType() == typeof(DROPPING_ITEM) ||
                stateMachine.State.GetType() == typeof(GOING_TO_ITEM_HOLDER) ||
                stateMachine.State.GetType() == typeof(DUMPING);
        }

        public override void OnEnter()
        {
            carrierAI.carrier.TakingItemType = Carrier.TakeType.NONE;
            carrierAI.Agent.SetDestination(carrierAI.BasePointTransform.position);
            carrierAI.Animator.SetBool("Running", true);
        }

        public override void OnExit()
        {
            hasReached = false;
        }

        public override void OnTriggerEnter(Collider other)
        {
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void OnUpdate()
        {
            float dist = carrierAI.Agent.remainingDistance;
            if (!hasReached && dist != Mathf.Infinity && Vector3.Distance(carrierAI.Transform.position, carrierAI.BasePointTransform.position) < 0.2f && carrierAI.Agent.pathStatus == NavMeshPathStatus.PathComplete && carrierAI.Agent.remainingDistance == 0)
                ReachToDestination();
        }

        private void ReachToDestination()
        {
            hasReached = true;
            carrierAI.ChangeState(carrierAI.Waiting);
        }
    }
    #endregion
}
