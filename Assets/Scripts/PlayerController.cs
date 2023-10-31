using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;
using FMODUnity;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed;

    private Vector2 _direction;
    private float _currentSpeed;

    private SpriteRenderer _spriteRenderer;

    private Transform _arenaTransform;

    private Animator _anim;

    private PlayerManager _playerManager;
    public PlayerManager playerManager { get { return _playerManager; } set { _playerManager = value; } }

    private int _playerIndex = 0;
    public int playerIndex { get { return _playerIndex; } set { _playerIndex = value; } }

    private bool _inputDisabled;
    public bool inputDisabled { get { return _inputDisabled; } set
        {
            if (_gameOver) _inputDisabled = true;
            else _inputDisabled = value;
        }
    }

    [SerializeField] private List<RuntimeAnimatorController> _animsGlitter = new List<RuntimeAnimatorController>();
    [SerializeField] private List<RuntimeAnimatorController> _animsGore = new List<RuntimeAnimatorController>();

    [SerializeField] private GameObject _GlitterVisual;
    [SerializeField] private GameObject _GoreVisual;

    private bool _gameOver;

    #region Abilities

    [SerializeField] private GameObject _PunchTrigger;
    [SerializeField] private GameObject _GrabTrigger;
    [SerializeField] private GameObject _NPPPunchTrigger;
    [SerializeField] private GameObject _HeadPrefab;

    private bool _punchCD;
    public bool punchCD { get { return _punchCD; } set
        {
            _punchCD = value;
            if (!value) _PunchTrigger.SetActive(false);
        }
    }

    private bool _grabCD;
    private bool _nppPunchCD;
    public bool nppPunchCD { get { return _nppPunchCD; } set
        {
            _nppPunchCD = value;
            if (!value) _NPPPunchTrigger.SetActive(false);
        }
    }

    private bool _nppGrabbed;
    public bool nppGrabbed { get { return _nppGrabbed; } set
        {
            _nppGrabbed = value;
            _anim.SetBool("hasNPP", value);
        }
    }
    private int _nppDurability;

    private bool _dashCD;
    private bool _dashing;
    private Vector2 _dashDir;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private float _plowingDashSpeed;
    [SerializeField] private float _dashTime;
    [SerializeField] private float _dashCDTime;

    [SerializeField] private int _punchDamage;
    [SerializeField] private int _nppPunchDamage;
    [SerializeField] private int _throwDamage;

    #endregion

    #region HatredBar

    [SerializeField] private Image _hatredBar;
    [SerializeField] private List<Image> _hatredBars = new List<Image>();
    private float _currentHatred;

    [SerializeField] private int _hatredPerNPPKill;
    [SerializeField] private int _hatredLossPerPPHit;

    [SerializeField] private float _hatredTime;
    private float _currentHatredTime;

    private bool _hating = false;

    #endregion

    #region Sound

    [SerializeField] private EventReference _SFXGrab; // grab
    [SerializeField] private EventReference _SFXKill; // npp kill
    [SerializeField] private EventReference _SFXSmash; // punch with npp in glitter mode
    [SerializeField] private EventReference _SFXGlitterPunch; // normal punch in glitter mode
    [SerializeField] private EventReference _SFXLightningDash; // punch in gore mode
    [SerializeField] private EventReference _SFXRainbowDash; // dash in glitter mode
    [SerializeField] private EventReference _SFXSpawn; // dash in glitter mode
    [SerializeField] private EventReference _SFXTransform; // dash in glitter mode

    #endregion

    [SerializeField] private Vector3 _arenaScale;

    // Start is called before the first frame update
    void Awake()
    {
        _spriteRenderer = _GlitterVisual.GetComponent<SpriteRenderer>();
        _arenaTransform = GameObject.FindGameObjectWithTag("Arena").GetComponent<Transform>();
        _anim = _GlitterVisual.GetComponent<Animator>();

        GlobalGameManager.Instance.OnGameOver += GameOver;
        GlobalGameManager.Instance.OnGore += OnGoreEnter;
        GlobalGameManager.Instance.OnGlitter += OnGlitterEnter;
    }

    private void Start()
    {
        _currentSpeed = _speed;
        _anim.runtimeAnimatorController = _animsGlitter[playerIndex];
        _GoreVisual.GetComponent<Animator>().runtimeAnimatorController = _animsGore[playerIndex];
        if (playerIndex == 1)
        {
            //_hatredBar.transform.parent.transform.localScale = Vector3.Scale(_hatredBar.transform.parent.transform.localScale, new Vector3(1, -1, 1));
            _hatredBars[2].transform.parent.localScale = Vector3.Scale(_hatredBars[2].transform.parent.transform.localScale, new Vector3(1, -1, 1));
            _hatredBars[0].transform.parent.gameObject.SetActive(false);
        }
        else
        {
            _hatredBars[1].transform.parent.gameObject.SetActive(false);
            _spriteRenderer.flipX = true;
        }
        RuntimeManager.PlayOneShot(_SFXSpawn);
    }

    // Update is called once per frame
    void Update()
    {
        if (inputDisabled) return;
        if (!_dashing && !_punchCD && !_nppPunchCD) MovePlayer();
        else if (_dashing) Dash();
        else if (_hating && (_punchCD || _nppPunchCD))
        {
            if (_direction.magnitude == 0)
            {
                _dashDir = _spriteRenderer.flipX ? Vector2.right : Vector2.left;
            }
            else
            {
                _dashDir = _direction;
            }
            Dash();
        }
        else _anim.SetFloat("velocity", 0);
        SetOrderInLayer();
    }

    #region Input

    public void OnMove(InputValue inputValue)
    {
        Vector2 MoveDelta = inputValue.Get<Vector2>();
        _direction = new Vector2(MoveDelta.x, MoveDelta.y).normalized;
    }

    public void OnPunch()
    {
        if (inputDisabled || _punchCD || _nppPunchCD || _grabCD) return;
        if (!_nppGrabbed)
        {
            if (!_hating) RuntimeManager.PlayOneShot(_SFXGlitterPunch);
            else RuntimeManager.PlayOneShot(_SFXLightningDash);
            FlipTrigger(_PunchTrigger.transform);
            _PunchTrigger.SetActive(true);
            _anim.SetTrigger("punch");
            _punchCD = true;
        }
        else
        {
            if (!_hating) RuntimeManager.PlayOneShot(_SFXSmash);
            else RuntimeManager.PlayOneShot(_SFXLightningDash);
            _nppDurability--;
            if (_nppDurability <= 0)
            {
                nppGrabbed = false;
            }
            _anim.SetTrigger("punch");
            FlipTrigger(_NPPPunchTrigger.transform);
            _NPPPunchTrigger.SetActive(true);
            _nppPunchCD = true;
        }
    }

    public void OnGrab()
    {
        if (inputDisabled || _grabCD || _punchCD || _nppPunchCD) return;
        FlipTrigger(_GrabTrigger.transform);
        _GrabTrigger.SetActive(true);
    }

    public void OnRage()
    {
        if (inputDisabled || _currentHatred < 100) return;
        _hating = true;
        _anim.SetTrigger("transform");
        inputDisabled = true;
        RuntimeManager.PlayOneShot(_SFXTransform);
    }

    public void OnThrow()
    {
        if (inputDisabled || !_nppGrabbed || !_hating) return;
        var obj = Instantiate(_HeadPrefab, transform.position, Quaternion.identity);
        PonyHeadController prj = obj.GetComponent<PonyHeadController>();
        RuntimeManager.PlayOneShot(_SFXGrab);
        if (_direction.magnitude == 0)
        {
            prj.dir = _spriteRenderer.flipX ? Vector2.right : Vector2.left;
        }
        else
        {
            prj.dir = _direction;
        }
        prj.player = this;
        nppGrabbed = false;
        _anim.SetTrigger("throw");
        // TODO throw pony animation
    }

    public void OnDash()
    {
        if (inputDisabled || _dashCD || _punchCD || _nppPunchCD || _hating) return;
        _dashing = true;
        _dashCD = true;
        RuntimeManager.PlayOneShot(_SFXRainbowDash);
        if (_direction.magnitude == 0)
        {
            _dashDir = _spriteRenderer.flipX ? Vector2.right : Vector2.left;
        }
        else
        {
            _dashDir = _direction;
        }
        StartCoroutine(DashTimer());
        _anim.SetBool("dash", true);
    }

    #endregion

    IEnumerator DashTimer()
    {
        yield return new WaitForSeconds(_dashTime);
        _dashing = false;
        _anim.SetBool("dash", false);
        yield return new WaitForSeconds(_dashCDTime);
        _dashCD = false;
    }

    private void Dash()
    {
        float speed = _hating ? _plowingDashSpeed : _dashSpeed;

        Vector3 movement = new Vector3(_dashDir.x, 0, _dashDir.y) * speed * Time.deltaTime;

        if (_direction.x > 0)
        {
            _spriteRenderer.flipX = true;
        }
        else if (_direction.x < 0)
        {
            _spriteRenderer.flipX = false;
        }
        transform.Translate(Vector3.Scale(movement, IsOutOfBounds(new Vector3(transform.position.x + movement.x, transform.position.z + movement.z))), Space.World);
    }

    private void MovePlayer()
    {
        Vector3 movement = new Vector3(_direction.x, 0, _direction.y) * _currentSpeed * Time.deltaTime;

        if (_direction.x > 0)
        {
            _spriteRenderer.flipX = true;
        }
        else if (_direction.x < 0)
        {
            _spriteRenderer.flipX = false;
        }

        _anim.SetFloat("velocity", movement.magnitude);

        transform.Translate(Vector3.Scale(movement, IsOutOfBounds(new Vector3(transform.position.x + movement.x, transform.position.z + movement.z))), Space.World);
    }

    private Vector3 IsOutOfBounds(Vector2 nextPos)
    {
        Vector3 multiplier = new Vector3(1, 1, 1);
        Vector2 dist = new Vector2(Mathf.Abs(nextPos.x - _arenaTransform.position.x), Mathf.Abs(nextPos.y - _arenaTransform.position.z));

        Vector3 scaler = _arenaScale; //_arenaTransform.localScale;

        float yScale = scaler.z / scaler.x;
        dist.y *= 1 / yScale;

        if (dist.magnitude > scaler.x / 2)
        {
            dist = new Vector2(Mathf.Abs(nextPos.x - _arenaTransform.position.x), Mathf.Abs(transform.position.z - _arenaTransform.position.z) / yScale);
            if (dist.magnitude > scaler.x / 0.2f) multiplier.x *= 0;

            dist = new Vector2(Mathf.Abs(transform.position.x - _arenaTransform.position.x), Mathf.Abs(nextPos.y - _arenaTransform.position.z) / yScale);
            if (dist.magnitude > scaler.x / 0.2f) multiplier.z *= 0;
        }

        return multiplier;
    }


    // normalized from 0 to 1
    private Vector2 GetPositonOnTexture()
    {
        Vector3 pos = Vector3.zero;

        Vector3 scaler = _arenaScale;

        // local position on arena
        pos = transform.position - _arenaTransform.position;

        // put origin in corner
        pos += new Vector3(scaler.x / 0.2f, 0, scaler.z / 0.2f);

        Vector2 pos2 = new Vector2(pos.x / (scaler.x * 10), pos.z / (scaler.z * 10));

        pos2.y = 1 - pos2.y;

        return pos2;
    }

    private void SetOrderInLayer()
    {
        float order = 0;
        // remap
        float zPos = _arenaTransform.localScale.z / 0.1f - (transform.position.z + _arenaTransform.localScale.z / 0.2f);
        order = zPos * 10 + 5;
        _spriteRenderer.sortingOrder = (int)order;
    }

    private void FlipTrigger(Transform trigger)
    {
        BoxCollider col = trigger.GetComponent<BoxCollider>();
        if (!_spriteRenderer.flipX)
        {
            trigger.localScale = new Vector3(-1, 1, 1);
            if (col.size.x > 0) col.size = Vector3.Scale(col.size, new Vector3(-1, 1, 1));
        }
        else
        {
            trigger.localScale = Vector3.one;
            if (col.size.x < 0) col.size = Vector3.Scale(col.size, new Vector3(-1, 1, 1));
        }
    }

    public void PunchWithNPPHit(NPPController npp)
    {
        npp.GetHit(_nppPunchDamage, this);
    }

    public void PunchHit(NPPController npp)
    {
        npp.GetHit(_punchDamage, this);
    }

    public void PunchWithNPPHit(PlayerController pp)
    {
        pp.GetHit();
    }

    public void PunchHit(PlayerController pp)
    {
        pp.GetHit();
    }

    public void GrabHit(NPPController npp)
    {
        if (!_nppGrabbed)
        {
            nppGrabbed = true;
            npp.GetGrabbed();
            _nppDurability = npp.HP;
            RuntimeManager.PlayOneShot(_SFXGrab);
        }
        _GrabTrigger.SetActive(false);
        ResetCDs();
    }

    public void AddHatred()
    {
        RuntimeManager.PlayOneShot(_SFXKill);
        if (_hating) return;
        _currentHatred += _hatredPerNPPKill;
        if (_currentHatred > 100) _currentHatred = 100;

        foreach (Image bar in _hatredBars)
        {
            bar.fillAmount = (float)_currentHatred / 100;
        }
    }

    public void RemoveHatred()
    {
        _currentHatred -= _hatredLossPerPPHit;
        foreach (Image bar in _hatredBars)
        {
            bar.fillAmount = (float)_currentHatred / 100;
        }
    }

    public void GetHit()
    {
        if (_hating) return;
        _punchCD = false;
        _nppPunchCD = false;
        RemoveHatred();
        _anim.SetTrigger("hit");
        StartCoroutine(GotHitStun());
        int enemyIndex = playerIndex == 0 ? 1 : 0;
        GlobalGameManager.Instance.splatterManager.DrawSplatter(GetPositonOnTexture(), enemyIndex, SplatterManager.SplatterType.medium);
    }

    IEnumerator GotHitStun()
    {
        inputDisabled = true;
        yield return new WaitForSeconds(0.3f);
        inputDisabled = false;
    }

    IEnumerator HatredbarDecay()
    {
        while (_currentHatred > 0)
        {
            _currentHatred -= 100 * Time.deltaTime / _hatredTime;
            foreach (Image bar in _hatredBars)
            {
                bar.fillAmount = (float)_currentHatred / 100;
            }
            yield return null;
        }
        GlobalGameManager.Instance.ExitHatred(this);
        _hating = false;
    }

    public void TransformToGore()
    {
        Animator _goreAnim = _GoreVisual.GetComponent<Animator>();
        _GoreVisual.SetActive(true);
        SwapAnimator(_anim, _goreAnim);
        _GlitterVisual.SetActive(false);
        _anim = _goreAnim;
        _spriteRenderer = _GoreVisual.GetComponent<SpriteRenderer>();

        ResetCDs();
    }

    public void OnGoreEnter()
    {
        _hatredBars[playerIndex].transform.parent.gameObject.SetActive(false);
        _hatredBars[2].transform.parent.gameObject.SetActive(true);
    }

    public void TransformToGlitter()
    {
        Animator _glitterAnim = _GlitterVisual.GetComponent<Animator>();
        _GlitterVisual.SetActive(true);
        SwapAnimator(_anim, _glitterAnim);
        _GoreVisual.SetActive(false);
        _anim = _glitterAnim;
        _spriteRenderer = _GlitterVisual.GetComponent<SpriteRenderer>();

        ResetCDs();
    }

    public void OnGlitterEnter()
    {
        _hatredBars[playerIndex].transform.parent.gameObject.SetActive(true);
        _hatredBars[2].transform.parent.gameObject.SetActive(false);
    }

    private void SwapAnimator(Animator oldAnim, Animator newAnim)
    {
        newAnim.SetBool("dash", oldAnim.GetBool("dash"));
        newAnim.SetBool("hasNPP", oldAnim.GetBool("hasNPP"));
    }

    private void ResetCDs()
    {
        punchCD = false;
        nppPunchCD = false;
        _dashCD = false;
        _dashing = false;
    }

    public void TransformAnimComplete()
    {
        StartCoroutine(HatredbarDecay());
        GlobalGameManager.Instance.EnterHatred(this);
        inputDisabled = false;
    }

    public void Spawned()
    {
        playerManager.PlayerSpawned();
    }

    private void GameOver()
    {
        GlobalGameManager.Instance.OnGameOver -= GameOver;
        GlobalGameManager.Instance.OnGore -= OnGoreEnter;
        GlobalGameManager.Instance.OnGlitter -= OnGlitterEnter;
        _gameOver = true;
        inputDisabled = true;
        if (_hating) GlobalGameManager.Instance.ExitHatred(this);
        foreach (Image bar in _hatredBars)
        {
            bar.transform.parent.gameObject.SetActive(false);
        }
        StartCoroutine(ReturnToMenu());
    }

    IEnumerator ReturnToMenu()
    {
        yield return new WaitForSeconds(3);

        while (true)
        {
            if (Input.anyKeyDown)
            {
                GlobalGameManager.Instance.LoadMainMenu();
            }
            yield return null;
        }
    }
}
