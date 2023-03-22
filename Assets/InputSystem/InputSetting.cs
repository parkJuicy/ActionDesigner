using UnityEngine;
using System;
using System.Collections.Generic;

namespace InputSystem
{
    [Serializable]
    public class Key
    {
        [SerializeField]
        private string _name;
        [SerializeField]
        private KeyCode _code;

        public string Name { get => _name;}
        public KeyCode Code { get => _code;}
    }

    [Serializable]
    public class AxisKey
    {
        [SerializeField]
        private string _name;
        [SerializeField]
        private KeyCode _postiveCode;
        [SerializeField]
        private KeyCode _negativeCode;

        public string Name { get => _name; }
        public KeyCode PostiveCode { get => _postiveCode; }
        public KeyCode NegativeCode { get => _negativeCode; }
    }

    public class InputSetting : ScriptableObject
    {
        [SerializeField]
        private List<Key> _key;

        [SerializeField]
        private List<AxisKey> _axisKey;

        public List<Key> Key { get => _key; }
        public List<AxisKey> AxisKey { get => _axisKey; }
    }
}
