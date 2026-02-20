using UnityEngine;

public class Attackable : MonoBehaviour, IHasHealth, IAttackable
{
    [Header("Team & Classification")]
    public int teamID;
    public bool isCivilian;
    public bool isBuilding;

    [Header("Health")]
    public float maxHealth = 100f;
    float currentHealth;

    [Header("Repair")]
    public bool canBeRepaired = true;

    [Header("Hit Feedback")]
    public Renderer flashRenderer;
    public Color hitFlashColor = new(1f, 0.3f, 0.3f, 1f);
    public float hitFlashDuration = 0.08f;

    IAttackable attackProxy;
    IHasHealth healthProxy;
    Civilian civilianProxy;
    MaterialPropertyBlock block;
    Color baseColor = Color.white;
    float flashTimer;

    public int TeamID => teamID;
    public Transform AimPoint => transform;
    public bool IsAlive => CurrentHealth > 0f;
    public float CurrentHealth => healthProxy != null ? healthProxy.CurrentHealth : currentHealth;
    public float MaxHealth => healthProxy != null ? healthProxy.MaxHealth : maxHealth;
    public bool IsDamaged => IsAlive && CurrentHealth < MaxHealth;

    void Awake()
    {
        var unit = GetComponent<Unit>();
        var civ = GetComponent<Civilian>();
        var building = GetComponent<Building>();

        if (unit != null)
        {
            teamID = unit.teamID;
            attackProxy = unit;
            healthProxy = unit;
        }
        else if (civ != null)
        {
            teamID = civ.teamID;
            civilianProxy = civ;
            healthProxy = civ.GetComponent<HealthComponent>();
            isCivilian = true;
        }
        else if (building != null)
        {
            teamID = building.teamID;
            attackProxy = building;
            healthProxy = building;
            isBuilding = true;
        }

        if (healthProxy == null)
            currentHealth = maxHealth;

        if (flashRenderer == null)
            flashRenderer = GetComponentInChildren<Renderer>();

        if (flashRenderer != null)
        {
            block = new MaterialPropertyBlock();
            if (flashRenderer.sharedMaterial != null && flashRenderer.sharedMaterial.HasProperty("_Color"))
                baseColor = flashRenderer.sharedMaterial.color;
        }
    }

    void Update()
    {
        if (flashRenderer == null || block == null)
            return;

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(flashTimer / Mathf.Max(0.01f, hitFlashDuration));
            Color c = Color.Lerp(baseColor, hitFlashColor, t);
            flashRenderer.GetPropertyBlock(block);
            block.SetColor("_Color", c);
            flashRenderer.SetPropertyBlock(block);

            if (flashTimer <= 0f)
            {
                flashRenderer.GetPropertyBlock(block);
                block.SetColor("_Color", baseColor);
                flashRenderer.SetPropertyBlock(block);
            }
        }
    }

    public void Repair(float amount)
    {
        if (!canBeRepaired || !IsAlive || healthProxy != null) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public void TakeDamage(float dmg)
    {
        if (!IsAlive) return;

        flashTimer = hitFlashDuration;

        if (attackProxy != null)
        {
            attackProxy.TakeDamage(dmg);
            return;
        }

        if (civilianProxy != null)
        {
            civilianProxy.TakeDamage(dmg);
            return;
        }

        currentHealth -= dmg;

        AlertManager.Instance?.Push($"{name} is under attack!");

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        AlertManager.Instance?.Push($"{name} destroyed");
        Destroy(gameObject);
    }
}
