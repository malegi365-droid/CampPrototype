using UnityEngine;
using System.Collections;

public class BossChargeAttack : MonoBehaviour
{
    [SerializeField] private float chargeCooldown = 5f;
    [SerializeField] private float chargeSpeed = 12f;
    [SerializeField] private float chargeDuration = 1.2f;
    [SerializeField] private float chargeDamage = 25f;

    private bool isCharging = false;
    private float lastChargeTime;

    private Transform player;

    private void Start()
    {
        player = FindObjectOfType<PartyMemberControlBridge>().transform;
    }

    private void Update()
    {
        if (isCharging) return;

        if (Time.time - lastChargeTime >= chargeCooldown)
        {
            StartCoroutine(ChargeRoutine());
        }
    }

    private IEnumerator ChargeRoutine()
    {
        isCharging = true;
        lastChargeTime = Time.time;

        Vector3 direction = (player.position - transform.position).normalized;

        float timer = 0f;

        while (timer < chargeDuration)
        {
            transform.position += direction * chargeSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        isCharging = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isCharging) return;

        HealthController hc = collision.gameObject.GetComponent<HealthController>();

        if (hc != null)
        {
            hc.TakeDamage(chargeDamage);
            Debug.Log("Boss charge hit player!");
        }
    }
}