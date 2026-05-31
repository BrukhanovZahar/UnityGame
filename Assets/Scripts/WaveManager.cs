using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveManager : MonoBehaviour
{
    [Serializable] public class WaveEvent : UnityEvent<int> { }

    [Header("Drones")]
    [SerializeField] private DroneController[] dronePrefabs;
    [SerializeField] private float spawnDistance = 60f;
    [SerializeField] private float spawnInterval = 0.6f;

    [Header("Wave size & difficulty")]
    [SerializeField] private int baseDroneCount = 3;
    [SerializeField] private int droneCountGrowth = 2;
    [SerializeField] private float damageGrowthPerWave = 0.15f;
    [SerializeField] private float speedGrowthPerWave = 0.08f;
    [SerializeField] private float hpGrowthPerWave = 0.20f;

    [Header("Timing")]
    [SerializeField] private float startDelay = 3f;
    [SerializeField] private bool waitForShop = false;
    [SerializeField] private float betweenWavesDelay = 6f;

    [Header("Events")]
    [SerializeField] private WaveEvent onWaveStarted;
    [SerializeField] private WaveEvent onWaveCleared;

    private readonly List<GameObject> activeDrones = new List<GameObject>();
    private int currentWave;
    private bool waitingForNextWave;

    public int CurrentWave => currentWave;

    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(startDelay);

        while (!GlobalContext.End)
        {
            currentWave++;
            onWaveStarted.Invoke(currentWave);

            yield return StartCoroutine(SpawnWave(currentWave));

            yield return new WaitUntil(() => CountAlive() == 0 || GlobalContext.End);
            if (GlobalContext.End) yield break;

            onWaveCleared.Invoke(currentWave);

            if (waitForShop)
            {
                waitingForNextWave = true;
                yield return new WaitUntil(() => !waitingForNextWave || GlobalContext.End);
            }
            else
            {
                yield return new WaitForSeconds(betweenWavesDelay);
            }
        }
    }

    private IEnumerator SpawnWave(int wave)
    {
        int count = baseDroneCount + (wave - 1) * droneCountGrowth;
        float dmgMul = 1f + (wave - 1) * damageGrowthPerWave;
        float spdMul = 1f + (wave - 1) * speedGrowthPerWave;
        float hpMul = 1f + (wave - 1) * hpGrowthPerWave;

        for (int i = 0; i < count && !GlobalContext.End; i++)
        {
            while (GlobalContext.Pause && !GlobalContext.End) yield return null;

            SpawnDrone(dmgMul, spdMul, hpMul);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnDrone(float dmgMul, float spdMul, float hpMul)
    {
        if (dronePrefabs == null || dronePrefabs.Length == 0)
            return;

        DroneController prefab = dronePrefabs[UnityEngine.Random.Range(0, dronePrefabs.Length)];
        Vector3 pos = GetSpawnPosition();

        DroneController drone = Instantiate(prefab, pos, Quaternion.identity);
        drone.Configure(dmgMul, spdMul, hpMul);
        activeDrones.Add(drone.gameObject);
    }

    private Vector3 GetSpawnPosition()
    {
        Vector3 center = GlobalContext.AsteriodCollider != null
            ? GlobalContext.AsteriodCollider.bounds.center
            : transform.position;

        Vector3 dir = UnityEngine.Random.onUnitSphere;
        dir.y = Mathf.Abs(dir.y);
        return center + dir * spawnDistance;
    }

    private int CountAlive()
    {
        activeDrones.RemoveAll(d => d == null);
        return activeDrones.Count;
    }

    public void ContinueToNextWave()
    {
        waitingForNextWave = false;
    }
}
