using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DWFX
{
    public class UIEffectController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Parent node containing all the child objects to switch between")]
        public Transform parentNode;
        [Tooltip("Button to navigate to the previous item")]
        public Button lastBtn;
        [Tooltip("Button to navigate to the next item")]
        public Button nextBtn;
        [Tooltip("Text component to display current particle Index")]
        public TMP_Text particleIndexText;
        [Tooltip("Text component to display current child object name")]
        public TMP_Text nameText;
        [Tooltip("Image object to display the selected material")]
        public GameObject popDisplayImage;
        public Button activeIconButton;
        public GameObject icon;
        // Current index of the displayed child object
        private int currentIndex = 0;
        // Dictionary to track buttons that already have event listeners
        private Dictionary<Button, bool> buttonsWithListeners = new Dictionary<Button, bool>();


        private void Start()
        {
            // Initial validation
            if (parentNode == null || lastBtn == null || nextBtn == null ||
                particleIndexText == null || nameText == null || popDisplayImage == null)
            {
                Debug.LogError("Please set all necessary references in the Inspector!");
                return;
            }

            // Set up the popDisplayImage button to hide itself when clicked
            Button popDisplayButton = popDisplayImage.GetComponent<Button>();
            if (popDisplayButton != null)
            {
                popDisplayButton.onClick.AddListener(() => {
                    popDisplayImage.SetActive(false);
                });
            }
            else
            {
                Debug.LogWarning("popDisplayImage does not have a Button component!");
            }

            // Initially hide the pop display
            popDisplayImage.SetActive(false);

            // Initialize display with the first child object
            UpdateDisplay();

            // Add button click event listeners
            lastBtn.onClick.AddListener(ShowLastItem);
            nextBtn.onClick.AddListener(ShowNextItem);
            activeIconButton.onClick.AddListener(ActiveIconBtnCLick);
        }

        // Show the previous child object
        public void ShowLastItem()
        {
            if (currentIndex > 0)
            {
                currentIndex--;
            }
            else if (currentIndex == 0) currentIndex = parentNode.childCount - 1;
            UpdateDisplay();
        }
        public void ActiveIconBtnCLick()
        {
            icon.gameObject.SetActive(!icon.gameObject.activeSelf);
        }

        // Show the next child object
        public void ShowNextItem()
        {
            if (currentIndex < parentNode.childCount - 1)
            {
                currentIndex++;
            }
            else if (currentIndex == parentNode.childCount - 1) currentIndex = 0;
            UpdateDisplay();
        }

        // Update display
        private void UpdateDisplay()
        {
            print("currentIndex" + currentIndex);
            // Hide all child objects
            for (int i = 0; i < parentNode.childCount; i++)
            {
                parentNode.GetChild(i).gameObject.SetActive(false);
            }

            // Show the current index child object
            if (parentNode.childCount > 0)
            {
                GameObject currentObject = parentNode.GetChild(currentIndex).gameObject;
                currentObject.SetActive(true);

                // Check for Button components in the current object's children (non-recursive)
                AddButtonListeners(currentObject.transform);
            }

            // Update particle text
            UpdateparticleText();
            // Update child object name text
            UpdateNameText();
        }

        // Method to add button listeners to all child objects with Button components
        private void AddButtonListeners(Transform parent)
        {
            // Loop through all direct children
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Button button = child.GetComponent<Button>();

                // Check if this child has a Button component
                if (button == null)
                {
                    button = child.gameObject.AddComponent<Button>();
                }
                // Only add listener if it doesn't have one yet
                if (!buttonsWithListeners.ContainsKey(button) || !buttonsWithListeners[button])
                    {
                        // Store current index in a local variable to avoid closure issues
                        int index = currentIndex;

                        button.onClick.AddListener(() => {
                            DisplayImageInPopup(child.gameObject);
                        });

                        // Mark this button as having a listener
                        buttonsWithListeners[button] = true;
                    }
            }
        }

        // Method to display the material in the popup
        private void DisplayImageInPopup(GameObject sourceObject)
        {
            // Get the Image component from the source object
            Image sourceImage = sourceObject.GetComponent<Image>();
            if (sourceImage != null && sourceImage.material != null)
            {
                // Make sure popDisplayImage has at least one child
                if (popDisplayImage.transform.childCount > 0)
                {
                    // Get the Image component from the first child of popDisplayImage
                    Image targetImage = popDisplayImage.transform.GetChild(0).GetComponent<Image>();

                    if (targetImage != null)
                    {
                        // Assign the material from the source to the target
                        targetImage.material = sourceImage.material;

                        // Show the popDisplayImage
                        popDisplayImage.SetActive(true);
                    }
                    else
                    {
                        Debug.LogWarning("First child of popDisplayImage does not have an Image component!");
                    }
                }
                else
                {
                    Debug.LogWarning("popDisplayImage has no children!");
                }
            }
            else
            {
                Debug.LogWarning("Source object does not have an Image component or its material is null!");
            }
        }

        // Update particle text
        private void UpdateparticleText()
        {
            if (particleIndexText != null)
            {
                // particle number starts from 1, total count is number of child objects
                particleIndexText.text = $"{currentIndex + 1} / {parentNode.childCount}";
            }
        }

        // Update child object name text
        private void UpdateNameText()
        {
            if (nameText != null && parentNode.childCount > 0)
            {
                // Display the name of the current child object
                nameText.text = parentNode.GetChild(currentIndex).name;
            }
        }
    }
}