using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;

public class MoneyPile : MonoBehaviour
{
    [SerializeField] private int moneyMultiplier = 10;
    private List<Transform> moneyTransforms;
    private List<Transform> moneys = new List<Transform>();
    private Transform _transform;
    private int numberOfMoneys = 0;

    public int NumberOfMoneys
    {
        get => numberOfMoneys;
        set
        {
            numberOfMoneys = value;
            PlayerProgression.PlayerData.MoneyInPile = numberOfMoneys;
        }
    }

    private void Awake()
    {
        _transform = transform;
        moneyTransforms = FindChildrenTransformsWithTag("Money", _transform);
        for (int i = 0; i < moneyTransforms.Count; i++)
            Destroy(moneyTransforms[i].GetChild(0).gameObject);
    }

    private void Start()
    {
        int moneyInPile = PlayerProgression.PlayerData.MoneyInPile;
        for (int i = 0; i < moneyInPile; i++)
        {
            //numberOfMoneys++;
            AddMoney(_transform.position, false);
        }
    }

    public void AddMoney(Vector3 spawnPoint, bool initialization = false)
    {
        if (!initialization)
            NumberOfMoneys++;
        if (NumberOfMoneys > moneyTransforms.Count) return;
        Transform money = ObjectPooler.Instance.SpawnFromPool("Money", spawnPoint, Quaternion.identity).transform;
        moneys.Add(money);
        money.parent = moneyTransforms[NumberOfMoneys - 1];
        money.localRotation = Quaternion.identity;
        if (!initialization)
            //money.LeanMoveLocal(Vector3.zero, 0.2f);
            money.SimulateProjectileMotion(money.parent.position, 0.2f);
        else
            money.localPosition = Vector3.zero;
    }

    public Transform RemoveMoney()
    {
        if (numberOfMoneys == 0) return null;
        NumberOfMoneys--;
        if (NumberOfMoneys >= moneyTransforms.Count) return ObjectPooler.Instance.SpawnFromPool("Money", moneys[moneys.Count - 1].position, Quaternion.identity).transform;
        Transform money = moneys[NumberOfMoneys];
        moneys.RemoveAt(numberOfMoneys);
        return money;
    }

    public int Collect(out Transform money)
    {
        if (numberOfMoneys == 0)
        {
            money = null;
            return 0;
        }
        int totalMoney = moneyMultiplier;
        money = RemoveMoney();
        return totalMoney;
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
