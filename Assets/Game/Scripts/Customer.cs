using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]

public class Customer : StateMachine, IPooledObject
{
    #region States
    private INACTIVE inactive;
    private GOING_TO_BOOKSHELF goingToBookshelf;
    private TAKING_BOOK takingBook;
    private GOING_TO_TABLE goingToTable;
    private WAITING_AT_TABLE waitingAtTable;
    private STAYING_AT_TABLE stayingAtTable;
    private GOING_TO_CHECKOUT goingToCheckout;
    private IN_CHECKOUT inCheckout;
    private LEAVING leaving;

    public GOING_TO_BOOKSHELF GoingToBookshelf { get => goingToBookshelf; }
    public TAKING_BOOK TakingBook { get => takingBook; }
    public GOING_TO_TABLE GoingToTable { get => goingToTable; }
    public WAITING_AT_TABLE WaitingAtTable { get => waitingAtTable; }
    public STAYING_AT_TABLE StayingAtTable { get => stayingAtTable; }
    public GOING_TO_CHECKOUT GoingToCheckout { get => goingToCheckout; }
    public IN_CHECKOUT InCheckout { get => inCheckout; }
    public LEAVING Leaving { get => leaving; }
    public INACTIVE Inactive { get => inactive; }

    #endregion

    [SerializeField] private ItemStack stack;
    [SerializeField] private EventDispatcher onTakeBook, onSit;
    [SerializeField] private CustomerCanvas customerCanvas;
    [SerializeField] private Animator animator;
    private UnityEvent onLeave = new UnityEvent();
    private Transform _transform;
    private NavMeshAgent agent;
    public int Bill = 0;
    public CustomerAim Aim = CustomerAim.BOOK_AND_COFFEE;
    public NavMeshAgent Agent { get => agent; }
    public ItemStack Stack { get => stack; }
    public int NumberOfBooks { get; set; }
    public int DesiredNumberOfBooks { get; set; }
    public Transform Transform { get => _transform; }
    public EventDispatcher OnTakeBook { get => onTakeBook; }
    public EventDispatcher OnSit { get => onSit; }
    public UnityEvent OnLeave { get => onLeave; }
    public Animator Animator { get => animator; }

    protected override void Awake()
    {
        base.Awake();
        state = inactive;
        _transform = transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        stack.OnAdd.AddListener((newItem) => { animator.SetInteger("ItemCount", stack.Count); });
        stack.OnRemove.AddListener(() => { animator.SetInteger("ItemCount", stack.Count); });
    }
    public int Limit { get => stack.Limit; set => stack.Limit = value; }
    private float cooldown { get => stack.PushPeriod; }
    private float lastPushTime = float.MinValue;
    private float lastDropTime = float.MinValue;
    public bool CanTakeItem { get => stack.CanPush; }

    public bool PushToStack(Stackable item)
    {
        if (CanTakeItem)
        {
            stack.Push(item);
            lastPushTime = Time.time;
            //onPush?.Call();
            return true;
        }
        return false;
    }
    protected override void InitializeStates()
    {
        inactive = new INACTIVE(this);
        goingToBookshelf = new GOING_TO_BOOKSHELF(this);
        takingBook = new TAKING_BOOK(this);
        goingToTable = new GOING_TO_TABLE(this);
        waitingAtTable = new WAITING_AT_TABLE(this);
        stayingAtTable = new STAYING_AT_TABLE(this);
        goingToCheckout = new GOING_TO_CHECKOUT(this);
        inCheckout = new IN_CHECKOUT(this);
        leaving = new LEAVING(this);
    }
    public void OnObjectSpawn()
    {
        NumberOfBooks = 0;
        state = inactive;
        Bill = 0;
        stack.Clear();
    }

    #region State Implementations

    public class GOING_TO_BOOKSHELF : State
    {
        public Bookshelf Bookshelf = null;
        private Customer customer;
        private bool hasReached = false;
        public GOING_TO_BOOKSHELF(Customer customer) : base(customer)
        {
            this.customer = customer;
        }

        public override bool CanEnter()
        {
            System.Type type = stateMachine.State.GetType();
            return customer.DesiredNumberOfBooks > 0 && Bookshelf && (type == typeof(INACTIVE) || type == typeof(GOING_TO_BOOKSHELF) || type == typeof(TAKING_BOOK));
        }

        public override void OnEnter()
        {
            customer.customerCanvas.ActivateExpression(CustomerCanvas.ExpressionType.BOOK);
            customer.Agent.SetDestination(Bookshelf.InteractionPointTransform.position);
            customer.Animator.SetBool("Running", true);
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
            if (Bookshelf.ItemHolder.NumberOfItems == 0)
                Bookshop.Instance.SendCustomerToBookshelf(customer);
            float dist = customer.Agent.remainingDistance;
            if (!hasReached && dist != Mathf.Infinity && Vector3.Distance(customer.Transform.position, Bookshelf.InteractionPointTransform.position) < 0.2f && customer.Agent.pathStatus == NavMeshPathStatus.PathComplete && customer.Agent.remainingDistance == 0)
                ReachToDestination();
        }

        private void ReachToDestination()
        {
            customer.takingBook.Bookshelf = Bookshelf;
            customer.ChangeState(customer.takingBook);
        }
    }

    public class TAKING_BOOK : State
    {
        public Bookshelf Bookshelf = null;
        private Customer customer;
        private float enterTime;
        private float castDuration = 0.2f;
        private bool tookBook = false;
        public TAKING_BOOK(Customer customer) : base(customer)
        {
            this.customer = customer;
        }

        public override bool CanEnter()
        {
            System.Type type = stateMachine.State.GetType();
            return Bookshelf && type == typeof(GOING_TO_BOOKSHELF);
        }

        public override void OnEnter()
        {
            enterTime = Time.time;
            customer.customerCanvas.ActivateExpression(CustomerCanvas.ExpressionType.BOOK);
            customer.Animator.SetBool("Running", false);
        }

        public override void OnExit()
        {
            Bookshelf = null;
            tookBook = false;
        }

        public override void OnTriggerEnter(Collider other)
        {
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void OnUpdate()
        {
            if (!tookBook && enterTime + castDuration <= Time.time && customer.CanTakeItem)
                TakeBook();
        }

        private void TakeBook()
        {
            if (!customer.CanTakeItem) return;
            tookBook = true;
            Stackable book = ObjectPooler.Instance.SpawnFromPool("Book", Bookshelf.transform.position, Quaternion.identity).GetComponent<Stackable>();
            customer.PushToStack(book);
            customer.Bill++;
            Bookshelf.ItemHolder.Remove();
            customer.NumberOfBooks++;
            customer.OnTakeBook.Call();
            if (customer.NumberOfBooks < customer.DesiredNumberOfBooks)
                Bookshop.Instance.SendCustomerToBookshelf(customer);
            else if (customer.Aim == CustomerAim.BOOK)
                customer.ChangeState(customer.GoingToCheckout);
            else if (customer.Aim == CustomerAim.BOOK_AND_COFFEE)
                customer.ChangeState(customer.GoingToTable);
        }
    }

    public class GOING_TO_TABLE : State
    {
        public Table Table = null;
        private Customer customer;
        private bool hasReached = false;
        public GOING_TO_TABLE(Customer customer) : base(customer)
        {
            this.customer = customer;
        }

        public override bool CanEnter()
        {
            System.Type type = stateMachine.State.GetType();
            return Table && (type == typeof(INACTIVE) || type == typeof(TAKING_BOOK));

        }

        public override void OnEnter()
        {
            customer.Agent.SetDestination(Table.InteractionPointTransform.position);
            customer.customerCanvas.ActivateExpression(CustomerCanvas.ExpressionType.TABLE);
            customer.Animator.SetBool("Running", true);
        }

        public override void OnExit()
        {
            Table = null;
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
            float dist = customer.Agent.remainingDistance;
            if (!hasReached && dist != Mathf.Infinity && Vector3.Distance(customer.Transform.position, Table.InteractionPointTransform.position) < 0.2f && customer.Agent.pathStatus == NavMeshPathStatus.PathComplete && customer.Agent.remainingDistance == 0)
                ReachToDestination();
        }

        private void ReachToDestination()
        {
            customer.WaitingAtTable.Table = Table;
            customer.ChangeState(customer.WaitingAtTable);
        }
    }

    public class WAITING_AT_TABLE : State
    {
        private Customer customer;
        public Table Table;
        public WAITING_AT_TABLE(Customer customer) : base(customer)
        {
            this.customer = customer;
        }

        public override bool CanEnter()
        {
            System.Type type = stateMachine.State.GetType();
            return Table && type == typeof(GOING_TO_TABLE);

        }

        public override void OnEnter()
        {
            Table.Customer = customer;
            Table.OnSit.Invoke();
            customer.OnSit.Call(Table);
            customer.customerCanvas.ActivateExpression(CustomerCanvas.ExpressionType.COFFEE);
            customer.Animator.SetBool("Running", false);
        }

        public override void OnExit()
        {
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
        }
    }

    public class STAYING_AT_TABLE : State
    {
        public Table Table;
        private Customer customer;
        public STAYING_AT_TABLE(Customer customer) : base(customer)
        {
            this.customer = customer;
        }

        public override bool CanEnter()
        {
            System.Type type = stateMachine.State.GetType();
            return Table && type == typeof(WAITING_AT_TABLE);
        }

        public override void OnEnter()
        {
            customer.Bill += 2;
            LeanTween.delayedCall(1, () => { customer.ChangeState(customer.goingToCheckout); });
            customer.customerCanvas.ActivateExpression(CustomerCanvas.ExpressionType.NONE);
            customer.Animator.SetBool("Running", false);
        }

        public override void OnExit()
        {
            Table.OnLeave.Call();
            Table.Customer = null;
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
        }
    }

    public class GOING_TO_CHECKOUT : State
    {
        private Customer customer;
        private Checkout checkout;
        private Transform destinationPoint;
        private bool hasReached = false;
        public GOING_TO_CHECKOUT(Customer customer) : base(customer)
        {
            this.customer = customer;
        }

        public override bool CanEnter()
        {
            System.Type type = stateMachine.State.GetType();
            return type == typeof(STAYING_AT_TABLE) || type == typeof(TAKING_BOOK) || type == typeof(IN_CHECKOUT) || type == typeof(GOING_TO_CHECKOUT);

        }

        public override void OnEnter()
        {
            checkout = Bookshop.Instance.Checkout;
            destinationPoint = checkout.Enqueue(customer);
            customer.Agent.SetDestination(destinationPoint.position);
            customer.customerCanvas.ActivateExpression(CustomerCanvas.ExpressionType.CHECKOUT);
            customer.Animator.SetBool("Running", true);
        }

        public override void OnExit()
        {
            destinationPoint = null;
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
            float dist = customer.Agent.remainingDistance;
            if (!hasReached && dist != Mathf.Infinity && Vector3.Distance(customer.Transform.position, destinationPoint.position) < 0.1f && customer.Agent.pathStatus == NavMeshPathStatus.PathComplete && customer.Agent.remainingDistance == 0)
                ReachToDestination();
        }

        private void ReachToDestination()
        {
            customer.ChangeState(customer.InCheckout);
        }


    }

    public class IN_CHECKOUT : State
    {
        private Customer customer;
        private Checkout checkout;
        public IN_CHECKOUT(Customer customer) : base(customer)
        {
            this.customer = customer;
        }

        public override bool CanEnter()
        {
            System.Type type = stateMachine.State.GetType();
            return type == typeof(GOING_TO_CHECKOUT);

        }

        public override void OnEnter()
        {
            checkout = Bookshop.Instance.Checkout;
            customer.customerCanvas.ActivateExpression(CustomerCanvas.ExpressionType.CHECKOUT);
            customer.Animator.SetBool("Running", false);
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

        public void Pay()
        {
            while (customer.Bill-- > 0)
                checkout.MoneyPile.AddMoney(customer.Transform.position + Vector3.up * 2);
            checkout.Dequeue(customer);
            customer.ChangeState(customer.Leaving);
        }
    }

    public class LEAVING : State
    {
        private Customer customer;
        private bool hasReached = false;
        public LEAVING(Customer customer) : base(customer)
        {
            this.customer = customer;
        }

        public override bool CanEnter()
        {
            System.Type type = stateMachine.State.GetType();
            return type == typeof(IN_CHECKOUT);

        }

        public override void OnEnter()
        {
            customer.Agent.SetDestination(Bookshop.Instance.CustomerLeavePointTransform.position);
            customer.customerCanvas.ActivateExpression(CustomerCanvas.ExpressionType.HAPPY);
            customer.Animator.SetBool("Running", true);
            customer.OnLeave.Invoke();
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
            float dist = customer.Agent.remainingDistance;
            if (!hasReached && dist != Mathf.Infinity && Vector3.Distance(customer.Transform.position, Bookshop.Instance.CustomerLeavePointTransform.position) < 0.2f && customer.Agent.pathStatus == NavMeshPathStatus.PathComplete && customer.Agent.remainingDistance == 0)
                ReachToDestination();
        }

        private void ReachToDestination()
        {
            customer.stack.Clear();
            customer.ChangeState(customer.Inactive);
        }
    }
    #endregion

    public enum CustomerAim { COFFEE, BOOK, BOOK_AND_COFFEE }
}
