using System;
using System.Collections.Generic;
using UnityEngine;

using PizzaPie.Runtime.Util;

namespace PizzaPie.Editor.Example.MultiSelectGameObject
{
    /// <summary>
    /// GameObject name comparer, with natural comparison.
    /// </summary>
    public class GameObjectNameCompararer : IComparer<GameObject>
    {
        private NaturalStringComparer naturalStringComparer = new NaturalStringComparer();

        public int Compare(GameObject x, GameObject y)
        {
            if (x == null && y == null)
                return 0;
            else if (x == null)
                return -1;
            else if (y == null)
                return 1;

            return naturalStringComparer.Compare(x.name, y.name);
        }

    }
}