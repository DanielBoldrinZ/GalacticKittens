using UnityEngine;

public class UpdateNoiseMaterial : MonoBehaviour
{
    public Material noiseMaterial;
    public Camera mainCamera;
    public float movementSpeed = 1.0f;

    void Update()
    {
        if (noiseMaterial && mainCamera)
        {
            noiseMaterial.SetMatrix("_CamToWorld", mainCamera.cameraToWorldMatrix);
            //noiseMaterial.SetFloat("_MovementSpeed", movementSpeed);
        }
    }
}