using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainControls : MonoBehaviour
{
    public static TrainControls Instance;
    [SerializeField] float minAngle;
    [SerializeField] float maxAngle;
    [SerializeField] float curAngle;
    [SerializeField] float range;
    [SerializeField] float steps;
    [SerializeField] float angleStep;
    [SerializeField] GameObject content;

    public bool isLocked = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnValidate()
    {
        range = maxAngle - minAngle;
        angleStep = range / steps;
    }

    public void Decrease()
    {
        curAngle += angleStep;
        if (curAngle > maxAngle)
        {
            curAngle = maxAngle;
        }
        UpdateSpeedAndAngle();
    }

    public void Increase()
    {
        curAngle -= angleStep;
        if (curAngle < minAngle)
        {
            curAngle = minAngle;
        }
        UpdateSpeedAndAngle();
    }

    private void UpdateSpeedAndAngle()
    {
        transform.rotation = Quaternion.AngleAxis(curAngle, Vector3.forward);
        TrainEngine.Instance.SetTargetSpeed((int)(((maxAngle - curAngle) / maxAngle) * 2));
    }

    public void Show()
    {
        content.SetActive(true);
    }

    public void Hide()
    {
        content.SetActive(false);
    }

    public bool IsOn()
    {
        return curAngle < maxAngle;
    }

    public void TurnOff()
    {
        curAngle = maxAngle;
        UpdateSpeedAndAngle();
    }

    public void Lock()
    {
        isLocked = true;
    }

    public void Unlock()
    {
        isLocked = false;
    }
}
