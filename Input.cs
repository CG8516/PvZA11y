using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.XInput;
using static PvZA11y.Program;
using System.Collections.Immutable;
using System.Xml.Xsl;
using System.Text.Json.Serialization;

namespace PvZA11y
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum InputIntent
    {
        None,       //Do nothing
        Up,
        Down,
        Left,
        Right,
        Confirm,    //A
        Deny,       //B
        Start,      //Start
        Option,     //Select
        CycleLeft,  //Lb
        CycleRight, //Rb
        ZombieMinus,//Lt
        ZombiePlus, //Rt
        Info1,      //X
        Info2,      //Y
        Info3,      //L3
        Info4,      //R3
        Slot1,      //Right-Stick / number keys
        Slot2,
        Slot3,
        Slot4,
        Slot5,
        Slot6,
        Slot7,
        Slot8,
        Slot9,
        Slot10,
    }

    public class Input
    {
        bool[] heldKeys = new bool[0xff + 1]; //Array of all currently-held keys
        State prevState;

        int prevRightSlot = -1;

        Dictionary<uint, InputIntent> keyBinds = new Dictionary<uint, InputIntent>();
        Dictionary<GamepadButtons, InputIntent> controllerBinds = new Dictionary<GamepadButtons, InputIntent>();

        bool keybindsChanged = false;
        bool running = true;

        short xThreshold = 20000;
        short yThreshold = 20000;

        int triggerThreshold = 250;

        ConcurrentQueue<InputIntent> inputQueue = new ConcurrentQueue<InputIntent>();


        //TODO: Make these options user-customisable in accessibility menu
        int initialKeyRepeatDelay = 400;    //Delay before keys start being repeated
        int keyRepeatDelay = 100;   //Delay between key repetitions, after the initial timer has been exceeded

        //TODO: Make this a bit nicer
        Dictionary<InputIntent, long> keyRepeatTimers = new Dictionary<InputIntent, long>() { { InputIntent.Up, 0 }, { InputIntent.Down, 0 }, { InputIntent.Left, 0 }, { InputIntent.Right, 0 }, { InputIntent.CycleLeft, 0 }, { InputIntent.CycleRight, 0 }, { InputIntent.ZombieMinus, 0 }, { InputIntent.ZombiePlus, 0 } };

        public Input()
        {
            bool controllerBindsOk = true;
            if (Config.current.controllerBinds != null)
            {
                foreach (InputIntent intent in Enum.GetValues(typeof(InputIntent)))
                {
                    if (intent is InputIntent.None or (>= InputIntent.Up and <= InputIntent.Right) or InputIntent.ZombieMinus or InputIntent.ZombiePlus or (>= InputIntent.Slot1 and <= InputIntent.Slot10))
                        continue;
                    if (!Config.current.controllerBinds.ContainsValue(intent))
                    {
                        controllerBindsOk = false;
                        break;
                    }
                }

            }
            else
                controllerBindsOk = false;

            if (controllerBindsOk)
                controllerBinds = Config.current.controllerBinds;
            else
            {
                controllerBinds = new Dictionary<GamepadButtons, InputIntent>();

                controllerBinds.Add(GamepadButtons.DPadUp, InputIntent.Up);
                controllerBinds.Add(GamepadButtons.DPadDown, InputIntent.Down);
                controllerBinds.Add(GamepadButtons.DPadLeft, InputIntent.Left);
                controllerBinds.Add(GamepadButtons.DPadRight, InputIntent.Right);

                controllerBinds.Add(GamepadButtons.A, InputIntent.Confirm);
                controllerBinds.Add(GamepadButtons.B, InputIntent.Deny);

                controllerBinds.Add(GamepadButtons.Start, InputIntent.Start);
                controllerBinds.Add(GamepadButtons.Back, InputIntent.Option);

                controllerBinds.Add(GamepadButtons.LeftShoulder, InputIntent.CycleLeft);
                controllerBinds.Add(GamepadButtons.RightShoulder, InputIntent.CycleRight);

                controllerBinds.Add(GamepadButtons.X, InputIntent.Info1);
                controllerBinds.Add(GamepadButtons.Y, InputIntent.Info2);
                controllerBinds.Add(GamepadButtons.LeftThumb, InputIntent.Info3);
                controllerBinds.Add(GamepadButtons.RightThumb, InputIntent.Info4);

                Config.current.controllerBinds = controllerBinds;
                Config.SaveConfig();
            }

            bool keyboardBindsOk = true;

            if (Config.current.keyBinds != null)
            {
                foreach (InputIntent intent in Enum.GetValues(typeof(InputIntent)))
                {
                    if (intent is InputIntent.None)
                        continue;
                    if (!Config.current.keyBinds.ContainsValue(intent))
                    {
                        keyboardBindsOk = false;
                        break;
                    }
                }

            }
            else
                keyboardBindsOk = false;

            foreach(var pair in Config.current.keyBinds)
            {
                if(pair.Key >= 0xff)
                {
                    keyboardBindsOk = false;
                    break;
                }
            }    

            if (keyboardBindsOk)
                keyBinds = Config.current.keyBinds;
            else
            {
                keyBinds = new Dictionary<uint, InputIntent>();

                keyBinds.Add(Key.Up, InputIntent.Up);
                keyBinds.Add(Key.Down, InputIntent.Down);
                keyBinds.Add(Key.Left, InputIntent.Left);
                keyBinds.Add(Key.Right, InputIntent.Right);

                keyBinds.Add(Key.Return, InputIntent.Confirm);
                keyBinds.Add(Key.Back, InputIntent.Deny);

                keyBinds.Add(Key.Escape, InputIntent.Start);
                keyBinds.Add(Key.Tab, InputIntent.Option);

                keyBinds.Add(Key.Minus, InputIntent.CycleLeft);
                keyBinds.Add(Key.Plus, InputIntent.CycleRight);

                keyBinds.Add(Key.F1, InputIntent.Info1);
                keyBinds.Add(Key.F2, InputIntent.Info2);
                keyBinds.Add(Key.F3, InputIntent.Info3);
                keyBinds.Add(Key.F4, InputIntent.Info4);

                //Really need to improve input rebinding menu
                keyBinds.Add((uint)Key.One, InputIntent.Slot1);
                keyBinds.Add((uint)Key.Two, InputIntent.Slot2);
                keyBinds.Add((uint)Key.Three, InputIntent.Slot3);
                keyBinds.Add((uint)Key.Four, InputIntent.Slot4);
                keyBinds.Add((uint)Key.Five, InputIntent.Slot5);
                keyBinds.Add((uint)Key.Six, InputIntent.Slot6);
                keyBinds.Add((uint)Key.Seven, InputIntent.Slot7);
                keyBinds.Add((uint)Key.Eight, InputIntent.Slot8);
                keyBinds.Add((uint)Key.Nine, InputIntent.Slot9);
                keyBinds.Add((uint)Key.Zero, InputIntent.Slot10);

                keyBinds.Add((uint)Key.Comma, InputIntent.ZombieMinus);
                keyBinds.Add((uint)Key.Period, InputIntent.ZombiePlus);

                Config.current.keyBinds = keyBinds;
                Config.SaveConfig();
            }

            Task.Run(InputScanThread);
        }

        public InputIntent GetCurrentIntent()
        {
            if(inputQueue.TryDequeue(out var intent))
                return intent;

            return InputIntent.None;
        }

        private void InputScanThread()
        {
            while (running)
            {
                var iControllerBinds = controllerBinds.ToImmutableDictionary();
                var iKeyBinds = keyBinds.ToImmutableDictionary();
                keybindsChanged = false;
                inputQueue.Clear(); //Clear intents if keybinds have changed

                while (!keybindsChanged)
                {
                    var intents = GetCurrentIntents(iKeyBinds,iControllerBinds);
                    foreach (var intent in intents)
                    {
                        //Try to avoid queuing up too many unprocessed inputs
                        //Without this, it was possible to queue thousands of inputs by holding a few directional inputs at the same time
                        if (inputQueue.Count < 8)
                            inputQueue.Enqueue(intent);
                    }

                    Thread.Sleep(5);    //Just a little delay, to avoid scanning billions of times per second
                }
            }

        }

        public void ClearIntents()
        {
            inputQueue.Clear();
        }

        public GamepadButtons GetButton()
        {
            State state;

            //Wait until no buttons are being pressed, then return first button pressed after that
            bool buttonPressed = true;
            while (buttonPressed)
            {
                if (XInput.GetState(0, out state))
                {
                    buttonPressed = false;
                    foreach (GamepadButtons button in Enum.GetValues(typeof(GamepadButtons)))
                    {
                        if (button is GamepadButtons.None)
                            continue;
                        if (state.Gamepad.Buttons.HasFlag(button))
                            buttonPressed = true;
                    }
                }
                else
                    return GamepadButtons.None;
            }

            while (true)
            {
                if (XInput.GetState(0, out state))
                {
                    foreach (GamepadButtons button in Enum.GetValues(typeof(GamepadButtons)))
                    {
                        if (button is GamepadButtons.None)
                            continue;
                        if (state.Gamepad.Buttons.HasFlag(button))
                            return button;
                    }

                    if (NativeKeyboard.IsKeyDown(Key.Escape))
                        return GamepadButtons.None;

                }
                else
                    return GamepadButtons.None;
            }
        }

        public void WaitForNoInput()
        {
            ClearIntents();
            bool buttonPressed = true;
            while (buttonPressed)
            {
                buttonPressed = false;
                for (uint i = 1; i < 0xff; i++)
                {
                    if (NativeKeyboard.IsKeyDown(i))
                        buttonPressed = true;
                }
                if (XInput.GetState(0, out State state))
                {
                    foreach (GamepadButtons button in Enum.GetValues(typeof(GamepadButtons)))
                    {
                        if (button is GamepadButtons.None)
                            continue;
                        if (state.Gamepad.Buttons.HasFlag(button))
                        {
                            buttonPressed = true;
                            break;
                        }
                    }
                }
            }
            ClearIntents();
        }

        public uint GetKey(bool blocking = true)
        {
            //Wait until no keys are being pressed, then return first key pressed after
            bool buttonPressed = true;
            while(buttonPressed)
            {
                buttonPressed = false;
                for(uint i = 1; i < 0xff; i++)
                {
                    if (NativeKeyboard.IsKeyDown(i))
                        buttonPressed = true;
                }
                if (!blocking && buttonPressed)
                    return 0;
            }

            while(true)
            {
                for (uint i = 1; i < 0xff; i++)
                {
                    if (NativeKeyboard.IsKeyDown(i))
                        return i;
                }
                if (!blocking)
                    return 0;
            }

        }

        public void GetKeyOrButton(ref uint pressedKey, ref GamepadButtons pressedButton, bool allowKeyboard = true, bool allowController = true)
        {
            pressedKey = 0;
            pressedButton = GamepadButtons.None;

            WaitForNoInput();

            while (pressedKey == 0 && pressedButton == GamepadButtons.None)
            {
                bool escapePressed = false;
                if (CheckEscapeHeld(ref escapePressed))
                    return;

                if (allowController)
                {
                    if (XInput.GetState(0, out State state))
                    {
                        foreach (GamepadButtons button in Enum.GetValues(typeof(GamepadButtons)))
                        {
                            if (button is GamepadButtons.None)
                                continue;
                            if (state.Gamepad.Buttons.HasFlag(button))
                            {
                                pressedButton = button;
                                return;
                            }
                        }
                    }
                }

                if (allowKeyboard)
                {
                    if(escapePressed)
                    {
                        pressedKey = Key.Escape;
                        return;
                    }
                    for (uint i = 1; i < 0xff; i++)
                    {
                        if (NativeKeyboard.IsKeyDown(i))
                        {
                            pressedKey = i;
                            return;
                        }

                    }
                }

            }
        }

        public void UpdateControllerBinds(Dictionary<GamepadButtons, InputIntent> controllerBinds)
        {
            this.controllerBinds = controllerBinds;
            keybindsChanged = true;

            Config.current.controllerBinds = controllerBinds;
            Config.SaveConfig();
            ClearIntents();
        }

        public void UpdateKeyboardBinds(Dictionary<uint, InputIntent> keyBinds)
        {
            this.keyBinds = keyBinds;
            keybindsChanged = true;

            Config.current.keyBinds = keyBinds;
            Config.SaveConfig();
            ClearIntents();
        }

        public bool CheckEscapeHeld(ref bool wasPressed, int holdTimeMs = 1000)
        {

            bool escapeDown = NativeKeyboard.IsKeyDown(Key.Escape);
            wasPressed = escapeDown;
            long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            while (escapeDown)
            {
                escapeDown = NativeKeyboard.IsKeyDown(Key.Escape);
                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - milliseconds >= holdTimeMs)
                    return true;

                Task.Delay(1).Wait();
            }

            return false;
        }

        public void UpdateInputBinds(Dictionary<uint,InputIntent> keyBinds, Dictionary<GamepadButtons,InputIntent> controllerBinds)
        {
            this.controllerBinds = controllerBinds;
            this.keyBinds = keyBinds;
            keybindsChanged = true;

            Config.current.keyBinds = keyBinds;
            Config.current.controllerBinds = controllerBinds;
            Config.SaveConfig();
        }

        //To be called from input thread
        private List<InputIntent> GetCurrentIntents(in ImmutableDictionary<uint,InputIntent> ro_KeyBinds, in ImmutableDictionary<GamepadButtons,InputIntent> ro_ControllerBinds)
        {
            List<InputIntent> intents = new List<InputIntent>();

            //Avoid bug in accessibility menu, when enabling key repetition with the left/right inputs
            if(!Config.current.KeyRepetition)
            {
                foreach(var item in keyRepeatTimers)
                    keyRepeatTimers[item.Key] = long.MaxValue;
            }

            foreach (var keybind in ro_KeyBinds)
            {
                bool thisKeyDown = NativeKeyboard.IsKeyDown(keybind.Key);
                if (thisKeyDown && !heldKeys[keybind.Key])
                {
                    if(keyRepeatTimers.ContainsKey(keybind.Value))
                        keyRepeatTimers[keybind.Value] = DateTime.UtcNow.Ticks + initialKeyRepeatDelay*10000;
                    intents.Add(keybind.Value);
                }
                else if(Config.current.KeyRepetition && thisKeyDown && keyRepeatTimers.ContainsKey(keybind.Value))
                {
                    if (keyRepeatTimers[keybind.Value] < DateTime.UtcNow.Ticks)
                    {
                        keyRepeatTimers[keybind.Value] = DateTime.UtcNow.Ticks + keyRepeatDelay * 10000;
                        intents.Add(keybind.Value);
                    }
                }

                heldKeys[keybind.Key] = thisKeyDown;
            }

            if (XInput.GetState(0, out State state))
            {
                foreach (var controllerBind in ro_ControllerBinds)
                {
                    bool shouldPress = state.Gamepad.Buttons.HasFlag(controllerBind.Key) && !prevState.Gamepad.Buttons.HasFlag(controllerBind.Key);

                    if (Config.current.KeyRepetition && keyRepeatTimers.ContainsKey(controllerBind.Value))
                    {
                        if (shouldPress)
                            keyRepeatTimers[controllerBind.Value] = DateTime.UtcNow.Ticks + initialKeyRepeatDelay * 10000;
                        else if (state.Gamepad.Buttons.HasFlag(controllerBind.Key) && keyRepeatTimers[controllerBind.Value] < DateTime.UtcNow.Ticks)
                        {
                            keyRepeatTimers[controllerBind.Value] = DateTime.UtcNow.Ticks + keyRepeatDelay * 10000;
                            shouldPress = true;
                        }
                    }

                    if (shouldPress)
                        intents.Add(controllerBind.Value);

                }

                //TODO: Clean this up
                if (state.Gamepad.LeftThumbX < -xThreshold && prevState.Gamepad.LeftThumbX >= -xThreshold)
                {
                    keyRepeatTimers[InputIntent.Left] = DateTime.UtcNow.Ticks + initialKeyRepeatDelay * 10000;
                    intents.Add(InputIntent.Left);
                }
                else if (state.Gamepad.LeftThumbX < -xThreshold && Config.current.KeyRepetition && keyRepeatTimers[InputIntent.Left] < DateTime.UtcNow.Ticks)
                {
                    keyRepeatTimers[InputIntent.Left] = DateTime.UtcNow.Ticks + keyRepeatDelay * 10000;
                    intents.Add(InputIntent.Left);
                }

                if (state.Gamepad.LeftThumbX > xThreshold && prevState.Gamepad.LeftThumbX <= xThreshold)
                {
                    keyRepeatTimers[InputIntent.Right] = DateTime.UtcNow.Ticks + initialKeyRepeatDelay * 10000;
                    intents.Add(InputIntent.Right);
                }
                else if (state.Gamepad.LeftThumbX > xThreshold && Config.current.KeyRepetition && keyRepeatTimers[InputIntent.Right] < DateTime.UtcNow.Ticks)
                {
                    keyRepeatTimers[InputIntent.Right] = DateTime.UtcNow.Ticks + keyRepeatDelay * 10000;
                    intents.Add(InputIntent.Right);
                }

                if (state.Gamepad.LeftThumbY < -yThreshold && prevState.Gamepad.LeftThumbY >= -yThreshold)
                {
                    keyRepeatTimers[InputIntent.Down] = DateTime.UtcNow.Ticks + initialKeyRepeatDelay * 10000;
                    intents.Add(InputIntent.Down);
                }
                else if (state.Gamepad.LeftThumbY < -yThreshold && Config.current.KeyRepetition && keyRepeatTimers[InputIntent.Down] < DateTime.UtcNow.Ticks)
                {
                    keyRepeatTimers[InputIntent.Down] = DateTime.UtcNow.Ticks + keyRepeatDelay * 10000;
                    intents.Add(InputIntent.Down);
                }

                if (state.Gamepad.LeftThumbY > yThreshold && prevState.Gamepad.LeftThumbY <= yThreshold)
                {
                    keyRepeatTimers[InputIntent.Up] = DateTime.UtcNow.Ticks + initialKeyRepeatDelay * 10000;
                    intents.Add(InputIntent.Up);
                }
                else if (state.Gamepad.LeftThumbY > yThreshold && Config.current.KeyRepetition && keyRepeatTimers[InputIntent.Up] < DateTime.UtcNow.Ticks)
                {
                    keyRepeatTimers[InputIntent.Up] = DateTime.UtcNow.Ticks + keyRepeatDelay * 10000;
                    intents.Add(InputIntent.Up);
                }

                //Left and right triggers as zombie minus/plus
                if (state.Gamepad.LeftTrigger > triggerThreshold && prevState.Gamepad.LeftTrigger <= triggerThreshold)
                {
                    keyRepeatTimers[InputIntent.ZombieMinus] = DateTime.UtcNow.Ticks + initialKeyRepeatDelay * 10000;
                    intents.Add(InputIntent.ZombieMinus);
                }
                else if (state.Gamepad.LeftTrigger > triggerThreshold && Config.current.KeyRepetition && keyRepeatTimers[InputIntent.ZombieMinus] < DateTime.UtcNow.Ticks)
                {
                    keyRepeatTimers[InputIntent.ZombieMinus] = DateTime.UtcNow.Ticks + keyRepeatDelay * 10000;
                    intents.Add(InputIntent.ZombieMinus);
                }

                if (state.Gamepad.RightTrigger > triggerThreshold && prevState.Gamepad.RightTrigger <= triggerThreshold)
                {
                    keyRepeatTimers[InputIntent.ZombiePlus] = DateTime.UtcNow.Ticks + initialKeyRepeatDelay * 10000;
                    intents.Add(InputIntent.ZombiePlus);
                }
                else if (state.Gamepad.RightTrigger > triggerThreshold && Config.current.KeyRepetition && keyRepeatTimers[InputIntent.ZombiePlus] < DateTime.UtcNow.Ticks)
                {
                    keyRepeatTimers[InputIntent.ZombiePlus] = DateTime.UtcNow.Ticks + keyRepeatDelay * 10000;
                    intents.Add(InputIntent.ZombiePlus);
                }


                //Map right-stick as wheel selector for plant slot (generous deadzones to keep wheel segments more distinct)
                if (state.Gamepad.RightThumbX > 27000 || state.Gamepad.RightThumbX < -27000 || state.Gamepad.RightThumbY > 27000 || state.Gamepad.RightThumbY < -27000)
                {
                    float theta = MathF.Atan2(state.Gamepad.RightThumbY, state.Gamepad.RightThumbX);
                    theta += MathF.PI * 2.0f;
                    theta %= MathF.PI * 2.0f;
                    theta = theta * 180.0f / MathF.PI;
                    float segmentSize = 360.0f / 10.0f;

                    // Map to the corresponding slot, handling wrap-around
                    int segmentIndex = (int)(theta / segmentSize);
                    segmentIndex = 9 - segmentIndex;
                    if (segmentIndex != prevRightSlot)
                        intents.Add((InputIntent)((int)InputIntent.Slot1 + segmentIndex));

                    prevRightSlot = segmentIndex;
                }
                
            }

            prevState = state;

            return intents;
        }

        public void StopThread()
        {
            running = false;
        }
    }
}
