using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Timer
{
    private float timer;
    private float waitingTime;

    public event Action OnTimerComplete;

    public void StartTimer(float time)
    {
        waitingTime = time;
        timer = 0f;
    }

    //El metodo puede ser llamado desde otro script
    public void Update(float deltaTime)
    {

        timer += deltaTime;
        if (timer >= waitingTime)
        {
            timer = 0f;
            OnTimerComplete?.Invoke();
        }
    }
}
