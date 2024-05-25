using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private bool onControls = false;
    private bool onFires = false;
    private bool onTrain = false;

    private void FixedUpdate()
    {
        if (onTrain)
        {
            transform.Translate(TrainEngine.Instance.currentTrainPosition - TrainEngine.Instance.lastTrainPosition);
        }
    }

    private void ShowTrainControls()
    {
        TrainControls.Instance.Show();
        onControls = true;
    }

    private void HideTrainControls()
    {
        TrainControls.Instance.Hide();
        onControls = false;
    }

    private void ShowFireControls()
    {
        FireControls.Instance.Show();
        onFires = true;
    }

    private void HideFireControls()
    {
        FireControls.Instance.Hide();
        onFires = false;
    }

}
