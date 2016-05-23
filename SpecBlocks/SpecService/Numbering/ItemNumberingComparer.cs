using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;

namespace SpecBlocks.Numbering
{
    internal class ItemNumberingComparer : IEqualityComparer<SpecItem>, IComparer<SpecItem>
    {
        public static ItemNumberingComparer New { get; private set; } = new ItemNumberingComparer();
        private static AcadLib.Comparers.AlphanumComparator alphaComparer = AcadLib.Comparers.AlphanumComparator.New;

        public int Compare(SpecItem x, SpecItem y)
        {
            // Если для имени блока задан префикс, то сравнение по префиксу
            var res = string.Compare(x.NumPrefix, y.NumPrefix);
            if(res != 0)
            {
                return res;
            }

            // Сравнение объектов по параметрам группировки
            foreach (var xProp in x.NumGroupProperties)
            {                
                var yProp = y.Properties[xProp.Key];
                res = alphaComparer.Compare(xProp.Value, yProp.Value);
                if(res != 0)
                {
                    return res;                    
                }
            }
            return 0;
        }

        public bool Equals(SpecItem x, SpecItem y)
        {
            var res = string.Equals(x.NumPrefix, y.NumPrefix);
            if (res)
            {
                res = x.NumGroupProperties.SequenceEqual(y.NumGroupProperties);
            }
            return res;
        }

        public int GetHashCode(SpecItem obj)
        {
            if (obj == null) return 0;
            if (!string.IsNullOrEmpty(obj.NumPrefix))
            {
                return obj.Type.GetHashCode() ^ obj.Group.GetHashCode() ^ obj.NumPrefix.GetHashCode();                
            }
            else
            {
                return obj.Type.GetHashCode() ^ obj.Group.GetHashCode();
            }            
        }
    }
}
