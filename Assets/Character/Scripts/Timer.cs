using System;

public class Timer
{
    public float time;
    private float waitingTime;
    public bool isCounting = false;

   
    public bool timerStarted;
    public event Action OnTimerComplete;

    public void StartTimer(float time)
    {
        timerStarted = true;

        isCounting = true;
        waitingTime = time;
        this.time = 0f;
    }

    //El metodo puede ser llamado desde otro script
    public void Update(float deltaTime)
    {
        if (isCounting) 
            time += deltaTime;
        else
            time = 0f;

        if (time >= waitingTime)
        {
            time = 0f;
            OnTimerComplete?.Invoke();
            isCounting = false;
        }
    }
    
    public void StopTimer()
    {
        isCounting = false;
        time = 0f;
    }
}
