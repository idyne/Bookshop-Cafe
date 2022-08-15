using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using UnityEngine.AI;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Carrier))]
[RequireComponent(typeof(Rigidbody))]
public class WaiterAI : StateMachine, IPooledObject
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
    [SerializeField] private Transform basePointTransform;
    public NavMeshAgent Agent { get => agent; }
    public Transform Transform { get => _transform; }
    public Carrier Carrier { get => carrier; }
    public Transform BasePointTransform { get => basePointTransform; }

    protected override void Awake()
    {
        base.Awake();
        _transform = transform;
        basePointTransform.parent = null;
        carrier = GetComponent<Carrier>();
        agent = GetComponent<NavMeshAgent>();
        state = inactive;
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
        ChangeState(waiting);
    }

    public void OnObjectSpawn()
    {
        state = inactive;
        ChangeState(waiting);
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
        private WaiterAI waiterAI = null;
        public WAITING(WaiterAI stateMachine) : base(stateMachine)
        {
            waiterAI = stateMachine;
        }

        public override bool CanEnter()
        {
            return stateMachine.State.GetType() == typeof(INACTIVE) ||
                stateMachine.State.GetType() == typeof(RETURNING);
        }

        public override void OnEnter()
        {
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
            if (Random.value < 0.5f)
            {
                Bookshelf bookshelf = waiterAI.FindIncompleteBookshelf();
                if (bookshelf)
                {
                    waiterAI.GoingToItemHolder.Bookshelf = bookshelf;
                    waiterAI.goingToSupplier.Supplier = Bookshop.Instance.BookSuppliers[0];
                    waiterAI.ChangeState(waiterAI.GoingToSupplier);
                }
                else
                {
                    CoffeeVendingMachine vendingMachine = waiterAI.FindIncompleteVendingMachine();
                    if (vendingMachine)
                    {
                        waiterAI.GoingToItemHolder.VendingMachine = vendingMachine;
                        waiterAI.goingToSupplier.Supplier = Bookshop.Instance.CupSuppliers[0];
                        waiterAI.ChangeState(waiterAI.GoingToSupplier);
                    }
                }
            }
            else
            {
                CoffeeVendingMachine vendingMachine = waiterAI.FindIncompleteVendingMachine();
                if (vendingMachine)
                {
                    waiterAI.GoingToItemHolder.VendingMachine = vendingMachine;
                    waiterAI.goingToSupplier.Supplier = Bookshop.Instance.CupSuppliers[0];
                    waiterAI.ChangeState(waiterAI.GoingToSupplier);
                }
                else
                {
                    Bookshelf bookshelf = waiterAI.FindIncompleteBookshelf();
                    if (bookshelf)
                    {
                        waiterAI.GoingToItemHolder.Bookshelf = bookshelf;
                        waiterAI.goingToSupplier.Supplier = Bookshop.Instance.BookSuppliers[0];
                        waiterAI.ChangeState(waiterAI.GoingToSupplier);
                    }
                }
            }
        }
    }
    public class GOING_TO_SUPPLIER : State
    {
        private WaiterAI waiterAI;
        public Supplier Supplier;
        private bool hasReached = false;
        public GOING_TO_SUPPLIER(WaiterAI stateMachine) : base(stateMachine)
        {
            waiterAI = stateMachine;
        }

        public override bool CanEnter()
        {
            System.Type type = stateMachine.State.GetType();
            return Supplier != null && (type == typeof(WAITING));
        }

        public override void OnEnter()
        {
            waiterAI.Agent.SetDestination(Supplier.InteractionPointTransform.position);
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
            float dist = waiterAI.Agent.remainingDistance;
            if (!hasReached && dist != Mathf.Infinity && Vector3.Distance(waiterAI.Transform.position, Supplier.InteractionPointTransform.position) < 0.2f && waiterAI.Agent.pathStatus == NavMeshPathStatus.PathComplete && waiterAI.Agent.remainingDistance == 0)
                ReachToDestination();
        }

        private void ReachToDestination()
        {
            hasReached = true;
            waiterAI.ChangeState(waiterAI.TakingSupply);
        }
    }
    public class TAKING_SUPPLY : State
    {
        private WaiterAI waiterAI;
        public TAKING_SUPPLY(WaiterAI stateMachine) : base(stateMachine)
        {
            waiterAI = stateMachine;
        }

        public override bool CanEnter()
        {
            return stateMachine.State.GetType() == typeof(GOING_TO_SUPPLIER);
        }

        public override void OnEnter()
        {
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
            if (waiterAI.Carrier.IsFull)
                waiterAI.ChangeState(waiterAI.GoingToItemHolder);
        }
    }
    public class GOING_TO_ITEM_HOLDER : State
    {
        public Bookshelf Bookshelf;
        public CoffeeVendingMachine VendingMachine;
        private Transform destinationPointTransform;
        private WaiterAI waiterAI;
        private bool hasReached = false;
        public GOING_TO_ITEM_HOLDER(WaiterAI stateMachine) : base(stateMachine)
        {
            waiterAI = stateMachine;
        }

        public override bool CanEnter()
        {
            return (Bookshelf != null || VendingMachine != null) && (stateMachine.State.GetType() == typeof(WAITING) ||
                stateMachine.State.GetType() == typeof(TAKING_SUPPLY) ||
                stateMachine.State.GetType() == typeof(GOING_TO_ITEM_HOLDER));
        }

        public override void OnEnter()
        {
            destinationPointTransform = Bookshelf ? Bookshelf.InteractionPointTransform : VendingMachine.InteractionPointTransform;
            waiterAI.Agent.SetDestination(destinationPointTransform.position);
        }

        public override void OnExit()
        {
            hasReached = false;
            Bookshelf = null;
            VendingMachine = null;
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
            if (waiterAI.Carrier.IsEmpty)
            {
                waiterAI.ChangeState(waiterAI.Returning);
                return;
            }
            if (Bookshelf && Bookshelf.ItemHolder.IsFull)
            {
                Bookshelf bookshelf = waiterAI.FindIncompleteBookshelf();
                if (bookshelf)
                {
                    Bookshelf = bookshelf;
                    OnEnter();
                    return;
                }
                else
                {
                    waiterAI.ChangeState(waiterAI.GoingToDump);
                }
            }
            else if (VendingMachine && VendingMachine.ItemHolder.IsFull)
            {
                CoffeeVendingMachine vendingMachine = waiterAI.FindIncompleteVendingMachine();
                if (vendingMachine)
                {
                    VendingMachine = vendingMachine;
                    OnEnter();
                    return;
                }
                else
                {
                    waiterAI.ChangeState(waiterAI.GoingToDump);
                }
            }
            else
            {
                float dist = waiterAI.Agent.remainingDistance;
                if (!hasReached && dist != Mathf.Infinity && Vector3.Distance(waiterAI.Transform.position, destinationPointTransform.position) < 0.2f && waiterAI.Agent.pathStatus == NavMeshPathStatus.PathComplete && waiterAI.Agent.remainingDistance == 0)
                    ReachToDestination();
            }

        }

        private void ReachToDestination()
        {
            hasReached = true;
            waiterAI.ChangeState(waiterAI.DroppingItem);
        }
    }
    public class DROPPING_ITEM : State
    {
        public DROPPING_ITEM(WaiterAI stateMachine) : base(stateMachine)
        {
        }

        public override bool CanEnter()
        {
            return stateMachine.State.GetType() == typeof(GOING_TO_ITEM_HOLDER);
        }

        public override void OnEnter()
        {
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
        }
    }
    public class GOING_TO_DUMP : State
    {
        private WaiterAI waiterAI;
        private bool hasReached = false;
        public GOING_TO_DUMP(WaiterAI stateMachine) : base(stateMachine)
        {
            waiterAI = stateMachine;
        }

        public override bool CanEnter()
        {
            return stateMachine.State.GetType() == typeof(GOING_TO_ITEM_HOLDER) ||
                stateMachine.State.GetType() == typeof(DROPPING_ITEM);
        }

        public override void OnEnter()
        {
            waiterAI.Agent.SetDestination(Bookshop.Instance.Dump.InteractionPointTransform.position);
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
            float dist = waiterAI.Agent.remainingDistance;
            if (!hasReached && dist != Mathf.Infinity && Vector3.Distance(waiterAI.Transform.position, Bookshop.Instance.Dump.InteractionPointTransform.position) < 0.2f && waiterAI.Agent.pathStatus == NavMeshPathStatus.PathComplete && waiterAI.Agent.remainingDistance == 0)
                ReachToDestination();
        }

        private void ReachToDestination()
        {
            hasReached = true;
            waiterAI.ChangeState(waiterAI.Dumping);
        }
    }

    public class DUMPING : State
    {
        private WaiterAI waiterAI;
        public DUMPING(WaiterAI stateMachine) : base(stateMachine)
        {
            waiterAI = stateMachine;
        }

        public override bool CanEnter()
        {
            return stateMachine.State.GetType() == typeof(GOING_TO_DUMP);
        }

        public override void OnEnter()
        {
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
            if (waiterAI.Carrier.IsEmpty)
                waiterAI.ChangeState(waiterAI.Returning);
        }

    }
    public class RETURNING : State
    {
        private WaiterAI waiterAI;
        private bool hasReached = false;
        public RETURNING(WaiterAI stateMachine) : base(stateMachine)
        {
            waiterAI = stateMachine;
        }

        public override bool CanEnter()
        {
            return stateMachine.State.GetType() == typeof(DROPPING_ITEM) ||
                stateMachine.State.GetType() == typeof(GOING_TO_ITEM_HOLDER) ||
                stateMachine.State.GetType() == typeof(DUMPING);
        }

        public override void OnEnter()
        {
            waiterAI.Agent.SetDestination(waiterAI.BasePointTransform.position);
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
            float dist = waiterAI.Agent.remainingDistance;
            if (!hasReached && dist != Mathf.Infinity && Vector3.Distance(waiterAI.Transform.position, waiterAI.BasePointTransform.position) < 0.2f && waiterAI.Agent.pathStatus == NavMeshPathStatus.PathComplete && waiterAI.Agent.remainingDistance == 0)
                ReachToDestination();
        }

        private void ReachToDestination()
        {
            hasReached = true;
            waiterAI.ChangeState(waiterAI.Waiting);
        }
    }
    #endregion
}
