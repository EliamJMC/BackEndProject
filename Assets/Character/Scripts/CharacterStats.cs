using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterStats : MonoBehaviour
{
    [Header("Components")]
    [SerializeField]    private MainCharacterController MCC;
    [SerializeField]    Timer regenerationTimer = new Timer();

    [Header("Stamina")]
    public float maxStamina = 100;
    
    public float regenerationDelay = 5f;
    public float staminaRegeneration = 5f;
    public float lastStaminaUse;

    public float runningStaminaUse = 7.5f;
    public float jumpingStaminaUse;

    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public float currentStamina;

    private bool canRegenerate = false;


    public void Start()
    {
        MCC = GetComponent<MainCharacterController>();

        currentHealth = maxHealth;
        currentStamina = maxStamina;
    }

    public void Update()
    {
        StaminaManagement();
    }

    public void StaminaManagement()
    {
        // If player is running, substract stamina
        if (MCC.isRunning)
        {
            currentStamina -= runningStaminaUse * Time.deltaTime;
            jumpingStaminaUse = runningStaminaUse * (4 / 3);

            // If player jumps while running, substract stamina
            if (MCC.isJumping)
                currentStamina -= jumpingStaminaUse;
            
            // Set last time Stamina was used
            lastStaminaUse = Time.time;
        }
        // If player jumps
        else if (!MCC.isMoving && MCC.isJumping)
        {
            jumpingStaminaUse = runningStaminaUse * (2 / 3);

            currentStamina -= jumpingStaminaUse;
            lastStaminaUse = Time.time;
        }
        // Regenerate stamina if (lastStamina use)
        else if (Time.time - lastStaminaUse >= regenerationDelay)
        {
            if (currentStamina < maxStamina)
                currentStamina += staminaRegeneration * Time.deltaTime;
        }
            
    }
    public void EnemyAttackPlayer(int damage)
    {
        currentHealth -= damage;
    }
}
