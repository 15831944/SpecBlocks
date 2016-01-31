using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AcadLib.Errors;
using AcadLib.Extensions;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpecBlocks
{
   /// <summary>
   /// Элемент спецификации
   /// </summary>
   internal class SpecItem
   {
      public Dictionary<string, DBText> AttrsDict { get; private set; }
      public string BlName { get; private set; }
      // Название группы для элемента
      public string Group { get; private set; } = "";
      public ObjectId IdBlRef { get; private set; }
      /// <summary>
      /// Ключевое свойство элемента - обычно это Марка элемента.
      /// </summary>
      public string Key { get; private set; }

      public SpecItem(ObjectId idBlRef)
      {
         IdBlRef = idBlRef;
      }

      /// <summary>
      /// Фильтр блоков. И составление списка всех элементов (1 блок - 1 элемент).
      /// </summary>
      public static List<SpecItem> FilterSpecItems(SpecTable specTable)
      {
         List<SpecItem> items = new List<SpecItem>();
         // Обработка блоков и отбор блоков монолитных конструкций
         foreach (var idBlRef in specTable.SelBlocks.IdsBlRefSelected)
         {
            SpecItem specItem = new SpecItem(idBlRef);
            if (specItem.Define(specTable))
            {
               items.Add(specItem);
            }
         }

         if (items.Count == 0)
         {
            throw new Exception("Не определены блоки монолитных конструкций.");
         }
         else
         {
            specTable.Doc.Editor.WriteMessage($"\nОтобрано блоков для спецификации: {items.Count}\n");
         }
         return items;
      }

      /// <summary>
      /// Проверка соответствия значениям в столбцах
      /// </summary>
      /// <param name="columnsValue"></param>
      public void CheckColumnsValur(List<ColumnValue> columnsValue, SpecTable specTable)
      {
         string err = string.Empty;
         foreach (var colVal in columnsValue)
         {
            if (colVal.ColumnSpec.ItemPropName == "Count")
            {
               continue;
            }

            DBText atr;
            if (AttrsDict.TryGetValue(colVal.ColumnSpec.ItemPropName, out atr))
            {
               if (!colVal.Value.Equals(atr.TextString, StringComparison.OrdinalIgnoreCase))
               {
                  err += $"'{colVal.ColumnSpec.ItemPropName}'='{atr.TextString}' не соответствует эталонному значению '{colVal.Value}', '{specTable.SpecOptions.KeyPropName}' = '{Key}'.\n";
               }
            }
            else
            {
               // В элементе вообще нет свойства для этого столбца
               err += $"Не определено свойство '{colVal.ColumnSpec.ItemPropName}'.\n";
            }
         }
         if (!string.IsNullOrEmpty(err))
         {
            Inspector.AddError($"Ошибки в блоке {BlName}: {err} Этот блок попадет в спецификацию с эталонными значениями.", IdBlRef);
         }
      }

      public bool Define(SpecTable specTable)
      {
         if (IdBlRef.IsNull)
         {
            Logger.Log.Error($"Ошибка в методе SpecItem.Define() - IdBlRef.IsNull. Недопустимая ситуация.");
            return false;
         }
         var blRef = IdBlRef.GetObject(OpenMode.ForRead, false, true) as BlockReference;
         if (blRef == null)
         {
            Logger.Log.Error($"Ошибка в методе SpecItem.Define() - blRef == null. Недопустимая ситуация.");
            return false;
         }

         string err = string.Empty;
         BlName = blRef.GetEffectiveName();

         if (blRef.AttributeCollection == null)
         {
            // В блоке нет атрибутов.            
            err += "Нет атрибутов. ";
         }
         else
         {            
            if (Regex.IsMatch(BlName, specTable.SpecOptions.BlocksFilter.BlockNameMatch, RegexOptions.IgnoreCase))
            {
               // все атрибуты блока
               AttrsDict = blRef.GetAttributeDictionary();

               // Проверка типа блока
               var typeBlock = specTable.SpecOptions.BlocksFilter.Type;
               if (typeBlock != null)
               {
                  DBText atrType;
                  if (AttrsDict.TryGetValue(typeBlock.BlockPropName, out atrType))
                  {
                     if (!typeBlock.Name.Equals(atrType.TextString, StringComparison.OrdinalIgnoreCase))
                     {
                        // Свойство типа не соответствует требованию  
                        err += $"Свойство '{typeBlock.BlockPropName}'='{atrType.TextString}' не соответствует требуемому значению '{typeBlock.Name}'. ";                      
                     }
                  }
                  // В блоке нет свойства Типа
                  else
                  {
                     err += $"Нет обязательного свойства {typeBlock.BlockPropName}. ";
                  }
               }

               // Проверка обязательных атрибутов                              
               foreach (var atrMustHave in specTable.SpecOptions.BlocksFilter.AttrsMustHave)
               {
                  if (!AttrsDict.ContainsKey(atrMustHave))
                  {                     
                     err += $"Нет обязательного свойства: '{atrMustHave}'. ";                     
                  }
               }

               // определение Группы
               DBText groupAttr;
               if (AttrsDict.TryGetValue(specTable.SpecOptions.GroupPropName, out groupAttr))
               {
                  Group = groupAttr.TextString;
               }

               // Ключевое свойство
               DBText keyAttr;
               if (AttrsDict.TryGetValue(specTable.SpecOptions.KeyPropName, out keyAttr))
               {
                  Key = keyAttr.TextString;
               }
               else
               {
                  err += $"Не определено ключевое свойство '{specTable.SpecOptions.KeyPropName}'. ";                  
               }
            }
            // Имя блока не соответствует Regex.IsMatch
            else
            {
               err += $"Имя блока не соответствует '{specTable.SpecOptions.BlocksFilter.BlockNameMatch}'. ";               
            }            
         }

         if (string.IsNullOrEmpty(err))
         {
            return true;
         }
         else
         {
            Inspector.AddError($"Пропущен блок '{BlName}': {err}", blRef);
            return false;
         }
      }
   }
}