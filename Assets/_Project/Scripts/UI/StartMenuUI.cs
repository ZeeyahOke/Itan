using System;
using UnityEngine;
using UnityEngine.UI;

namespace Itan.UI
{
    /// <summary>
    /// The world-space title panel. It only shows itself and reports
    /// when Begin is pressed; MenuState decides what happens next.
    /// </summary>
    public class StartMenuUI : MonoBehaviour
    {
        [SerializeField] Button beginButton;

        public event Action BeginPressed;

        public Button BeginButton => beginButton;

        void Awake()
        {
            beginButton.onClick.AddListener(() => BeginPressed?.Invoke());
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);
    }
}
