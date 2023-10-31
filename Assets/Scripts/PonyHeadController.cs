using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PonyHeadController : MonoBehaviour
{
    private Vector2 _dir;
    public Vector2 dir { get { return _dir; } set { _dir = value; } }

    [SerializeField] private float _speed;

    private SpriteRenderer _spriteRenderer;

    [SerializeField] private GameObject _explosion;

    [SerializeField] private int _impactDamage;
    [SerializeField] private int _explosionDamage;
    [SerializeField] private float _lifetime;

    private PlayerController _player;
    public PlayerController player { get { return _player; } set { _player = value; } }

    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(Lifetime());
    }

    void Update()
    {
        MoveHead();
    }

    private void MoveHead()
    {
        Vector3 movement = new Vector3(_dir.x, 0, _dir.y) * _speed * Time.deltaTime;

        if (_dir.x > 0)
        {
            _spriteRenderer.flipX = true;
        }
        else if (_dir.x < 0)
        {
            _spriteRenderer.flipX = false;
        }

        transform.Translate(movement, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPP"))
        {
            NPPController npp = other.GetComponent<NPPController>();

            npp.GetHit(_impactDamage, player);

            var obj = Instantiate(_explosion, transform.position, Quaternion.identity);
            ExplosionTrigger expl = obj.GetComponent<ExplosionTrigger>();
            expl.dmg = _explosionDamage;
            expl.player = player;

            Destroy(gameObject);
        }

        if (other.CompareTag("PP"))
        {
            PlayerController pp = other.GetComponent<PlayerController>();
            if (pp == player) return;

            pp.GetHit();

            var obj = Instantiate(_explosion, transform.position, Quaternion.identity);
            ExplosionTrigger expl = obj.GetComponent<ExplosionTrigger>();
            expl.dmg = _explosionDamage;
            expl.player = player;

            Destroy(gameObject);
        }
    }

    IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(_lifetime);

        var obj = Instantiate(_explosion, transform.position, Quaternion.identity);
        ExplosionTrigger expl = obj.GetComponent<ExplosionTrigger>();
        expl.dmg = _explosionDamage;
        expl.player = player;

        Destroy(gameObject);
    }
}
