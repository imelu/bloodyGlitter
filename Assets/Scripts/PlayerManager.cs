using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerManager : MonoBehaviour
{
    private List<GameObject> _players = new List<GameObject>();

    private PlayerInputManager _playerInputManager;

    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();

    [SerializeField] private TMP_Text _joinText;
    private void Awake()
    {
        _playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void Start()
    {
        _joinText.DOColor(_joinText.color * new Color(1, 1, 1, 0f), 1f).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnEnable()
    {
        _playerInputManager.onPlayerJoined += AddPlayer;
    }

    private void OnDisable()
    {
        _playerInputManager.onPlayerJoined -= AddPlayer;
    }

    public void AddPlayer(PlayerInput player)
    {
        _players.Add(player.gameObject);

        if (_players.Count >= 2)
        {
            _joinText.gameObject.SetActive(false);
            GetComponent<PlayerInputManager>().DisableJoining();
        }

        PlayerController playerController = player.GetComponent<PlayerController>();

        playerController.inputDisabled = true;
        
        int index = _players.Count - 1;
        playerController.playerIndex = index;
        playerController.playerManager = this;
        player.transform.position = _spawnPoints[index].position;
    }

    public void PlayerSpawned()
    {
        if (_players.Count == 2)
        {
            _players[0].GetComponent<PlayerController>().inputDisabled = false;
            _players[1].GetComponent<PlayerController>().inputDisabled = false;

            GlobalGameManager.Instance.StartGame();
        }
    }
}
