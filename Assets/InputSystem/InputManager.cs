using System;
using System.Collections.Generic;
using UnityEngine;

namespace InputSystem
{
    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance;
        public static InputManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<InputManager>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject(typeof(InputManager).Name);
                        _instance = obj.AddComponent<InputManager>();
                    }
                    return _instance;
                }
                return _instance;
            }
        }

        private InputSetting _inputSetting;

        public Dictionary<KeyName, KeyInput> KeyInputs { get; private set; }
        public Dictionary<AxisKeyName, AxisKeyInput> AxisKeyInputs { get; private set; }

        private void Awake()
        {
            _inputSetting = Resources.Load<InputSetting>(InputSystemSetting.Path);
            if (_inputSetting == null)
            {
                Debug.LogError("Input System을 설정하지 않았습니다");
                return;
            }
            BindKey();
        }

        private void BindKey()
        {
            KeyInputs = new Dictionary<KeyName, KeyInput>();
            foreach (var keyButton in _inputSetting.Key)
            {
                KeyName name = EnumMapper.GetEnumType<KeyName>(keyButton.Name);
                KeyInputs[name] = new KeyInput(keyButton.Code);
            }

            AxisKeyInputs = new Dictionary<AxisKeyName, AxisKeyInput>();
            foreach (var keyButton in _inputSetting.AxisKey)
            {
                AxisKeyName name = EnumMapper.GetEnumType<AxisKeyName>(keyButton.Name);
                AxisKeyInputs[name] = new AxisKeyInput(keyButton.PostiveCode, keyButton.NegativeCode);
            }
        }

        private void Update()
        {
            foreach (var key in KeyInputs.Values)
            {
                key.IsKeyPressed = Input.GetKey(key.Code);
                key.IsKeyDown = Input.GetKeyDown(key.Code);
                key.IsKeyUp = Input.GetKeyUp(key.Code);
            }

            foreach (var axisKey in AxisKeyInputs.Values)
            {
                if(Input.GetKey(axisKey.NegativeCode) && !Input.GetKey(axisKey.PostiveCode))
                {
                    axisKey.Value = -1;
                }
                else if(!Input.GetKey(axisKey.NegativeCode) && Input.GetKey(axisKey.PostiveCode))
                {
                    axisKey.Value = 1;
                }
                else
                {
                    axisKey.Value = 0;
                }
            }
        }

        public KeyInput GetKey(KeyName name)
        {
            return KeyInputs[name];
        }
        
        public float GetAxisKey(AxisKeyName name)
        {
            return AxisKeyInputs[name].Value;
        }
    }
}