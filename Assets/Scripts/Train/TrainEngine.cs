using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TrainEngine : MonoBehaviour
{
    public static TrainEngine Instance;
    public Vector2 lastTrainPosition;
    public Vector2 currentTrainPosition;
    private Transform cachedTR;
    public float targetSpeed;
    [SerializeField] List<HingeJoint2D> wheels = new List<HingeJoint2D>();
    [SerializeField] List<float> speeds = new List<float>();
    [SerializeField] ParticleSystem smoke;
    [SerializeField] Rigidbody2D rb;

    public bool IsOwner;

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

        cachedTR = transform;

        Invoke("FreezeY", 0.5f);
    }

    private void FreezeY()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
    }

    private void FixedUpdate()
    {
        lastTrainPosition = currentTrainPosition;
        currentTrainPosition = cachedTR.position;
    }

    public void SetTargetSpeed(int speed)
    {
        targetSpeed = speeds[speed];

        if (targetSpeed > 0 && FireControls.Instance.IsOn())
        {
            SetSpeed(targetSpeed);
        }
        else if (targetSpeed == 0)
        {
            SetSpeed(targetSpeed);
        }
    }

    private void SetSpeed(float _targetSpeed)
    {
        if (_targetSpeed > 0)
        {
            smoke.Play();
            var emission = smoke.emission;
            emission.rateOverTime = _targetSpeed / 100;
        }
        else
        {
            if (FireControls.Instance.IsOn())
            {
                smoke.Play();
                var emission = smoke.emission;
                emission.rateOverTime = 0.5f;
            }
            else
            {
                smoke.Stop();
            }
        }

        foreach (var item in wheels)
        {
            JointMotor2D newMotor = item.motor;
            newMotor.motorSpeed = _targetSpeed;
            item.motor = newMotor;
        }
    }

    public void TryTurnTrainOn()
    {
        SetSpeed(targetSpeed);
    }

    public void TurnTrainOff()
    {
        SetSpeed(0);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.tag.Equals("trainStop"))
        {
            Destroy(collider.gameObject);
            TrainControls.Instance.TurnOff();
        }
    }
}
