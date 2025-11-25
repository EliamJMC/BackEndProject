using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    public MainCharacterController controller;
    void Start() 
    {
        controller = GetComponentInParent<MainCharacterController>();
    }
    public void IniciateMotion()
    {
        controller.iniciated = true;
    }
}