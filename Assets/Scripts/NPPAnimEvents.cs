using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPPAnimEvents : MonoBehaviour
{
    private NPPController _npp;

    private void Awake()
    {
        _npp = GetComponentInParent<NPPController>();    
    }

    public void Died()
    {
        _npp.RemovePony();
    }
}
