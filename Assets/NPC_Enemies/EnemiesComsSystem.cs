using System.Collections.Generic;
using System;
using UnityEngine;

public class EnemiesComsSystem : MonoBehaviour
{
    public List<Enemies> enemiesInScene = new List<Enemies>();
    public List<Enemies> enemiesDetectedPlayer = new List<Enemies>();

    void Update()
    {
        SetAllDetectors();
        SetAllEnemies();
    }
    void SetAllEnemies()
    {
        Enemies[] enemies = FindObjectsByType<Enemies>(FindObjectsSortMode.None);
        List<Enemies> validationList = new List<Enemies>();
        foreach (var enemy in enemies)
            if (!enemiesInScene.Contains(enemy))
                enemiesInScene.Add(enemy);  

        foreach(var enemy in enemiesInScene)
            for(int i = 0; i < enemies.Length; i++)
                if (enemies[i] == enemy)
                    validationList.Add(enemy);
        enemiesInScene.Clear();
        enemiesInScene = validationList;
    }
    void SetAllDetectors()
    {
        foreach (var enemy in enemiesInScene)
        {
            Enemies script = enemy.GetComponent<Enemies>();
            if (script.DetectedPlayer() && !enemiesDetectedPlayer.Contains(enemy))
                enemiesDetectedPlayer.Add(enemy);
            else if (!script.DetectedPlayer() && enemiesDetectedPlayer.Contains(enemy))
                enemiesDetectedPlayer.Remove(enemy);
        }
    }
    public Enemies FindNearestDetector(Enemies leaded)
    {
        float
            dist,
            nearestDist = Mathf.Infinity;
        Enemies leader;
        Enemies script = leaded.GetComponent<Enemies>();
        foreach (var possibleLeader in enemiesDetectedPlayer)
        {
            if (possibleLeader == null) continue;
            dist = Vector3.Distance(leaded.transform.position, possibleLeader.transform.position);
            if (dist < nearestDist && dist <= script.comRadius)
            {
                leader = possibleLeader;
                nearestDist = dist;
                script.detectorIsClose = true;
                return leader;
            }
        }
        return null;
    }
}
