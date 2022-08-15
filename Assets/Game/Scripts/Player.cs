using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
using DG.Tweening;

[RequireComponent(typeof(TopDownCharacterMovement))]
public class Player : MonoBehaviour
{
    private Transform _transform;
    [SerializeField] private ItemStack itemStack;
    private Carrier carrier;
    private TopDownCharacterMovement movement;
    private List<Slot> overlappedSlots = new List<Slot>();
    private List<MoneyPile> overlappedMoneyPiles = new List<MoneyPile>();
    [SerializeField] private Animator animator;
    [SerializeField] private AnimationCurve curve;

    public TopDownCharacterMovement Movement { get => movement; }
    public Carrier Carrier { get => carrier; }

    private void Awake()
    {
        _transform = transform;
        itemStack = GetComponentInChildren<ItemStack>();
        carrier = GetComponent<Carrier>();
        movement = GetComponent<TopDownCharacterMovement>();
        carrier.OnPush.Bind((args) => { HapticManager.DoHaptic(); });
        carrier.OnDrop.AddListener(() => { HapticManager.DoHaptic(); });

    }

    private void Start()
    {
        itemStack.OnAdd.AddListener((newItem) => { animator.SetInteger("ItemCount", itemStack.Count); });
        itemStack.OnRemove.AddListener(() => { animator.SetInteger("ItemCount", itemStack.Count); });
    }

    private void Update()
    {
        for (int i = 0; i < overlappedSlots.Count; i++)
            if (!overlappedSlots[i].gameObject.activeSelf) overlappedSlots.RemoveAt(i);
        for (int i = 0; i < overlappedMoneyPiles.Count; i++)
            if (!overlappedMoneyPiles[i].gameObject.activeSelf) overlappedMoneyPiles.RemoveAt(i);
        if (movement.Velocity <= 0.1f)
            AddMoneyToOverlappedSlot();
        CollectMoney();
    }

    private void AddMoneyToOverlappedSlot()
    {
        Slot closestSlot = FindClosest(overlappedSlots);
        if (closestSlot)
            AddMoneyToSlot(closestSlot);
    }

    private void CollectMoney()
    {
        if (overlappedMoneyPiles.Count == 0) return;
        int moneyChange = overlappedMoneyPiles[0].Collect(out Transform money);
        if (moneyChange > 0)
        {
            PlayerProgression.MONEY += moneyChange;
            if (money)
            {
                Vector3 start = money.position;
                DOTween.To((val) =>
                {
                    Vector3 end = _transform.position + Vector3.up * 2 + _transform.forward;
                    Vector3 pos = Vector3.Lerp(start, end, val);
                    pos.y += curve.Evaluate(val);
                    money.position = pos;
                }, 0, 1, 0.2f).OnComplete(() =>
                {
                    money.gameObject.SetActive(false);
                });
                money.parent = _transform;
            }
            HapticManager.DoHaptic();
        }
    }

    private T FindClosest<T>(List<T> collection) where T : MonoBehaviour
    {
        if (collection.Count == 0) return null;
        T closestItem = collection[0];
        float closestDistance = Vector3.SqrMagnitude(_transform.position - closestItem.transform.position);

        for (int i = 0; i < collection.Count; i++)
        {
            T item = collection[0];
            float distance = Vector3.SqrMagnitude(_transform.position - closestItem.transform.position);
            if (distance < closestDistance)
                closestItem = item;
        }
        return closestItem;
    }
    private void AddMoneyToSlot(Slot slot)
    {
        int deltaPrice = Mathf.Clamp(Mathf.Clamp(Mathf.CeilToInt(Time.deltaTime * slot.Price / 2), 0, slot.Price), 0, PlayerProgression.MONEY);
        if (deltaPrice > 0)
        {
            Transform money = ObjectPooler.Instance.SpawnFromPool("Money", _transform.position + Vector3.up * 2, Quaternion.identity).transform;
            money.LeanMove(slot.Transform.position + new Vector3(Random.Range(-0.4f, 0.4f), 0, Random.Range(-0.4f, 0.4f)), 0.2f).setOnComplete(() => { money.gameObject.SetActive(false); });
            PlayerProgression.MONEY -= deltaPrice;
            slot.Price -= deltaPrice;
            HapticManager.DoHaptic(amplitude: 0.005f, frequency: 0.005f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Book Shelf Slot") ||
            other.CompareTag("Coffee Vending Machine Slot") ||
            other.CompareTag("Table Slot") ||
            other.CompareTag("Expand Slot") ||
            other.CompareTag("Carrier Hiring Slot") ||
            other.CompareTag("Waitress Hiring Slot") ||
            other.CompareTag("Cashier Hiring Slot") ||
            other.CompareTag("Player Limit Slot") ||
            other.CompareTag("Player Speed Slot") ||
            other.CompareTag("Carrier Limit Slot") ||
            other.CompareTag("Carrier Speed Slot") ||
            other.CompareTag("Waitress Limit Slot") ||
            other.CompareTag("Waitress Speed Slot"))
            overlappedSlots.Add(other.GetComponent<Slot>());
        else if (other.CompareTag("Money Pile"))
            overlappedMoneyPiles.Add(other.GetComponent<MoneyPile>());
        else if (other.CompareTag("Interaction Plane"))
        {
            other.transform.localScale = Vector3.one;
            other.gameObject.LeanScale(Vector3.one * 1.1f, 0.1f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Book Shelf Slot") ||
            other.CompareTag("Coffee Vending Machine Slot") ||
            other.CompareTag("Table Slot") ||
            other.CompareTag("Expand Slot") ||
            other.CompareTag("Carrier Hiring Slot") ||
            other.CompareTag("Waitress Hiring Slot") ||
            other.CompareTag("Cashier Hiring Slot") ||
            other.CompareTag("Player Limit Slot") ||
            other.CompareTag("Player Speed Slot") ||
            other.CompareTag("Carrier Limit Slot") ||
            other.CompareTag("Carrier Speed Slot") ||
            other.CompareTag("Waitress Limit Slot") ||
            other.CompareTag("Waitress Speed Slot"))
            overlappedSlots.Remove(other.GetComponent<Slot>());
        else if (other.CompareTag("Money Pile"))
            overlappedMoneyPiles.Remove(other.GetComponent<MoneyPile>());
        else if (other.CompareTag("Interaction Plane"))
        {
            other.transform.localScale = Vector3.one * 1.1f;
            other.gameObject.LeanScale(Vector3.one, 0.1f);
        }
    }


}
