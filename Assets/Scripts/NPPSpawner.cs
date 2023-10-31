using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

public class NPPSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _NPPPrefab;

    [SerializeField] private int _NPPAmount;

    private Transform _arena;

    [SerializeField] private Transform _NPPParent;

    [SerializeField] private List<Transform> _glitterSpawnPoints = new List<Transform>();
    [SerializeField] private List<Transform> _goreSpawnPoints = new List<Transform>();

    private List<GameObject> _alivePonies = new List<GameObject>();
    private List<GameObject> _deadPonies = new List<GameObject>();

    private void Awake()
    {
        _arena = GameObject.FindGameObjectWithTag("Arena").GetComponent<Transform>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SpawnStartNPPs();
    }

    // Update is called once per frame
    void Update()
    {
        RespawnNPP();
    }

    private void SpawnStartNPPs()
    {
        NavMeshHit hit;
        bool foundPosition;

        for (int i = 0; i < _NPPAmount; i++)
        {
            foundPosition = NavMesh.SamplePosition(_arena.position + Random.insideUnitSphere * _arena.localScale.x * 5, out hit, Mathf.Infinity, 1);
            if (foundPosition)
            {
                GameObject pony = Instantiate(_NPPPrefab, hit.position, Quaternion.identity, _NPPParent);
                _alivePonies.Add(pony);
                pony.GetComponent<NPPController>().spawner = this;
            }
        }
    }

    private void RespawnNPP()
    {
        int nppsToSpawn = _deadPonies.Count;
        for (int i = 0; i < nppsToSpawn; i++)
        {
            UnkillPony();
        }
    }

    public void KillPony(GameObject pony)
    {
        _alivePonies.Remove(pony);
        _deadPonies.Add(pony);
        pony.SetActive(false);
    }

    private void UnkillPony()
    {
        GameObject pony = _deadPonies[0];
        _deadPonies.Remove(pony);
        _alivePonies.Remove(pony);

        if(GlobalGameManager.Instance.haters > 0)
        {
            pony.transform.position = _goreSpawnPoints[Random.Range(0, _goreSpawnPoints.Count)].position;
        }
        else
        {
            pony.transform.position = _glitterSpawnPoints[Random.Range(0, _goreSpawnPoints.Count)].position;
        }        
        
        pony.SetActive(true);
        pony.GetComponent<NPPController>().SetRandomDestination();
    }
}
