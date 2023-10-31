using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using System;
using FMOD.Studio;

public class MainMenuManager : MonoBehaviour
{
    private int _selectedIndex = 0;

    [SerializeField] private List<Image> _buttons = new List<Image>();

    [SerializeField] private EventReference _StartEvent;

    [SerializeField] private List<Image> _hoverGlitter1 = new List<Image>();
    [SerializeField] private List<Image> _hoverGlitter2 = new List<Image>();

    [SerializeField] private GameObject _controls;

    private void Start()
    {
        HoverButton(0);
    }

    private void Update()
    {
        if (Input.anyKeyDown) GlobalGameManager.Instance.StartMusic();
    }

    public void OnUp()
    {
        if (_controls.activeInHierarchy) return;
        StopHoverButton(_selectedIndex);
        //_selectedIndex = _selectedIndex == 0 ? 1 : 0;
        _selectedIndex++;
        if (_selectedIndex >= _buttons.Count) _selectedIndex = 0;
        HoverButton(_selectedIndex);
    }

    public void OnDown()
    {
        if (_controls.activeInHierarchy) return;
        StopHoverButton(_selectedIndex);
        //_selectedIndex = _selectedIndex == 0 ? 1 : 0;
        _selectedIndex--;
        if(_selectedIndex < 0) _selectedIndex = _buttons.Count - 1;
        HoverButton(_selectedIndex);
    }

    public void OnConfirm()
    {
        ConfirmButton(_selectedIndex);
    }

    private void StopHoverButton(int index)
    {
        DOTween.KillAll();
        _hoverGlitter1[index].color = Color.white * new Color(1, 1, 1, 0);
        _hoverGlitter2[index].color = Color.white * new Color(1, 1, 1, 0);
    }

    private void HoverButton(int index)
    {
        _buttons[index].transform.DOPunchScale(Vector3.one * 0.2f, 0.1f, 10, 1);
        _hoverGlitter1[index].DOColor(Color.white, 1.3f).SetLoops(-1, LoopType.Yoyo);
        _hoverGlitter2[index].color = Color.white;
        _hoverGlitter2[index].DOColor(Color.white * new Color(1, 1, 1, 0), 1f).SetLoops(-1, LoopType.Yoyo);
    }

    private void ConfirmButton(int index)
    {
        switch (index)
        {
            case 0:
                RuntimeManager.PlayOneShot(_StartEvent);
                _buttons[index].transform.DOPunchScale(-Vector3.one * 0.2f, 0.1f, 10, 1).OnComplete(() => StartGame());
                break;

            case 1:
                if (_controls.activeInHierarchy) _controls.SetActive(false);
                else _controls.SetActive(true);
                break;

            case 2:
                _buttons[index].transform.DOPunchScale(-Vector3.one * 0.2f, 0.1f, 10, 1).OnComplete(()=> Application.Quit());
                break;
        }
    }

    private void StartGame()
    {
        DOTween.KillAll();
        GlobalGameManager.Instance.LoadGameScene();
    }
}
