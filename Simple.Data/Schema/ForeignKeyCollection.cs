using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Simple.Data.Schema
{
    class ForeignKeyCollection : KeyedCollection<string, ForeignKey>
    {
        /// <summary>
        /// When implemented in a derived class, extracts the key from the specified element.
        /// </summary>
        /// <returns>
        /// The key for the specified element.
        /// </returns>
        /// <param name="item">The element from which to extract the key.</param>
        protected override string GetKeyForItem(ForeignKey item)
        {
            return item.MasterTable;
        }
    }
}
