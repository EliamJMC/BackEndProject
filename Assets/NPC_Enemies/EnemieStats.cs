using UnityEngine;

public class EnemieStats : MonoBehaviour
{
    private Enemies enemies;
    int maxHealth = 100;
    int courentHealth;
    int regenFactor;
    float regenDelay = 4.0f;
    float lastRegen;

    private void Start()
    {
        enemies = GetComponent<Enemies>();
        maxHealth = Mathf.RoundToInt(maxHealth * enemies.scaleValue);
        regenFactor = enemies.attackDamage;
        courentHealth = maxHealth;
    }

    private void Update()
    {
        if (courentHealth <= 0) Destroy(gameObject);
        autoHealthRegen();
    }
    void autoHealthRegen()
    {
        if ((courentHealth < maxHealth) && (Time.time - lastRegen >= regenDelay))
        {
            courentHealth += regenFactor;
            lastRegen = Time.time;
        }
    }
    void damageEnemie(int damage)
    {
        courentHealth -= damage;
    }
}
