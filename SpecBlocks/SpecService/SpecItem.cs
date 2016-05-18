using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AcadLib.Errors;
using AcadLib.Extensions;
using Autodesk.AutoCAD.DatabaseServices;
using System.Drawing;
using SpecBlocks.Options;

namespace SpecBlocks
{
    /// <summary>
    /// Элемент спецификации
    /// </summary>
    public class SpecItem : IEquatable<SpecItem>
    {
        public Dictionary<string, DBText> AttrsDict { get; private set; }
        public Dictionary<string, string> MastHaveParamsWoKey { get; private set; }
        public string BlName { get; private set; }
        // Название группы для элемента
        public string Group { get; private set; } = "";
        public ObjectId IdBlRef { get; private set; }
        /// <summary>
        /// Ключевое свойство элемента - обычно это Марка элемента.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Тип блока
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Атрибут ключа - Марка
        /// </summary>
        public AttributeReference AtrKey { get; set; }

        public SpecItem(ObjectId idBlRef)
        {
            IdBlRef = idBlRef;
        }

        /// <summary>
        /// Фильтр блоков. И составление списка всех элементов (1 блок - 1 элемент).
        /// </summary>
        public static List<SpecItem> FilterSpecItems(SelectBlocks sel)
        {
            // обновления полей в чертеже
            //SpecService.Doc.Database.EvaluateFields();

            List<SpecItem> items = new List<SpecItem>();
            List<ObjectId> idBlRefsFiltered = new List<ObjectId>();
            // Обработка блоков и отбор блоков монолитных конструкций
            foreach (var idBlRef in sel.IdsBlRefSelected)
            {
                SpecItem specItem = new SpecItem(idBlRef);
                if (specItem.Define(SpecService.Optinons))
                {
                    items.Add(specItem);
                    idBlRefsFiltered.Add(idBlRef);
                }
            }

            if (items.Count == 0)
            {
                //throw new Exception("Не определены блоки монолитных конструкций.");
                SpecService.Doc.Editor.WriteMessage("\nНе определены блоки монолитных конструкций.");
                return items;                
            }
            else
            {
                SpecService.Doc.Editor.WriteMessage($"\nОтобрано блоков для спецификации: {items.Count}.");
            }

            // Проверка дубликатов
            if (SpecService.Optinons.CheckDublicates)
            {
                AcadLib.Blocks.Dublicate.CheckDublicateBlocks.Check(idBlRefsFiltered);
            }

            return items;
        }

        /// <summary>
        /// Проверка соответствия значениям в столбцах
        /// </summary>
        /// <param name="columnsValue"></param>
        public void CheckColumnsValue(List<ColumnValue> columnsValue)
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
                        err += $"'{colVal.ColumnSpec.ItemPropName}'='{atr.TextString}' " + 
                            $"не соответствует эталонному значению '{colVal.Value}', " + 
                            $"'{SpecService.Optinons.KeyPropName}' = '{Key}'.\n";
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
                Inspector.AddError($"Ошибки в блоке {BlName}: {err} " + 
                    $"Этот блок попадет в спецификацию с эталонными значениями.", IdBlRef, icon: SystemIcons.Error);
            }
        }

        public bool Define(SpecOptions options)
        {
            if (IdBlRef.IsNull)
            {
                Logger.Log.Error($"Ошибка в методе SpecItem.Define() - IdBlRef.IsNull. Недопустимая ситуация.");
                return false;
            }
            using (var blRef = IdBlRef.GetObject(OpenMode.ForRead, false, true) as BlockReference)
            {

                if (blRef == null)
                {
                    Logger.Log.Error($"Ошибка в методе SpecItem.Define() - blRef == null. Недопустимая ситуация.");
                    return false;
                }

                string err = string.Empty;
                BlName = blRef.GetEffectiveName();

                if (Regex.IsMatch(BlName, options.BlocksFilter.BlockNameMatch, RegexOptions.IgnoreCase))
                {
                    if (blRef.AttributeCollection == null)
                    {
                        // В блоке нет атрибутов.            
                        err += "Нет атрибутов. ";
                    }
                    else
                    {
                        // Обновление полей в блоке
                        AcadLib.Field.UpdateField.Update(blRef.Id);                        

                        // все атрибуты блока
                        AttrsDict = blRef.GetAttributeDictionary();                        

                        // Проверка типа блока
                        var typeBlock = options.BlocksFilter.Type;
                        if (typeBlock != null)
                        {
                            DBText atrType;
                            if (AttrsDict.TryGetValue(typeBlock.BlockPropName, out atrType))
                            {
                                if (!typeBlock.Name.Equals(atrType.TextString, StringComparison.OrdinalIgnoreCase))
                                {
                                    // Свойство типа не соответствует требованию  
                                    err += $"Свойство '{typeBlock.BlockPropName}'='{atrType.TextString}' " + 
                                        $"не соответствует требуемому значению '{typeBlock.Name}'. ";
                                }
                                Type = atrType.TextString;
                            }
                            // В блоке нет свойства Типа
                            else
                            {
                                err += $"Нет обязательного свойства {typeBlock.BlockPropName}. ";
                            }
                        }

                        MastHaveParamsWoKey = new Dictionary<string, string>();
                        // Проверка обязательных атрибутов                              
                        foreach (var atrMustHave in options.BlocksFilter.AttrsMustHave)
                        {
                            if (!AttrsDict.ContainsKey(atrMustHave))
                            {
                                err += $"Нет обязательного свойства: '{atrMustHave}'. ";
                            }
                            if (!atrMustHave.Equals(options.KeyPropName, StringComparison.OrdinalIgnoreCase))
                            {
                                MastHaveParamsWoKey.Add(atrMustHave, AttrsDict[atrMustHave].TextString);
                            }
                        }

                        // определение Группы
                        DBText groupAttr;
                        if (AttrsDict.TryGetValue(options.GroupPropName, out groupAttr))
                        {
                            Group = groupAttr.TextString;
                        }

                        // Ключевое свойство
                        DBText keyAttr;
                        if (AttrsDict.TryGetValue(options.KeyPropName, out keyAttr))
                        {
                            Key = keyAttr.TextString;
                            AtrKey = keyAttr as AttributeReference;
                        }
                        else
                        {
                            err += $"Не определено ключевое свойство '{options.KeyPropName}'. ";
                        }
                    }

                    if (string.IsNullOrEmpty(err))
                    {
                        //// Добавлен блок в спецификацию
                        //Inspector.AddError($"{BlName}, {specTable.SpecOptions.KeyPropName}='{Key}'", blRef, icon: SystemIcons.Information);
                        return true;
                    }
                    else
                    {
                        Inspector.AddError($"Пропущен блок '{BlName}': {err}", blRef, icon: SystemIcons.Warning);
                        return false;
                    }
                }
                // Имя блока не соответствует Regex.IsMatch
                else
                {
                    //err += $"Имя блока не соответствует '{specTable.SpecOptions.BlocksFilter.BlockNameMatch}'. ";
                    return false;
                }
            }         
        }

        public override int GetHashCode()
        {
            return Group.GetHashCode() ^ Key.GetHashCode();
        }

        public bool Equals(SpecItem other)
        {
            if (ReferenceEquals(this, other)) return true;

            return Group.Equals(other.Group) &&
                   Key.Equals(other.Key) &&
                   AttrsDict.Count == other.AttrsDict.Count &&
                   AttrsDict.All(i => 
                                    other.AttrsDict.ContainsKey(i.Key) &&
                                    AttrsDict[i.Key].TextString.Equals(other.AttrsDict[i.Key].TextString)
                                );
        }
    }
}