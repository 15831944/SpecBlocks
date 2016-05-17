using System;
using System.Collections.Generic;
using System.Linq;
using AcadLib.Errors;
using SpecBlocks.Numbering;

namespace SpecBlocks
{
    /// <summary>
    /// Группирование элементов в спецификации
    /// </summary>
    public class SpecGroup
    {
        public string Name { get; private set; }
        /// <summary>
        /// Уникальные строки элементов таблицы - по ключевому свойству
        /// </summary>
        public List<SpecRecord> Records { get; private set; } = new List<SpecRecord>();

        public SpecGroup(string name)
        {
            Name = name;
        }

        public static List<SpecGroup> Grouping(List<SpecItem> items)
        {
            List<SpecGroup> groups = new List<SpecGroup>();
            var itemsGroupBy = items.GroupBy(i => i.Group).OrderBy(g => g.Key);
            foreach (var itemGroup in itemsGroupBy)
            {
                SpecGroup group = new SpecGroup(itemGroup.Key);
                group.Calc(itemGroup);
                // проверка уникальности элементов в группе
                group.Check();
                groups.Add(group);
            }
            return groups;
        }

        internal static List<IGrouping<SpecItem, SpecItem>> GroupingForNumbering(List<SpecItem> items)
        {
            ItemNumberingComparer iNumComparer = ItemNumberingComparer.New;
            List<SpecItem> groups = new List<SpecItem>();            
            var itemsGroupBy = items.GroupBy(i => i, iNumComparer).OrderByDescending(g => g.Key, iNumComparer);
            return itemsGroupBy.ToList();
        }

        public void Calc(IGrouping<string, SpecItem> itemGroup)
        {
            // itemGroup - элементы одной группы.

            // Нужно сгруппировать по ключевому свойству
            //var uniqRecs = itemGroup.GroupBy(m => m.Key).OrderBy(m => m.Key, new AcadLib.Comparers.AlphanumComparator());

            //  Группировка по уникальным значениям ключа
            var uniqRecs = itemGroup.GroupBy(m => m).OrderBy(m => m.Key.Key, new AcadLib.Comparers.AlphanumComparator());

            //var groups = piles.GroupBy(g => new { g.View, g.TopPileAfterBeat, g.TopPileAfterCut, g.BottomGrillage, g.PilePike })
            //                    .OrderBy(g => g.Key.View, AcadLib.Comparers.AlphanumComparator.New);
                        
            foreach (var urec in uniqRecs)
            {                
                SpecRecord rec = new SpecRecord(urec.Key.Key, urec.ToList());
                Records.Add(rec);                

                // Добавление элементов определенных групп в инспектор для показа пользователю                
                if (!SpecService.IsNumbering)
                {
                    foreach (var item in urec)
                    {
                        Inspector.AddError($"{item.BlName} {SpecService.Optinons.KeyPropName}={item.Key}", item.IdBlRef,
                            icon: System.Drawing.SystemIcons.Information);
                    }
                }               
            }

            // Дублирование марки
            if (!SpecService.IsNumbering)
            {
                var errRecsDublKey = uniqRecs.GroupBy(g => g.Key.Key).Where(w => w.Skip(1).Any());
                foreach (var errRecDublKey in errRecsDublKey)
                {
                    int i = 0;
                    foreach (var items in errRecDublKey)
                    {
                        i++;
                        foreach (var rec in items)
                        {
                            Inspector.AddError($"Дублирование марки в блоке {rec.BlName} {SpecService.Optinons.KeyPropName}='{rec.Key}'-{i}, такая марка уже определена с другими параметрами блока.", rec.IdBlRef,
                            icon: System.Drawing.SystemIcons.Warning);
                        }
                    }
                }
            }           
        }        

        /// <summary>
        /// Проверка группы
        /// </summary>
        public void Check()
        {
            Records.ForEach(r => r.CheckRecords());
        }
    }
}