using Unity.VisualScripting;
using UnityEngine;

public class CharacterStats
{
    public int maxHealth = 100;
    public float maxStamina = 100;

    public int currentHealth;
    public float currentStamina;

    private Timer timer = new Timer();

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }


    // Health Methods
    public void _AddHealth(int amount)
    {
        currentHealth += amount;
        if (currentHealth < 0) currentHealth = 0;
    }

    public void _RemoveHealth(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
    }


    // Stamina Methods
    public void _AutoRecoverStamina(float amount)
    {
        timer.StartTimer(3f);
        timer.OnTimerComplete += () => _AddStamina(amount);
    }

    public void _FatigueStamina(float amount)
    {
        timer.StartTimer(0.3f);
        timer.OnTimerComplete += () => _RemoveStamina(amount);
    }

    public void _AddStamina(float amount)
    {
        currentStamina += amount;
        if (currentStamina < 0) currentStamina = 0;
    }

    public void _RemoveStamina(float amount)
    {
        currentStamina -= amount;
        if (currentStamina < 0) currentStamina = 0;
    }
}
