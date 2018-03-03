using LibSumo.Net.Events;
using LibSumo.Net.Hook;
using LibSumo.Net.Logger;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LibSumo.Net
{
    /// <summary>
    /// Class to pilot Jumping Sumo with Keyboard Arrow
    /// </summary>
    public class SumoKeyboardPiloting
    {
        #region Private Fields
        // Save Hook ID to Restore 
        private static IntPtr _hookIDKeyboard = IntPtr.Zero;
        private  HookUtils.LowLevelKeyboardProc _callbackKeyboard = null;

        // Keys
        private bool KEY_UP;
        private bool KEY_DOWN;
        private bool KEY_LEFT;
        private bool KEY_RIGHT;
        private bool PilotingThreadStarted = false;
        private bool KeyboardThreadStarted = false;
        private bool Should_run { get; set; }
        #endregion

        public BlockingCollection<KeyValuePair<HookUtils.VirtualKeyStates, bool>> CurrentKeyStack { get; set; }

        #region Piloting Constants
        // Const
        public const sbyte ACCELERATION_CONSTANT = 5;
        public const sbyte DECCELERATION_CONSTANT = ACCELERATION_CONSTANT * ACCELERATION_CONSTANT;
        public const sbyte TURN_CONSTANT = 2;
        #endregion
        

        /// <summary>
        /// Dont instanciate piloting. Let SumoController to do this for you.
        /// </summary>
        internal SumoKeyboardPiloting()
        {            
            CurrentKeyStack = new BlockingCollection<KeyValuePair<HookUtils.VirtualKeyStates, bool>>();            
        }

        internal void InstallHook()
        {
            if (_hookIDKeyboard == IntPtr.Zero)
            {
                //Lien avec les méthodes qui vont traiter les hooks clavier/souris            
                LOGGER.GetInstance.Info("Keyboard Hook installed");
                _callbackKeyboard = KeyboardHookCallback;
                _hookIDKeyboard = SetKeyboardHook(_callbackKeyboard);
            }else
                LOGGER.GetInstance.Info("Keyboard Hook already installed");
        }

        internal void UnInstallHook()
        {
            if (_hookIDKeyboard != IntPtr.Zero)
            {
                StopThreads();
                NativeMethods.UnhookWindowsHookEx(_hookIDKeyboard);
                LOGGER.GetInstance.Info("Keyboard Hook Uninstalled");
            }else
                LOGGER.GetInstance.Info("Keyboard Hook not installed");
        }

        #region Keyboard Hook Methods
        private  IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            
            HookUtils.KBDLLHOOKSTRUCT objKeyInfo = (HookUtils.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(HookUtils.KBDLLHOOKSTRUCT));
            
            bool KeyDown = (int)wParam == 0x0100;
            
            if (objKeyInfo.vkCode == HookUtils.VirtualKeyStates.VK_ESCAPE)
            {
                // Disconnect                                
                OnDisconnect(new EventArgs());
            }

            if (objKeyInfo.vkCode == HookUtils.VirtualKeyStates.VK_UP && KeyDown) KEY_UP = true;
            else if (objKeyInfo.vkCode == HookUtils.VirtualKeyStates.VK_UP && !KeyDown) KEY_UP = false;

            if(objKeyInfo.vkCode == HookUtils.VirtualKeyStates.VK_DOWN && KeyDown) KEY_DOWN = true;
            else if (objKeyInfo.vkCode == HookUtils.VirtualKeyStates.VK_DOWN && !KeyDown)KEY_DOWN = false;

            if (objKeyInfo.vkCode == HookUtils.VirtualKeyStates.VK_LEFT && KeyDown) KEY_LEFT = true;
            else if (objKeyInfo.vkCode == HookUtils.VirtualKeyStates.VK_LEFT && !KeyDown) KEY_LEFT = false;

            if (objKeyInfo.vkCode == HookUtils.VirtualKeyStates.VK_RIGHT && KeyDown) KEY_RIGHT = true;
            else if (objKeyInfo.vkCode == HookUtils.VirtualKeyStates.VK_RIGHT && !KeyDown) KEY_RIGHT = false;
            
            // Stack Current Key but exclude arrow and ESC
            switch(objKeyInfo.vkCode)
            {
                case HookUtils.VirtualKeyStates.VK_UP:
                case HookUtils.VirtualKeyStates.VK_DOWN:
                case HookUtils.VirtualKeyStates.VK_LEFT:
                case HookUtils.VirtualKeyStates.VK_RIGHT:
                case HookUtils.VirtualKeyStates.VK_ESCAPE:                
                    break;

                default:
                    CurrentKeyStack.Add(new KeyValuePair<HookUtils.VirtualKeyStates, bool>(objKeyInfo.vkCode, KeyDown));
                    break;
            }
            
            return NativeMethods.CallNextHookEx(_hookIDKeyboard, nCode, wParam, lParam);
        }      

        private IntPtr SetKeyboardHook(HookUtils.LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return NativeMethods.SetWindowsHookEx(13, proc, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        #endregion

        #region Threads
        internal void RunKeyboardThread()
        {
            if (!KeyboardThreadStarted)
            {
                KeyboardThreadStarted = true;
                Should_run = true;
                Task.Run(() =>
                {
                    LOGGER.GetInstance.Info("Keyboard Thread Started");
                    while (this.Should_run)
                    {
                        KeyValuePair<HookUtils.VirtualKeyStates, bool> q = CurrentKeyStack.Take();
                        OnKeyboard(new KeyboardEventArgs(q.Key, q.Value));
                        Thread.Sleep(50);
                    }
                    LOGGER.GetInstance.Info("Keyboard Thread Stopped");
                });
            }else
                LOGGER.GetInstance.Info("Keyboard Thread already started");
        }
        internal void RunPilotingThread()
        {
            if (!PilotingThreadStarted)
            {
                PilotingThreadStarted = true;
                Should_run = true;
                Task.Run(() =>
                {
                    LOGGER.GetInstance.Info("Piloting Thread Started");
                    sbyte turn = 0;
                    int speed = 0;

                /// original https://github.com/iloreen/libsumo algorythme
                while (this.Should_run)
                    {

                        sbyte mod = 0;
                        if (KEY_UP == true)
                        {
                            if (speed >= 0) mod = ACCELERATION_CONSTANT;
                            else mod = DECCELERATION_CONSTANT * 2;//breaking - we are going reverse                     
                    }
                        else if (KEY_DOWN == true)
                        {
                            if (speed <= 0) mod = -ACCELERATION_CONSTANT;
                            else mod = -DECCELERATION_CONSTANT * 2;//breaking
                    }
                        else if (!KEY_UP && !KEY_DOWN)
                        {
                            mod = (sbyte)(-speed / ACCELERATION_CONSTANT);
                        ///* the faster we go the more we reduce speed */
                        if (mod == 0 && speed != 0)
                            {
                                if (speed < 0) mod = 1;
                                else mod = -1;
                            }
                        }
                        speed += mod;
                    //Limit
                    if (speed > 127) speed = 127;
                        if (speed < -127) speed = -127;

                    ///* turning */        
                    mod = 0;
                        if (KEY_LEFT == true) mod = -TURN_CONSTANT;
                        else if (KEY_RIGHT == true) mod = TURN_CONSTANT;
                        else if (!KEY_LEFT && !KEY_RIGHT)
                        {
                            mod = (sbyte)(-turn / TURN_CONSTANT * 3);
                            if (Math.Abs(turn) < TURN_CONSTANT && turn != 0) mod = (sbyte)-turn;
                        }
                        turn += mod;
                    //Limit
                    if (turn > 32) turn = 32;
                        if (turn < -32) turn = -32;


                        OnMove(new MoveEventArgs((sbyte)speed, (sbyte)turn));
                        Thread.Sleep(20);
                    }
                    LOGGER.GetInstance.Info("Piloting Thread Stopped");
                });
            }else
                LOGGER.GetInstance.Info("Piloting Thread already started");
        }
        internal void StopThreads()
        {
            Should_run = false;
        }
        #endregion

        #region Event Handler
        public delegate void DisconnectEventHandler(object sender, EventArgs e);
        public event DisconnectEventHandler Disconnect;
        protected virtual void OnDisconnect(EventArgs e)
        {
            Disconnect?.Invoke(this, e);
        }

        public delegate void MoveEventHandler(object sender, MoveEventArgs e);
        /// <summary>
        /// Return the calculated Speed and Turn value based on original https://github.com/iloreen/libsumo algorythme
        /// </summary>
        public event MoveEventHandler Move;
        protected virtual void OnMove(MoveEventArgs e)
        {
            Move?.Invoke(this, e);
        }

        public delegate void KeyboardEventHandler(object sender, KeyboardEventArgs e);
        /// <summary>
        /// Callback all keys pressed (and depressed) except Arrows and ESC
        /// </summary>
        public event KeyboardEventHandler KeyboardKeysAvailable;
        protected virtual void OnKeyboard(KeyboardEventArgs e)
        {
            KeyboardKeysAvailable?.Invoke(this, e);
        }
        #endregion

    }
}
