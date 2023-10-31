using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class NPPController : MonoBehaviour
{
    private Transform _arena;

    private SpriteRenderer _spriteRenderer;

    private NavMeshAgent _agent;

    private Animator _anim;

    private NPPSpawner _spawner;
    public NPPSpawner spawner { get { return _spawner; } set { _spawner = value; } }

    [SerializeField] private float _stoppingDist;

    [SerializeField] private int _maxHP;
    private int _HP;
    public int HP { get { return _HP; } }

    [SerializeField] private Vector3 _arenaScale;

    [SerializeField] private GameObject _deathParticle;

    // Start is called before the first frame update
    void Awake()
    {
        _arena = GameObject.FindGameObjectWithTag("Arena").GetComponent<Transform>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _anim = GetComponentInChildren<Animator>();
        _HP = _maxHP;
        SetRandomDestination();
    }

    // Update is called once per frame
    void Update()
    {
        SetOrderInLayer();
        CheckGoalReached();
        FlipSprite();
    }

    private void FlipSprite()
    {
        if(_agent.velocity.x > 0) _spriteRenderer.flipX = false;
        else _spriteRenderer.flipX = true;
    }

    private void SetOrderInLayer()
    {
        float order = 0;
        // remap
        float zPos = _arena.localScale.z / 0.1f - (transform.position.z + _arena.localScale.z / 0.2f);
        order = zPos + 5;
        order = zPos * 10 + 5;
        _spriteRenderer.sortingOrder = (int)order;
    }

    public void SetRandomDestination()
    {
        NavMeshHit hit;
        bool foundPosition;
        foundPosition = NavMesh.SamplePosition(_arena.position + Random.insideUnitSphere * _arena.localScale.x * 5, out hit, Mathf.Infinity, 1);
        if (foundPosition)
        {
            _agent.SetDestination(hit.position);
            //if(_anim == null) _anim = GetComponentInChildren<Animator>();
            _anim.SetBool("isMoving", true);
        } 
    }

    private void CheckGoalReached()
    {
        if(!_agent.pathPending && Vector3.Distance(_agent.destination, transform.position) < _stoppingDist && _agent.hasPath)
        {
            _agent.ResetPath();
            StartCoroutine(IdleTime(Random.Range(2f, 5f)));
            _anim.SetBool("isMoving", false);
        }
    }

    IEnumerator IdleTime(float idleTime)
    {
        yield return new WaitForSeconds(idleTime);
        SetRandomDestination();
    }

    public void GetHit(int dmg, PlayerController player)
    {
        _HP -= dmg;
        if (_HP <= 0) Die(player);
        else _anim.SetTrigger("hit");
    }

    private void Die(PlayerController player)
    {
        GlobalGameManager.Instance.splatterManager.DrawSplatter(GetPositonOnTexture(), player.playerIndex, SplatterManager.SplatterType.small);
        player.AddHatred();
        _anim.SetTrigger("die");
        _deathParticle.SetActive(true);
        //Destroy(gameObject);
    }

    public void RemovePony()
    {
        _deathParticle.SetActive(false);
        spawner.KillPony(gameObject);
    }

    private Vector2 GetPositonOnTexture()
    {
        Vector3 pos = Vector3.zero;

        Vector3 scaler = _arenaScale;

        // local position on arena
        pos = transform.position - _arena.position;

        // put origin in corner
        pos += new Vector3(scaler.x / 0.2f, 0, scaler.z / 0.2f);

        Vector2 pos2 = new Vector2(pos.x / (scaler.x * 10), pos.z / (scaler.z * 10));

        pos2.y = 1 - pos2.y;

        return pos2;
    }

    public void GetGrabbed()
    {
        RemovePony();
    }
}
