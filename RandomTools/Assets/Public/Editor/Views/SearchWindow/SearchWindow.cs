using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace PizzaPie.Editor.Views
{
    /// <summary>
    /// Base class for SearchWindow PopupWindow.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SearchWindow<T> : PopupWindowContent where T : class
    {
        /// <summary>
        /// Items to search on.
        /// </summary>
        protected T[] sourceItems;
        private List<T> filteredItems;
        private Filter filter;

        /// <summary>
        /// ListView that presents results.
        /// </summary>
        protected ListView<T> listView;
        /// <summary>
        /// CellView used by ListView.
        /// </summary>
        protected CellView<T> cellView;

        private string searchQuery;

        /// <summary>
        /// Height differense from the top of the SearchWindow.
        /// </summary>
        protected float listViewHeightOffset;

        /// <summary>
        /// On Cell Clicked event.
        /// </summary>
        public event EventHandler<ListView<T>.OnCellClickedEventArgs> CellClicked
            { add { listView.CellClicked += value; } remove { listView.CellClicked -= value; } }

        /// <summary>
        /// On Cell Double Clicked event.
        /// </summary>
        public event EventHandler<ListView<T>.OnSelectionEventArgs> CellDoubleClicked
            { add { listView.CellDoubleClicked  += value; } remove { listView.CellDoubleClicked -= value; } }

        /// <summary>
        /// Subscribe to get the Selection Finished event.
        /// </summary>
        public event EventHandler<ListView<T>.OnSelectionEventArgs> SelectionFinished;

        #region ctors
        /// <summary>
        /// Creates SearchWindow.
        /// </summary>
        /// <param name="sourceItems">Items to search from.</param>
        /// <param name="filter">Filter to validate search.</param>
        /// <param name="cellView">CellView used by results ListView.</param>
        /// <param name="comparer">Comparer used to sort results ListView Cells.</param>
        /// <param name="selectionType">Type of selection.</param>
        public SearchWindow(T[] sourceItems, Filter filter, CellView<T> cellView, IComparer<T> comparer, SelectionType selectionType)
        {
            this.sourceItems = sourceItems;
            this.filter = filter;
            this.cellView = cellView;

            filteredItems = new List<T>();
            listView = new ListView<T>(sourceItems.ToList(), cellView, comparer, selectionType, true);


            CellClicked += OnCellClickedHandler;
            CellDoubleClicked += OnCellDoubleClickedHandler;
        }

        #endregion

        /// <summary>
        /// Unity callback override for PopupWindowContext, do not call directly use PopupWindow.Show().
        /// </summary>
        /// <param name="rect"></param>
        public override void OnGUI(Rect rect)
        {
            Event e = Event.current;
            Rect searchFieldRect = new Rect(0, 0, 100f, 15f);
            searchQuery = EditorGUILayout.TextField("Search: ", searchQuery);//EditorGUI.TextField(searchFieldRect, new GUIContent("Search: "), searchQuery);

            if (GUI.changed)
            {
                FilterList(searchQuery, ref sourceItems, ref filteredItems);
                UpdateListView(filteredItems);
            }
            Rect resultsListRect = new Rect(2f, EditorGUIUtility.singleLineHeight * 1.5f, rect.width - 5f, rect.height + listViewHeightOffset);
            listView.OnGUI(resultsListRect, e);
        }

        private void FilterList(string query, ref T[] items, ref List<T> filteredItems)
        {
            filteredItems.Clear();

            for (int i = 0; i < items.Length; i++)
            {
                if (filter.IsItemValid(items[i], query))
                {
                    filteredItems.Add(items[i]);
                }
            }
        }

        /// <summary>
        /// Set preselected items, Cell Clicked event will not fire.
        /// </summary>
        /// <param name="selectedItems">Items to select.</param>
        public void SetPreselectedItems(List<T> selectedItems)
        {
            foreach (var item in selectedItems)
                listView.Select(item);
        }

        /// <summary>
        /// Unselect all selected cells, Cell Clicked event will not fire.
        /// </summary>
        public void UnselectAll()
        {
            listView.UnselectAll();
        }

        /// <summary>
        /// Set item as selected, Cell clicked event will not fire.
        /// </summary>
        /// <param name="item">Item to select.</param>
        public void SetPreselectedItem(T item)
        {
            listView.Select(item);
        }

        private void UpdateListView(List<T> items)
        {
            listView.UpdateData(items);
        }

        /// <summary>
        /// Override to handle cell clicked event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnCellClickedHandler(object sender,ListView<T>.OnCellClickedEventArgs e)
        {
        }

        /// <summary>
        /// Override to handler Cell Double Clicked event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnCellDoubleClickedHandler(object sender, ListView<T>.OnSelectionEventArgs e)
        {
        }

        /// <summary>
        /// Fire Selection Finished event.
        /// </summary>
        /// <param name="e"></param>
        protected void OnSelectionFinished(ListView<T>.OnSelectionEventArgs e)
        {
            if (SelectionFinished == null)
            {
                editorWindow.Close();
                return;
            }

            SelectionFinished(this, e);
            editorWindow.Close();
        }

        /// <summary>
        /// Interface to define how search query validates against each item.
        /// </summary>
        public interface Filter
        {
            bool IsItemValid(T item, string query);
        }
    }
}
