using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;

namespace SpecBlocks.Numbering
{
    public class ItemNumberingComparer : IEqualityComparer<SpecItem>, IComparer<SpecItem>
    {
        public static ItemNumberingComparer New { get; private set; } = new ItemNumberingComparer();
        private static AcadLib.Comparers.AlphanumComparator alphaComparer = AcadLib.Comparers.AlphanumComparator.New;

        public int Compare(SpecItem x, SpecItem y)
        { 
            // Сравнение объектов по параметрам MastHaveParamsWoKey
            foreach (var iX in x.MastHaveParamsWoKey)
            {
                var iYValue = y.MastHaveParamsWoKey[iX.Key];
                var res = alphaComparer.Compare(iX.Value, iYValue);
                if(res != 0)
                {
                    return res;                    
                }
            }
            return 0;
        }

        public bool Equals(SpecItem x, SpecItem y)
        {
            var res = x.MastHaveParamsWoKey.SequenceEqual(y.MastHaveParamsWoKey);
            return res;
        }

        public int GetHashCode(SpecItem obj)
        {
            if (obj == null) return 0;
            return obj.Type.GetHashCode() ^ obj.Group.GetHashCode();
        }
    }
}
