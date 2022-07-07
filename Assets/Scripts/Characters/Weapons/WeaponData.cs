using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/WeaponScriptableObject", order = 1)]
public class WeaponData : ScriptableObject
{
    public enum ShotSpawnsUsage { Cyclic, Simultaneous };

    public bool hasSingle = true;
    public bool hasBurst = false;
    public bool hasAuto = false;
    public bool hasAutoBurst = false;

    public float cooldown = 0.2f;
    public float afterBurstCooldown = 0.3f;

    public float equipTime = 0.2f;
    public float damage = 10f;
    public float speed = 20f;
    public float speedVariation = 1f;
    public float impulseRecoil = 0f;
    public float impulseHit = 5f;
    public int magSize = 30;
    public float reloadTime = 0.4f;

    public int burstCount = 3;
    public int projectilesPerShotSpawn = 1;
    public float projectilesOffsetAngle = 0f;
    public GameObject prjPrefab;
    public float inheritVelocityMultiplier = 0f;

    public float maxOffsetAngle = 0f;
    public float recoilDecayTime = 0.6f;
    public float maxRecoilAngle = 50f;

    public ShotSpawnsUsage shotSpawnsUsage = ShotSpawnsUsage.Simultaneous;

    public Transform[] shotSpawns = new Transform[1];
}