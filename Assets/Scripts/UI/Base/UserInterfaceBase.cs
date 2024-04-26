using System.Collections.Generic;
using RPSLS.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPSLS.UI.Base
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract class UserInterfaceBase : MonoBehaviour
    {
        [SerializeField, Tooltip("The bases of screens")] 
        protected List<ScreenBase> screenBases;

        private int _currentIndex;
        private InputAction _escapeKeyAction;

        internal IReadOnlyList<ScreenBase> Screens => screenBases;

        /// <summary>
        /// Activate the previous screen of current screen
        /// </summary>
        /// <param name="screen">current screen</param>
        /// <param name="disableCurrent">if true(default), deactivate current screen, false otherwise</param>
        internal virtual void ActivatePreviousScreen(ScreenBase screen, bool disableCurrent = true)
        {
            var index = screenBases.FindIndex(x => x.Equals(screen));
            if (index > 0)
            {
                if (disableCurrent)
                    screen.DisableScreen();
                screenBases[_currentIndex = index - 1].EnableScreen();
            }
            else Debug.LogError("Doesn't have a Previous Screen!");
        }

        /// <summary>
        /// Activate the next screen of current screen
        /// </summary>
        /// <param name="screen">current screen</param>
        /// <param name="disableCurrent">if true(default), deactivate current screen, false otherwise</param>
        internal virtual void ActivateNextScreen(ScreenBase screen, bool disableCurrent = true)
        {
            var index = screenBases.FindIndex(x => x.Equals(screen));
            if (index < screenBases.Count - 1)
            {
                if (disableCurrent)
                    screen.DisableScreen();
                screenBases[_currentIndex = index + 1].EnableScreen();
            }
            else Debug.LogError("Doesn't have a Next Screen!");
        }

        /// <summary>
        /// Activate the next screen of specified screen index
        /// </summary>
        /// <param name="index">index of specified screen</param>
        /// <param name="disableCurrent">if true(default), deactivate screen of specified index, false otherwise</param>
        internal virtual void ActivateNextScreen(int index = -1, bool disableCurrent = true)
        {
            if (index < 0)
                index = _currentIndex;

            if (index < screenBases.Count - 1)
            {
                if (disableCurrent)
                    screenBases[_currentIndex = index].DisableScreen();
                screenBases[_currentIndex = index + 1].EnableScreen();
            }
            else Debug.LogError("Doesn't have a Next Screen!");
        }

        /// <summary>
        /// Activate the previous screen of specified screen index
        /// </summary>
        /// <param name="index">index of specified screen</param>
        /// <param name="disableCurrent">if true(default), deactivate screen of specified index, false otherwise</param>
        internal virtual void ActivatePreviousScreen(int index = -1, bool disableCurrent = true)
        {
            if (index < 0)
                index = _currentIndex;

            if (index > 0)
            {
                if (disableCurrent)
                    screenBases[_currentIndex = index].DisableScreen();
                screenBases[_currentIndex = index - 1].EnableScreen();
            }
            else Debug.LogError("Doesn't have a Previous Screen!");
        }

        /// <summary>
        /// Activate the screen of specified type
        /// </summary>
        /// <typeparam name="T">type of screen specified</typeparam>
        /// <param name="disableCurrent">if true(default), deactivate screen of current index, false otherwise</param>
        internal virtual void ActivateThisScreen<T>(bool disableCurrent = true) where T : ScreenBase
        {
            var screen = GetScreen<T>();
            if (!screen)
            {
                Debug.LogError($"Cannot Activate Screen: {nameof(T)}");
                return;
            }

            if (disableCurrent)
                screenBases[_currentIndex].DisableScreen();
            else screen.SetOverridePreviousScreen(screenBases[_currentIndex]);

            screen.EnableScreen();
            _currentIndex = screenBases.FindIndex(x => x.Equals(screen));
        }

        /// <summary>
        /// Activate the specified screen
        /// </summary>
        /// <param name="screen">screen specified</param>
        /// <param name="disableCurrent">if true(default), deactivate screen of current index, false otherwise</param>
        internal virtual void ActivateThisScreen(ScreenBase screen, bool disableCurrent = true)
        {
            if (disableCurrent)
                screenBases[_currentIndex].DisableScreen();
            else screen.SetOverridePreviousScreen(screenBases[_currentIndex]);

            screen.EnableScreen();
            _currentIndex = screenBases.FindIndex(x => x.Equals(screen));
        }

        /// <summary>
        /// Deactivate the screen of specified type
        /// </summary>
        /// <typeparam name="T">specified type of screen</typeparam>
        internal virtual void DeActivateThisScreen<T>() where T : ScreenBase
        {
            var screen = GetScreen<T>();
            if (!screen)
            {
                Debug.LogError($"Cannot Activate Screen: {nameof(T)}");
                return;
            }

            screen.DisableScreen();
        }

        /// <summary>
        /// Deactivate the specified screen
        /// </summary>
        /// <param name="screen">specified screen to deactivate</param>
        internal virtual void DeActivateThisScreen(ScreenBase screen) =>
            screen.DisableScreen();

        /// <summary>
        /// Get the screen of specified type
        /// </summary>
        /// <typeparam name="T">type specified</typeparam>
        /// <returns>screen instance of specified type</returns>
        internal T GetScreen<T>() where T : ScreenBase => (T)screenBases.Find(x => x.GetType() == typeof(T));

        protected virtual void Awake() =>
            _escapeKeyAction = new InputAction("Escape",
                InputActionType.Button, Keyboard.current.escapeKey.path, "tap");

        protected virtual void Start()
        {
            Bootstrap.GetService<UserInterfaceService>().CurrentInterface = this;
            screenBases.ForEach(x => x.DisableScreen());
            screenBases[0].EnableScreen();
        }

        private void OnEnable()
        {
            _escapeKeyAction.started += OnEscapeKeyCallback;
            _escapeKeyAction.Enable();
        }

        private void OnDisable()
        {
            _escapeKeyAction.started -= OnEscapeKeyCallback;
            _escapeKeyAction.Disable();
        }

        private void OnEscapeKeyCallback(InputAction.CallbackContext callbackContext)
        {
            switch (callbackContext.control.name)
            {
                case "escape":
                    screenBases[_currentIndex].OnBackKeyPressed();
                    break;
            }
        }
    }
}