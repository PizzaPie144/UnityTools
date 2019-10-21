using System;
using UnityEngine;

using PizzaPie.Editor.Views;

namespace PizzaPie.Editor.Example.MultiSelectGameObject
{
    public class GameObjectNameFilter : SearchWindow<GameObject>.Filter
    {
        public bool IsItemValid(GameObject item, string query)
        {
            string itemName = item != null ? item.name : "None";

            if (query == "" || itemName.IndexOf(query, StringComparison.OrdinalIgnoreCase) > -1)
            {
                return true;
            }

            return false;
        }
    }
}