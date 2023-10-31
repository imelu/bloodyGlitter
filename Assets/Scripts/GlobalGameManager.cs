using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using TMPro;
using UnityEditor.Rendering;

public class GlobalGameManager : MonoBehaviour
{
    private SplatterManager _splatterManager;
    public SplatterManager splatterManager { get { return _splatterManager; } }

    [SerializeField] private Image _fader;
    [SerializeField] private Image _winFader;

    [SerializeField] private Animator _lightning;
    private Coroutine lightningCor;

    [SerializeField] private List<GameObject> _playerWinScreen = new List<GameObject>();

    [SerializeField] private List<Image> _endGameBars = new List<Image>();

    [SerializeField] private RectTransform _leftCloud;
    [SerializeField] private RectTransform _rightCloud;

    #region Singleton
    public static GlobalGameManager Instance;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //StartMusic();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        _splatterManager = GetComponent<SplatterManager>();
    }
    #endregion

    private int _haters;
    public int haters { get { return _haters; } }

    [SerializeField] private float _gameTimeSeconds;
    private float _currentGameTime;

    private bool _gameOver;

    private int winnerIndex;

    public delegate void OnGameOverDelegate();
    public event OnGameOverDelegate OnGameOver;

    public delegate void OnGoreEnterDelegate();
    public event OnGoreEnterDelegate OnGore;

    public delegate void OnGlitterEnterDelegate();
    public event OnGlitterEnterDelegate OnGlitter;

    #region Music

    private EventInstance _music;
    [SerializeField] private EventReference _musicEvent;

    #endregion

    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private Color _glitterTimerColor;
    [SerializeField] private Color _goreTimerColor;

    private void OnSceneLoaded(Scene loadedScene, LoadSceneMode mode)
    {
        if (lightningCor != null) StopCoroutine(lightningCor);
        if (loadedScene.buildIndex == 0)
        {
            _music.setParameterByName("gamestage", 1);
            _fader.DOColor(_fader.color * new Color(1, 1, 1, 0), 0.5f);
        } else if (loadedScene.buildIndex == 1)
        {
            // play metal already?
            _fader.DOColor(_fader.color * new Color(1, 1, 1, 0), 0.5f).OnComplete(() => MoveCloudsOut());
        }

        splatterManager.ClearTextures();
    }

    public void StartMusic()
    {
        if (IsPlaying(_music)) return;
        _music = RuntimeManager.CreateInstance(_musicEvent);
        _music.setParameterByName("gamestage", 1);
        _music.start();
    }

    bool IsPlaying(EventInstance instance)
    {
        PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        return state != PLAYBACK_STATE.STOPPED;
    }

    private void MoveCloudsOut()
    {
        _leftCloud.DOLocalMoveX(-1920, 2);
        _rightCloud.DOLocalMoveX(1920, 2);
    }

    private void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Mouse0)) _fader.DOColor(_fader.color + new Color(0, 0, 0, 1), 0.1f).OnComplete(() => _fader.DOColor(_fader.color * new Color(1, 1, 1, 0), 0.5f));
        
    }

    public void EnterHatred(PlayerController player)
    {
        if(_haters == 0) _music.setParameterByName("gamestage", 3);
        _haters++;
        _fader.DOColor(_fader.color + new Color(0, 0, 0, 1), 0.1f).OnComplete(() => HatredFadeIn(player, true));
    }

    public void ExitHatred(PlayerController player)
    {
        _haters--;
        if (_haters == 0 && !_gameOver) _music.setParameterByName("gamestage", 2);
        _fader.DOColor(_fader.color + new Color(0, 0, 0, 1), 0.1f).OnComplete(() => HatredFadeIn(player, false));
    }

    public void HatredFadeIn(PlayerController player, bool enteringHatred)
    {
        if (enteringHatred)
        {
            player.TransformToGore();
        }
        else
        {
            player.TransformToGlitter();
        }
        if (_haters == 0 && !enteringHatred)
        {
            OnGlitter?.Invoke();
            GlitterLayerMask();
            StopCoroutine(lightningCor);
            _timerText.color = _glitterTimerColor;
        } 
        else if(_haters == 1 && enteringHatred)
        {
            OnGore?.Invoke();
            GoreLayerMask();
            lightningCor = StartCoroutine(LightningTimer());
            _timerText.color = _goreTimerColor;
        } 
        if (enteringHatred)
        {
            _lightning.Play("Lightning1");
        }
        _fader.DOColor(_fader.color * new Color(1, 1, 1, 0), 0.5f);
    }

    private void GlitterLayerMask()
    {
        Camera.main.cullingMask |= 1 << LayerMask.NameToLayer("Glitter");
        Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("Gore"));
    }

    private void GoreLayerMask()
    {
        Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("Glitter"));
        Camera.main.cullingMask |= 1 << LayerMask.NameToLayer("Gore");
    }

    private void PlayRandomLightning()
    {
        int lightning = Random.Range(1, 4);

        _lightning.Play("Lightning" + lightning);
    }

    IEnumerator LightningTimer()
    {
        yield return new WaitForSeconds(Random.Range(3f, 6f));
        PlayRandomLightning();
        lightningCor = StartCoroutine(LightningTimer());
    }

    public void LoadGameScene()
    {
        _fader.DOColor(_fader.color + new Color(0, 0, 0, 1), 0.5f).OnComplete(() => SceneManager.LoadScene(1));
    }

    public void LoadMainMenu()
    {
        ResetVals();
        _leftCloud.DOLocalMoveX(0, 0.5f);
        _rightCloud.DOLocalMoveX(0, 0.5f);
        _fader.DOColor(_fader.color + new Color(0, 0, 0, 1), 0.5f).OnComplete(() => SceneManager.LoadScene(0));
    }

    public void StartGame()
    {
        _music.setParameterByName("gamestage", 2);
        StartCoroutine(GameTime());
    }

    IEnumerator GameTime()
    {
        _timerText.gameObject.SetActive(true);
        UpdateTime(0);
        while (_currentGameTime < _gameTimeSeconds)
        {
            _currentGameTime += Time.deltaTime;
            UpdateTime((int)_currentGameTime);
            yield return null;
        }
        _gameOver = true;
        OnGameOver?.Invoke();
        while (_haters != 0)
        {
            yield return null;
        }
        _fader.gameObject.SetActive(false);
        _winFader.DOColor(Color.white * new Color(1, 1, 1, 0.5f), 0.5f).OnComplete(()=>EndGame());
    }

    private void EndGame()
    {
        float percentP1;
        float percentP2;

        Vector2 data = splatterManager.CalculateWinner();
        percentP1 = data.x;
        percentP2 = data.y;

        winnerIndex = percentP2 > percentP1 ? 1 : 0;

        _playerWinScreen[winnerIndex].SetActive(true);

        //Debug.Log(percentP2);
        //Debug.Log(percentP1);

        _endGameBars[0].transform.parent.gameObject.SetActive(true);
        _endGameBars[0].DOFillAmount(percentP1, 1);
        _endGameBars[1].DOFillAmount(percentP2, 1);

        if (_haters == 0) _music.setParameterByName("gamestage", 4);

        _timerText.gameObject.SetActive(false);
    }

    private void UpdateTime(int remainingTime)
    {
        remainingTime = (int)_gameTimeSeconds - remainingTime;
        int minutes = remainingTime / 60;
        int seconds = remainingTime % 60;

        int secondsTens = seconds / 10;
        int secondsOnes = seconds % 10;

        _timerText.text = minutes + ":" + secondsTens + secondsOnes;
    }

    private void ResetVals()
    {
        _gameOver = false;
        _haters = 0;
        _currentGameTime = 0;
        _endGameBars[0].transform.parent.gameObject.SetActive(false);
        _playerWinScreen[winnerIndex].SetActive(false);
        _fader.gameObject.SetActive(true);
        _winFader.color *= new Color(1, 1, 1, 0f);
    }
}
