using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Unit : MonoBehaviour, IHittable {
    // The property / field structure is neccessary for serializing the object values. Otherwise, the prefab values would not apply to instances of it.
    public float Health {
        get {
            return health;
        }
        set {
            health = value;
            UpdateHealthbarFill();
        }
    }
    public float MaxHealth {
        get {
            return maxHealth;
        }
        set {
            maxHealth = value;
            UpdateHealthbarFill();
        }
    }
    public Factions.Faction Faction {
        get {
            return faction;
        }
        set {
            faction = value;
        }
    }

    public bool knockable;
    public bool invincible;

    [SerializeField] float health = 100f;
    [SerializeField] float maxHealth = 100f;
    [SerializeField] Factions.Faction faction;
    
    UnityEngine.UI.Image healthbarImage;
    Transform healthbar;
    Camera cam;

    void Start() {
        cam = Camera.main;
        healthbar = Instantiate(HealthbarManager.Prefab).transform;
        healthbar.SetParent(HealthbarManager.Instance.transform, false);
        healthbarImage = healthbar.GetChild(0).GetChild(0).GetComponent<UnityEngine.UI.Image>();
        UpdateHealthbarPosition();
        UpdateHealthbarFill();
    }

    void LateUpdate() {
        UpdateHealthbarPosition();
    }

    public void Hit(float damage, float impulse, Vector2 point, Vector2 direction) {
        if (damage < 0) {
            Debug.LogWarning("Hitting Character with negative amount of damage");
        }

        if (knockable)
            GetComponent<Rigidbody2D>().AddForceAtPosition(direction.normalized * impulse, point, ForceMode2D.Impulse);

        if (invincible)
            return;

        if (damage >= health) {
            health = 0;
            Die();
        } else {
            health -= damage;
        }

        UpdateHealthbarFill();
    }

    void UpdateHealthbarFill() {
        if (healthbarImage != null)
            healthbarImage.fillAmount = health / maxHealth;
    }

    void UpdateHealthbarPosition() {
        Vector3 ssPos = cam.WorldToScreenPoint(transform.position);
        healthbar.position = ssPos + Vector3.up * HealthbarManager.offset;
    }

    virtual protected void Die() {
        Destroy(gameObject);
    }

    void OnDisable() {
        if (healthbar != null)
            healthbar.gameObject.SetActive(false);
    }

    void OnDestroy() {
        if (healthbar != null)
            Destroy(healthbar.gameObject);
    }
}
