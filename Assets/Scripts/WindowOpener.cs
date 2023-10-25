using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WindowOpener : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private Transform _fence;

    [SerializeField] private List<GameObject> _windows = new List<GameObject>();

    [SerializeField] private Material _openWindowMaterial;
    [SerializeField] private Material _closedWindowMaterial;

    [SerializeField] private GameObject _projectile;
    [SerializeField] private float _projectileSpeed = 5f;

    private bool _isOpen;

    private void Start()
    {
        _projectile.SetActive(false);
    }

    private void Update()
    {
        if (!_isOpen && _player.transform.position.y >= _fence.transform.position.y)
        {
            StartCoroutine(WindowRoutine());
        }

        _projectile.transform.Translate(_projectileSpeed * Time.deltaTime * Vector3.forward);
    }

    IEnumerator WindowRoutine()
    {
        _isOpen = true;
        int randomWindowIndex = Random.Range(0, _windows.Count - 1);
        _windows[randomWindowIndex].GetComponent<Renderer>().sharedMaterial = _openWindowMaterial;
        _windows[randomWindowIndex].GetComponent<Collider>().enabled = true;

        float offsetY = 0.3f;
        _projectile.transform.position = new Vector3(
            _windows[randomWindowIndex].transform.position.x,
            _windows[randomWindowIndex].transform.position.y - offsetY,
            _player.transform.position.z);
        _projectile.transform.LookAt(_player.transform);
        _projectile.SetActive(true);

        yield return new WaitForSeconds(2f);

        _windows[randomWindowIndex].GetComponent<Renderer>().sharedMaterial = _closedWindowMaterial;
        _windows[randomWindowIndex].GetComponent<Collider>().enabled = false;
        _isOpen = false;
        _projectile.SetActive(false);
    }
}
