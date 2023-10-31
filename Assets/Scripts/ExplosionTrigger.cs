using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class ExplosionTrigger : MonoBehaviour
{
    private int _dmg;
    public int dmg { get { return _dmg; } set { _dmg = value; } }

    private PlayerController _player;
    public PlayerController player { get { return _player; } set { _player = value; } }

    [SerializeField] private EventReference _SFXGlitterPunch;

    private void Start()
    {
        RuntimeManager.PlayOneShot(_SFXGlitterPunch);
        StartCoroutine(DestroyExplosion());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPP"))
        {
            NPPController npp = other.GetComponent<NPPController>();
            npp.GetHit(dmg, player);
        }

        if (other.CompareTag("PP"))
        {
            PlayerController pp = other.GetComponent<PlayerController>();
            if (pp == player) return;
            pp.GetHit();
        }
    }

    IEnumerator DestroyExplosion()
    {
        yield return new WaitForSeconds(0.2f);
        GetComponent<SphereCollider>().enabled = false;
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }
}
