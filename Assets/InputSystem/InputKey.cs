using UnityEngine;

namespace InputSystem
{
    public class KeyInput
    {
        public bool IsKeyPressed { get; set; }
        public bool IsKeyUp { get; set; }
        public bool IsKeyDown { get; set; }
        public KeyCode Code { get; set; }

        public KeyInput(KeyCode code)
        {
            Code = code;
        }
    }

    public class AxisKeyInput
    {
        public float Value { get; set; }

        public KeyCode PostiveCode;
        public KeyCode NegativeCode;

        public AxisKeyInput(KeyCode postive, KeyCode negative)
        {
            PostiveCode = postive;
            NegativeCode = negative;
        }
    }
}