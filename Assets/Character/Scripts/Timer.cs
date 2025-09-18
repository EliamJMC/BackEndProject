using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Timer
{
    private float timer;
    private float waitingTime;
    private bool isCounting;

    public event Action OnTimerComplete;

    public void StartTimer(float time)
    {
        isCounting = true;
        waitingTime = time;
        timer = 0f;
    }

    //El metodo puede ser llamado desde otro script
    public void Update(float deltaTime)
    {
        if (isCounting) 
        {
            timer += deltaTime;
        }
        else
        { 
            timer = 0f;
        }

        if (timer >= waitingTime)
        {
            timer = 0f;
            OnTimerComplete?.Invoke();
            isCounting = false;
        }
    }

}
