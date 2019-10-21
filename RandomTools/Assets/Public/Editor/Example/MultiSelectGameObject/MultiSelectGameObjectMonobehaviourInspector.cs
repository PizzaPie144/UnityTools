using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

using UnityEngine;
using UnityEditor;

using PizzaPie.Editor.Views;
using PizzaPie.Editor.Util;

namespace PizzaPie.Editor.Example.MultiSelectGameObject
{
    [CustomEditor(typeof(MonoBehaviour), true, isFallback = false), CanEditMultipleObjects]
    public class MultiSelectGameObjectMonobehaviourInspector : UnityEditor.Editor
    {
        private Dictionary<SerializedProperty, Vector2> supportedArrayProperties;
        private List<Type> supportedTypes = new List<Type>();

        private MultipleSelectionSearchWindow<GameObject> searchWindow;

        private Rect validRect;
        private SerializedProperty currentlySelectedProperty;

        Color targetRectColor;
        bool debugPositions = true;    //change this to diplay left click target positions

        private void OnEnable()
        {
            supportedTypes.Add(typeof(GameObject));
            supportedArrayProperties = new Dictionary<SerializedProperty, Vector2>();

            SerializedProperty sProperty = serializedObject.GetIterator();

            while (sProperty.NextVisible(true))
            {
                if (sProperty.isArray)
                {
                    Type elementType = SerializedProperties.GetElementType(sProperty);
                    if(supportedTypes.Contains(elementType))
                        supportedArrayProperties.Add(sProperty.Copy(), Vector2.zero);
                }
            }

            GameObject[] gos = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            GameObject[] gameObjects = new GameObject[gos.Length + 1];
            gos.CopyTo(gameObjects, 1);

            GameObjectNameFilter filter = new GameObjectNameFilter();
            GameObjectCellView cellView = new GameObjectCellView(Vector2.zero);         //use default size
            GameObjectNameCompararer compararer = new GameObjectNameCompararer();

            searchWindow = new MultipleSelectionSearchWindow<GameObject>(gameObjects, filter, cellView, compararer,false);

            searchWindow.CellClicked += OnCellSelectedHandler;

            //for debug position
            targetRectColor = Color.red;
            targetRectColor.a = 0.1f;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Event e = Event.current;

            Rect rect = EditorGUILayout.BeginVertical();    //hacky way to get rect of the component's inspector
            this.DrawDefaultInspector();
            EditorGUILayout.EndVertical();

            if (this.targets.Length > 1) //disable it for multiple objects editing
                return;

            if (e.type == EventType.Repaint)
                validRect = rect;

            if (validRect == null)                         //make sure a repaint event has occured to calculate array properties positions properly
                return;

            SerializedProperty sProperty = serializedObject.GetIterator();

            float padding = (float) /*GUI.skin.label.padding.vertical*//*4f /*/ 2f;         //technically we should get the style of each control drawn and use that padding
            float height = 0;

            bool isExpanded = true;

            //calculate relative positions of each target control
            while (sProperty.NextVisible(isExpanded))
            {
                //make sure we don't count in child elements in none expanded controls
                isExpanded = sProperty.hasVisibleChildren ? sProperty.isExpanded : true;

                float propertyHeight = EditorGUI.GetPropertyHeight(sProperty, false) + padding;
                foreach (var key in supportedArrayProperties.Keys)
                {
                    if (SerializedProperty.EqualContents(key, sProperty))
                    {
                        //save the current position of target controls
                        supportedArrayProperties[key] = new Vector2(height, propertyHeight);       //technically propertyHeight should remain the same
                        break;
                    }
                }
                height += propertyHeight;
            }

            foreach (var pair in supportedArrayProperties)
            {
                //relative rect to Inspector Window
                Rect targetRect = new Rect(0, pair.Value.x + rect.y, Screen.width, pair.Value.y);

                if (debugPositions)
                    EditorGUI.DrawRect(targetRect, targetRectColor);

                if (e.button == 1 && e.type == EventType.MouseUp)
                {
                    if (targetRect.Contains(e.mousePosition))
                    {
                        currentlySelectedProperty = pair.Key;
                        List<GameObject> selected = new List<GameObject>();

                        for (int i = 0; i < pair.Key.arraySize; i++)
                        {
                            GameObject go = SerializedProperties.GetArrayElementAt(i, pair.Key) as GameObject;
                            selected.Add(go);
                        }

                        //eh
                        searchWindow.UnselectAll();
                        searchWindow.SetPreselectedItems(selected);

                        PopupWindow.Show(new Rect(e.mousePosition, Vector2.zero), searchWindow);
                        break;
                    }
                }
            }
        }

        private void OnCellSelectedHandler(object sender, ListView<GameObject>.OnCellClickedEventArgs e)
        {
            serializedObject.Update();

            if (currentlySelectedProperty == null)
                return;

            if (e.IsSelected)
                SerializedProperties.AddArrayItem(e.CellData, currentlySelectedProperty);
            else
                SerializedProperties.RemoveArrayItem(e.CellData, currentlySelectedProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }

}