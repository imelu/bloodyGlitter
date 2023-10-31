using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPAnimEvents : MonoBehaviour
{
    private PlayerController _ppc;

    private void Awake()
    {
        _ppc = GetComponentInParent<PlayerController>();
    }

    public void PunchComplete()
    {
        _ppc.punchCD = false;
        _ppc.nppPunchCD = false;
    }

    public void TransformComplete()
    {
        _ppc.TransformAnimComplete();
    }

    public void Spawned()
    {
        _ppc.Spawned();
    }
}
