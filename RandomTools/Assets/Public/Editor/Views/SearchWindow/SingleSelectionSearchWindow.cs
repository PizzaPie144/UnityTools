using System.Collections.Generic;
using UnityEditor;

namespace PizzaPie.Editor.Views
{
    /// <summary>
    /// Single Selection Search Window.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingleSelectionSearchWindow<T> : SearchWindow<T> where T : class
    {
        #region ctors
        /// <summary>
        /// Create a Single Selection Search PopupWindow.
        /// </summary>
        /// <param name="sourceItems">Items to search from.</param>
        /// <param name="filter">Filter to validate items on search.</param>
        /// <param name="cellView">CellView used by results ListView.</param>
        /// <param name="comparer">Comparer to sort results ListView's Cells.</param>
        public SingleSelectionSearchWindow(T[] sourceItems, Filter filter, CellView<T> cellView, IComparer<T> comparer) : base(sourceItems, filter, cellView, comparer, SelectionType.single)
        {
            listViewHeightOffset = -5f - 1.5f * EditorGUIUtility.singleLineHeight;
        }
        #endregion

        protected override void OnCellDoubleClickedHandler(object sender, ListView<T>.OnSelectionEventArgs e)
        {
            OnSelectionFinished(e);
        }
    }
}