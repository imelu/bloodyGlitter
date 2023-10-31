using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityTrigger : MonoBehaviour
{
    public enum AbilityType
    {
        punch,
        grab,
        punchWithNPP
    }

    [SerializeField] private AbilityType type;

    private PlayerController _player;

    private void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPP"))
        {
            switch (type)
            {
                case AbilityType.punch:
                    _player.PunchHit(other.GetComponent<NPPController>());
                    break;

                case AbilityType.grab:
                    _player.GrabHit(other.GetComponent<NPPController>());
                    break;

                case AbilityType.punchWithNPP:
                    _player.PunchWithNPPHit(other.GetComponent<NPPController>());
                    break;
            }
        }

        if (other.CompareTag("PP"))
        {
            PlayerController pp = other.GetComponent<PlayerController>();
            if (pp == _player) return;
            switch (type)
            {
                case AbilityType.punch:
                    _player.PunchHit(pp);
                    break;

                case AbilityType.punchWithNPP:
                    _player.PunchWithNPPHit(pp);
                    break;
            }
        }
    }
}
