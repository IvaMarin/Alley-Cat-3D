using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageEnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject _garbageEnemy = null;

    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
    [SerializeField] private List<Collider> _spawnPointsColliders = new List<Collider>();

    private bool isEnemyActive = false;
    private float enemySpeed = 0.5f;

    private void Start()
    {
        _garbageEnemy.SetActive(false);
    }

    private void Update()
    {
        if (!isEnemyActive)
        {
            StartCoroutine(GarbageEnemyRoutine());
        }

        _garbageEnemy.transform.Translate(enemySpeed * Time.deltaTime * Vector3.up);
    }

    IEnumerator GarbageEnemyRoutine()
    {
        isEnemyActive = true;
        int spawnPointIndex = Random.Range(0, _spawnPoints.Count - 1);
        _spawnPointsColliders[spawnPointIndex].enabled = false;

        float offsetY = 0.3f;
        _garbageEnemy.transform.position = new Vector3(
            _spawnPoints[spawnPointIndex].transform.position.x,
            _spawnPoints[spawnPointIndex].transform.position.y - offsetY,
            _garbageEnemy.transform.position.z);
        _garbageEnemy.SetActive(true);

        yield return new WaitForSeconds(2f);

        _spawnPointsColliders[spawnPointIndex].enabled = true;
        isEnemyActive = false;
        _garbageEnemy.SetActive(false);
    }
}
