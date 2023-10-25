using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompletion : MonoBehaviour
{
    [SerializeField] private GameObject _victoryScreen;
    [SerializeField] private GameObject _gameOverScreen;

    [SerializeField] private PlayerMovementController _playerMovementController;
    [SerializeField] private GameObject _playerModel;

    private static readonly string s_enemyTag = "Enemy";
    private static readonly string s_windowTag = "Window";

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(s_enemyTag))
        {
            StartCoroutine(GameOverRoutine());
        }
        else if (other.gameObject.CompareTag(s_windowTag))
        {
            StartCoroutine(VictoryRoutine());
        }
    }

    IEnumerator VictoryRoutine()
    {
        _victoryScreen.SetActive(true);
        _playerMovementController._playerInput.Player.Disable();
        _playerModel.SetActive(false);
        _playerMovementController.GetComponent<CharacterController>().detectCollisions = false;

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    IEnumerator GameOverRoutine()
    {
        _gameOverScreen.SetActive(true);
        _playerMovementController._playerInput.Player.Disable();
        _playerModel.SetActive(false);
        _playerMovementController.GetComponent<CharacterController>().detectCollisions = false;

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }
}
