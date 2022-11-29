using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace TheKiwiCoder {
    public class NewScriptDialogView : VisualElement {
        public new class UxmlFactory : UxmlFactory<NewScriptDialogView, UxmlTraits> { }

        EditorUtility.ScriptTemplate scriptTemplate;
        TextField textField;
        Button confirmButton;

        public void CreateScript(EditorUtility.ScriptTemplate scriptTemplate) {

            this.scriptTemplate = scriptTemplate;

            style.visibility = Visibility.Visible;

            var background = this.Q<VisualElement>("Background");
            var titleLabel = this.Q<Label>("Title");
            textField = this.Q<TextField>("FileName");
            confirmButton = this.Q<Button>();

            titleLabel.text = $"New {scriptTemplate.subFolder.TrimEnd('s')} Script";

            textField.focusable = true;
            this.RegisterCallback<PointerEnterEvent>((e) => {
                textField[0].Focus();
            });

            textField.RegisterCallback<KeyDownEvent>((e) => {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) {
                    OnConfirm();
                }
            });

            confirmButton.clicked -= OnConfirm;
            confirmButton.clicked += OnConfirm;

            background.RegisterCallback<PointerDownEvent>((e) => {
                e.StopImmediatePropagation(); 
                Close();
            });
        }

        void Close() {
            style.visibility = Visibility.Hidden;
        }

        void OnConfirm() {
            string scriptName = textField.text;

            var newNodePath = $"{BehaviourTreeEditorWindow.Instance.settings.newNodePath}";
            if (AssetDatabase.IsValidFolder(newNodePath)) {

                var destinationFolder = System.IO.Path.Combine(newNodePath, scriptTemplate.subFolder);
                var destinationPath = System.IO.Path.Combine(destinationFolder, $"{scriptName}.cs");

                System.IO.Directory.CreateDirectory(destinationFolder);

                var parentPath = System.IO.Directory.GetParent(Application.dataPath);

                string templateString = scriptTemplate.templateFile.text;
                templateString = templateString.Replace("#SCRIPTNAME#", scriptName);
                string scriptPath = System.IO.Path.Combine(parentPath.ToString(), destinationPath);

                if (!System.IO.File.Exists(scriptPath)) {
                    System.IO.File.WriteAllText(scriptPath, templateString);
                    AssetDatabase.Refresh();
                    confirmButton.SetEnabled(false);
                    EditorApplication.delayCall += WaitForCompilation;
                } else {
                    Debug.LogError($"Script with that name already exists:{scriptPath}");
                    Close();
                }
            } else {
                Debug.LogError($"Invalid folder path:{newNodePath}. Check the project configuration settings 'newNodePath' is configured to a valid folder");
            }
        }

        void WaitForCompilation() {
            if (EditorApplication.isCompiling) {
                EditorApplication.delayCall += WaitForCompilation;
                return;
            }

            confirmButton.SetEnabled(true);
            Close();
        }
    }
}