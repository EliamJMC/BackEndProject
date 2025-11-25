using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeleportAttack : MonoBehaviour
{
    public CharacterController cc;

    public float detectionRange = 15.0f;
    public LayerMask enemyMask;
    public float behindDistance = 1.5f;
    public float timeBetweenTeleports = 10f;

    public void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update() 
    {
        if (Input.GetKeyDown(KeyCode.F))
            StartChainTeleport(5);
    }

    public void StartChainTeleport(int maxTargets = 5)
    {
        StartCoroutine(ChainTeleportRoutine(maxTargets));
    }

    void TeleportBehind(Transform enemy)
    {
        cc.enabled = false;
        Vector3 dir = enemy.forward;
        Vector3 newpos = enemy.position + dir * behindDistance;

        newpos.y = transform.position.y;

        transform.position = newpos;
        transform.LookAt(enemy);
        Vector3 rot = transform.eulerAngles;
        rot.x = 0;
        rot.z = 0;
        transform.eulerAngles = rot;
        cc.enabled = true;
    }

    IEnumerator ChainTeleportRoutine(int maxTargets)
    {
        Debug.Log("Inicio Corrutina");

        for (int i = 0; i < maxTargets; i++)
        {
            yield return new WaitForSeconds(timeBetweenTeleports);

            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, enemyMask);

            if (hits.Length == 0) yield break;

            var target = hits
                .Select(h => h.transform)
                .OrderBy(h => Vector3.Distance(transform.position, h.position))
                .FirstOrDefault();

            if (target == null) yield break;

            TeleportBehind(target);
            Destroy(target.gameObject);
        }

        Debug.Log("Fin Corrutina");
    }
}
