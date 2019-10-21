using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PizzaPie.Editor.Views
{
    /// <summary>
    /// Multi selection search window.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MultipleSelectionSearchWindow<T> : SearchWindow<T> where T : class
    {
        private bool useFinishButton;
        
        #region ctors
        /// <summary>
        /// Create a Mutli Selection Search PopupWindow.
        /// </summary>
        /// <param name="sourceItems">Items to search from.</param>
        /// <param name="filter">Filter to validate items on search.</param>
        /// <param name="cellView">CellView used by results ListView.</param>
        /// <param name="comparer">Comparer to sort results ListView's Cells.</param>
        /// <param name="useFinishButton">Show finish button on bottom of SearchWindow.</param>
        public MultipleSelectionSearchWindow(T[] sourceItems, Filter filter, CellView<T> cellView, IComparer<T> comparer, bool useFinishButton = true) : base(sourceItems, filter, cellView, comparer, SelectionType.multiple)
        {
            this.useFinishButton = useFinishButton;

            if (useFinishButton)
                listViewHeightOffset = -5f - 2.5f * EditorGUIUtility.singleLineHeight;
            else
                listViewHeightOffset = -5f - 1.5f * EditorGUIUtility.singleLineHeight;
        }
        #endregion

        /// <summary>
        /// Do not call directly, use PopupWindow.Show().
        /// </summary>
        /// <param name="rect"></param>
        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            if (!useFinishButton)
                return;

            Rect buttonRect = new Rect(0, rect.height - 1.2f * EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(buttonRect, "Finish"))
            {
                OnSelectionFinished(new ListView<T>.OnSelectionEventArgs(listView.GetSelectedItems));
            }
        }
    }
}
