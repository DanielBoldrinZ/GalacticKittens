using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireControls : MonoBehaviour
{
    public static FireControls Instance;
    [SerializeField] float minAngle;
    [SerializeField] float maxAngle;
    [SerializeField] float curAngle;
    [SerializeField] float range;
    [SerializeField] float steps;
    [SerializeField] float angleStep;
    [SerializeField] CanvasGroup content;
    [SerializeField] float burnRate;
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

    void Update()
    {
        if (TrainControls.Instance.IsOn())
        {
            curAngle += burnRate * TrainEngine.Instance.targetSpeed * Time.deltaTime;
            if (curAngle >= maxAngle)
            {
                curAngle = maxAngle;
                TrainEngine.Instance.TurnTrainOff();
                FurnaceScript.Instance.StopAnimation();
            }
            transform.rotation = Quaternion.AngleAxis(curAngle, Vector3.forward);
        }
    }

    public void Increase()
    {
        if (LogStack.Instance.HasLogs())
        {
            LogStack.Instance.Burn();
            curAngle -= angleStep;
            TrainEngine.Instance.TryTurnTrainOn(); // maybe check if train is not on already?
            FurnaceScript.Instance.StartAnimation();
            if (curAngle <= minAngle)
            {
                curAngle = minAngle;
            }
            transform.rotation = Quaternion.AngleAxis(curAngle, Vector3.forward);

            if (LogStack.Instance.HasLogs())
            {
                content.alpha = 1;
            }
            else
            {
                content.alpha = 0.2f;
            }
        }
    }

    public void Show()
    {
        content.gameObject.SetActive(true);
        if (LogStack.Instance.HasLogs())
        {
            content.alpha = 1;
        }
        else
        {
            content.alpha = 0.2f;
        }
    }

    public void Hide()
    {
        content.gameObject.SetActive(false);
    }

    public bool IsOn()
    {
        return curAngle < maxAngle;
    }
}
