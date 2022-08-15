using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FateGames;

public class UISwerve : MonoBehaviour
{
    private Swerve swerve;
    private bool isActive = false;
    [SerializeField] private Image innerCircle, outerCircle;
    private RectTransform outerCircleRectTransform;
    private RectTransform innerCircleRectTransform;

    private void Awake()
    {
        outerCircleRectTransform = outerCircle.rectTransform;
        innerCircleRectTransform = innerCircle.rectTransform;
        Deactivate();
        swerve = FindObjectOfType<Swerve>();
        swerve.OnStart.AddListener(Activate);
        swerve.OnRelease.AddListener(Deactivate);
    }
    private void Update()
    {
        if (isActive)
            SetCirclePositions();
    }

    public void Activate()
    {
        isActive = true;
        outerCircle.gameObject.SetActive(isActive);
        SetCirclePositions();
    }

    public void Deactivate()
    {
        isActive = false;
        outerCircle.gameObject.SetActive(isActive);
    }

    private void SetCirclePositions()
    {
        outerCircleRectTransform.position = swerve.Anchor;
        innerCircleRectTransform.position = Input.mousePosition;
    }
}
