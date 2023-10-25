using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundEnemySpawner : MonoBehaviour
{
    private enum State
    {
        MovingLeft,
        MovingRight,
        Spawning,
    }
    private State _currentState;

    [SerializeField] private Transform _leftSpawnPoint;
    [SerializeField] private Transform _rightSpawnPoint;
    [SerializeField] private GameObject _groundEnemy;
    [SerializeField] private float _moveSpeed = 1f;

    [SerializeField] private int _spawnRate = 150;

    private void Start()
    {
        _groundEnemy.SetActive(false);
        _currentState = State.Spawning;
    }

    private void Update()
    {
        switch (_currentState)
        {
            case State.Spawning:
                {
                    int roll = Random.Range(1, _spawnRate);
                    if (roll == _spawnRate / 2)
                    {
                        _groundEnemy.SetActive(true);
                        _groundEnemy.transform.position = _leftSpawnPoint.position;
                        _groundEnemy.transform.rotation = Quaternion.Euler(0, -90, 0);
                        _currentState = State.MovingRight;
                    }
                    if (roll == 1)
                    {
                        _groundEnemy.SetActive(true);
                        _groundEnemy.transform.position = _rightSpawnPoint.position;
                        _groundEnemy.transform.rotation = Quaternion.Euler(0, 90, 0);
                        _currentState = State.MovingLeft;
                    }
                    break;
                }
            case State.MovingRight:
                {
                    _groundEnemy.transform.Translate(_moveSpeed * Time.deltaTime * Vector3.forward);
                    if (_groundEnemy.transform.position.x <= _rightSpawnPoint.position.x)
                    {
                        _groundEnemy.SetActive(false);
                        _currentState = State.Spawning;
                    }
                    break;
                }
            case State.MovingLeft:
                {
                    _groundEnemy.transform.Translate(_moveSpeed * Time.deltaTime * Vector3.forward);
                    if (_groundEnemy.transform.position.x >= _leftSpawnPoint.position.x)
                    {
                        _groundEnemy.SetActive(false);
                        _currentState = State.Spawning;
                    }
                    break;
                }
        }
    }
}
