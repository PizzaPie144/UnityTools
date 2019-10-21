using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using PizzaPie.Editor.Views;

namespace PizzaPie.Editor.Example.MultiSelectGameObject
{
    public class GameObjectCellView : CellView<GameObject>
    {
        public GameObjectCellView(Vector2 dimensions) : base(dimensions)
        {
        }

        public override CellView<GameObject> Clone()
        {
            return new GameObjectCellView(dimensions);
        }

        protected override void OnGUIInternal(GameObject data, Rect viewRect, bool IsSelected, Event e)
        {
            EditorGUI.DrawRect(viewRect, IsSelected ? GUI.skin.settings.selectionColor : Color.clear);
            string label = data != null ? data.name : "None";
            GUI.BeginGroup(viewRect, label);
            GUI.EndGroup();
        }

        protected override void OnClick(bool isSelected)
        {
        }

        protected override void OnDoubleClick(bool isSelected)
        {
        }
    }
}
