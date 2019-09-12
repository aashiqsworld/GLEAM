using UnityEngine;
using System.Collections;
//using UnityStandardAssets.CinematicEffects;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

namespace UB
{
    public class EffectsGui : ExecuteOnMainThread
    {

        public GameObject Cam;
        public UnityEngine.UI.Text Text;
        private int _selectedShader = 0;
        private EffectBase[] _behaviours;

        public void Start()
        {
            Application.targetFrameRate = 300;

            _behaviours = Cam.GetComponents<EffectBase>();
            //foreach (var behaviour in _behaviours)
            //{
            //    Debug.Log(behaviour.name);
            //}
        }

        public void ChangeEffect()
        {
            foreach (var behavior in _behaviours)
            {
                behavior.enabled = false;
            }
            _behaviours[_selectedShader].enabled = true;
            Text.text = _behaviours[_selectedShader].name;
            _selectedShader++;
            if (_selectedShader >= _behaviours.Length)
            {
                _selectedShader = 0;
            }
        }

        public void LoadShaderScene()
        {
            SceneManager.LoadScene("Shaders");
        }

        public void LoadEffectsScene()
        {
            SceneManager.LoadScene("PostEffects");
        }
    }
}
