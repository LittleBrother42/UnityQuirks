using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Options
{
    /// <summary>
    /// Allows for loading, modifying and saving of options.
    /// </summary>
    public class OptionsManager : MonoBehaviour
    {
        /// <summary>
        /// The input actions that will be loaded and saved from this manager.
        /// </summary>
        [SerializeField]
        private InputActionAsset inputActions;

        /// <summary>
        /// The profile describing the post-processing effects to apply to every scene.
        /// </summary>
        [SerializeField]
        private VolumeProfile postProcessingVolumeProfile;

        /// <summary>
        /// The toggle component that can be used to switch between high and low graphics quality.
        /// </summary>
        /// <remarks>
        /// Try toggling the quality checkbox in play mode; you'll see that when the box is
        /// unchecked, the shadows disappear and anti-aliasing is disabled (the edges of shapes
        /// will become more jagged). You'll also see that the lighter parts of the image are
        /// brighter when checked, due to the bloom post-processing effect that is disabled when
        /// unchecked. If you build and run the app using the Options build profile, you'll find
        /// that it remembers your most recent choice!
        /// </remarks>
        [SerializeField]
        private Toggle qualityToggle;

        /// <summary>
        /// The text field in which the saved text will be displayed and can be edited.
        /// </summary>
        [SerializeField]
        private TMP_InputField savedTextField;

        /// <summary>
        /// The path of the saved text file.
        /// </summary>
        /// <remarks>
        /// This is always populated in the <see cref="OptionsManager.Start"/> method.
        /// </remarks>
        private string savedTextFilePath;

        /// <summary>
        /// The toggle component that can be used to enable or disable using the space bar to
        /// interact with the UI, instead of Enter.
        /// </summary>
        /// <remarks>
        /// Highlighting components in Unity is a bit awkward (especially when there's a text box
        /// on the canvas), but fixing most of those problems is outside the scope of this example.
        /// The easiest way to test that this is working is to click on this checkbox with the
        /// mouse to select it. If the box is checked, you'll be able to use Space to uncheck it,
        /// but not Enter. If it's unchecked, you'll be able to use Enter but not Space.
        /// </remarks>
        [SerializeField]
        private Toggle spaceSubmitToggle;

        /// <summary>
        /// Exits the application.
        /// </summary>
        public void Quit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Handles any logic that should be executed when this behaviour is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            this.savedTextField.onEndEdit.RemoveListener(this.OnTextEditEnded);
            this.qualityToggle.onValueChanged.RemoveListener(this.OnQualityToggleValueChanged);
        }

        /// <summary>
        /// Handles any logic specific to this behaviour that should be executed once, after the
        /// object is created and activated but before any updates have occurred.
        /// </summary>
        private void Start()
        {
            // Set up the saved text, and load it if available; we'll log an error message if the
            // file exists but fails to load.
            this.savedTextFilePath = Path.Combine(Application.persistentDataPath, "SavedText.txt");
            if (File.Exists(this.savedTextFilePath))
            {
                try
                {
                    FileStream textStream = File.OpenRead(this.savedTextFilePath);
                    using (StreamReader reader = new StreamReader(textStream))
                    {
                        string text = reader.ReadToEnd();
                        this.savedTextField.text = text;
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
            this.savedTextField.onEndEdit.AddListener(this.OnTextEditEnded);

            // Set up the inputs and Space submit toggle.
            string inputJson = PlayerPrefs.GetString("Input");
            if (inputJson != null)
            {
                this.inputActions.LoadBindingOverridesFromJson(inputJson);
                InputAction submitAction = this.inputActions.FindAction("Submit");
                InputBinding submitBinding = submitAction.bindings[0];
                bool isSpaceAvailable = submitBinding.overridePath == "<Keyboard>/space";
                this.spaceSubmitToggle.isOn = isSpaceAvailable;
            }
            this.spaceSubmitToggle.onValueChanged.AddListener(this.OnSpaceSubmitToggleValueChanged);

            // Set up the quality toggle and bloom setting.
            int highQualityIndex = Array.IndexOf(QualitySettings.names, "High");
            bool isHighQuality = QualitySettings.GetQualityLevel() == highQualityIndex;
            this.qualityToggle.isOn = isHighQuality;
            this.qualityToggle.onValueChanged.AddListener(this.OnQualityToggleValueChanged);

            this.postProcessingVolumeProfile.TryGet<Bloom>(out Bloom bloom);
            bloom.intensity.value = isHighQuality ? 0.75f : 0;
        }

        /// <summary>
        /// Handles completion of an edit of the <see cref="OptionsManager.savedTextField"/> by
        /// saving the contents to a file.
        /// </summary>
        /// <param name="text">
        /// The new content of the text field.
        /// </param>
        private void OnTextEditEnded(string text)
        {
            // As with loading, we'll log an exception if we can't write for any reason.
            try
            {
                FileStream textStream = File.Create(this.savedTextFilePath);
                using (StreamWriter writer = new StreamWriter(textStream))
                {
                    writer.Write(text);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        /// <summary>
        /// Handles a change to the value of the <see cref="OptionsManager.qualityToggle"/> by
        /// updating the graphics quality and saving the result.
        /// </summary>
        /// <param name="isToggled">
        /// <c>true</c> to use high quality graphics, <c>false</c> to use low quality graphics.
        /// </param>
        private void OnQualityToggleValueChanged(bool isToggled)
        {
            int qualityIndex;

            this.postProcessingVolumeProfile.TryGet<Bloom>(out Bloom bloom);

            if (isToggled)
            {
                qualityIndex = Array.IndexOf(QualitySettings.names, "High");
                bloom.intensity.value = 0.75f;
            }
            else
            {
                qualityIndex = Array.IndexOf(QualitySettings.names, "Low");
                bloom.intensity.value = 0;
            }

            QualitySettings.SetQualityLevel(qualityIndex);
        }

        /// <summary>
        /// Handles a change to the value of the <see cref="OptionsManager.spaceSubmitToggle"/> by
        /// updating and saving the input bindings.
        /// </summary>
        /// <param name="isToggled">
        /// <c>true</c> to use Space to submit, <c>false</c> otherwise.
        /// </param>
        private void OnSpaceSubmitToggleValueChanged(bool isToggled)
        {
            // Add or remove Space as a binding override for Submit.
            InputAction submitAction = this.inputActions.FindAction("Submit");
            if (isToggled)
            {
                submitAction.ApplyBindingOverride("<Keyboard>/space", "Keyboard 1");
            }
            else
            {
                submitAction.RemoveAllBindingOverrides();
            }

            // Save the new binding overrides.
            string inputJson = this.inputActions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("Input", inputJson);
        }
    }
}