using UnityEngine;
using Game.UI;

namespace Game.GameManagement
{
    public class UIManager : MonoBehaviour
    {
        public UIHUD HUDPanel;
        public GameObject MainMenuPanel;
        public UIInventoryPanel inventoryPanel;

        // Singleton
        public static UIManager Instance
        {
            get; private set;

        }

        #region Awake to Update

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            ShowHUD(true);
            ShowMouse(false);
        }

        // Update is called once per frame
        void Update()
        {
            TogglePauseMenu(Input.GetButtonDown("Pause"));
            ToggleInventory(Input.GetButtonDown("Inventory"));
        }

        #endregion

        #region Helpers

        private void ShowHUD(bool visible)
        {
            HUDPanel.gameObject.SetActive(visible);
            MainMenuPanel.SetActive(!visible);
            inventoryPanel.ShowInventory(!visible);
        }

        private void ShowPauseMenu(bool visible)
        {
            HUDPanel.gameObject.SetActive(!visible);
            MainMenuPanel.SetActive(visible);
            inventoryPanel.ShowInventory(!visible);
        }


        private void ShowInventory(bool visible)
        {
            inventoryPanel.ShowInventory(visible);
        }

        private void ShowMouse(bool visible)
        {
            if (visible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        #endregion

        private void ToggleInventory(bool buttonDown)
        {
            if (buttonDown)
            {
                GameTime.TogglePause();
                ShowInventory(GameTime.isPaused);
            }
        }

        private void TogglePauseMenu(bool buttonDown)
        {
            if (buttonDown)
            {
                GameTime.TogglePause();
                TogglePauseMenu(GameTime.isPaused);
                ShowMouse(true);
            }
        }



    }

}
