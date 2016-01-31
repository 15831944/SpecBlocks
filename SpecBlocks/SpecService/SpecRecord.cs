using System.Collections.Generic;
using System.Linq;

namespace SpecBlocks
{
   /// <summary>
   /// Обна строка в таблице - элементы с одним ключом
   /// </summary>
   public class SpecRecord
   {
      public List<ColumnValue> ColumnsValue { get; private set; } = new List<ColumnValue>();
      public int Count { get; set; }
      public List<SpecItem> Items { get; set; }
      public string Key { get; set; }

      public SpecRecord(string key, List<SpecItem> items, SpecTable specTable)
      {
         Key = key;
         Items = items ?? new List<SpecItem>();
         Count = Items.Count;
         // Составление строки таблицы
         foreach (var column in specTable.SpecOptions.TableOptions.Columns)
         {
            ColumnValue colVal = new ColumnValue(column);
            // Кол
            if (column.ItemPropName == "Count")
            {
               colVal.Value = Items.Count.ToString();
            }
            else
            {
               // Поиск первой значащей записи в элементах или пустая строка
               var itemSpec = Items.FirstOrDefault(i => i.AttrsDict.ContainsKey(column.ItemPropName));
               if (itemSpec != null)
               {
                  colVal.Value = itemSpec.AttrsDict[column.ItemPropName].TextString;
               }
            }
            ColumnsValue.Add(colVal);
         }
      }

      /// <summary>
      /// Проверка записей - должны быть одинаковые свойства у всех элементов
      /// </summary>
      public void CheckRecords(SpecTable specTable)
      {
         //Inspector.AddError("Пока не реализована проверка блоков с одним ключом но различающимися остальными свойствами. Скоро сделаю.");
         // TODO: Проверка - все свойства элементов должны совпадать между собой
         // Отличающиеся элементы вывести в инспектор.

         // Все записи должны соответствовать значениям в ColumnsValue
         Items.ForEach(i => i.CheckColumnsValur(ColumnsValue, specTable));
      }
   }
}