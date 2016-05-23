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
    internal class SpecItem : IEquatable<SpecItem>
    {
        public Dictionary<string, Property> Properties { get; private set; }
        public Dictionary<string, string> NumGroupProperties { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
        public string Type { get; set; } = "";

        /// <summary>
        /// Атрибут ключа - Марка
        /// </summary>
        public AttributeReference AtrKey { get; set; }

        /// <summary>
        /// префикс нумерации элемента
        /// </summary>
        public string NumPrefix { get; set; } = "";
        /// <summary>
        /// Значение дополнительного параметра нумерации
        /// </summary>
        public string ExGroupNumbering { get; set; } = "";

        public SpecItem(ObjectId idBlRef)
        {
            IdBlRef = idBlRef;
        }

        /// <summary>
        /// Фильтр блоков. И составление списка всех элементов (1 блок - 1 элемент).
        /// </summary>
        public static List<SpecItem> FilterSpecItems(SelectBlocks sel)
        {
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
                SpecService.Doc.Editor.WriteMessage($"\nНе определены блоки '{SpecService.Optinons.Name}'.");
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

                Property atr;
                if (Properties.TryGetValue(colVal.ColumnSpec.ItemPropName, out atr))
                {
                    if (!colVal.Value.Equals(atr.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        err += $"'{colVal.ColumnSpec.ItemPropName}'='{atr.Value}' " + 
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
                        var atrs = blRef.GetAttributeDictionary();
                        Properties = new Dictionary<string, Property>(atrs.Count, StringComparer.OrdinalIgnoreCase);
                        foreach (var item in atrs)
                        {
                            // Префикс по имени блока и параметра
                            string prefix = string.Empty;
                            SpecService.Optinons.PrefixParam?.TryGetValue(BlName + item.Key, out prefix);                                                        
                            string value = prefix + item.Value.TextString;
                            Property prop = new Property(value, item.Key, item.Value);
                            Properties.Add(item.Key, prop);

                            if (SpecService.Optinons.NumOptions.GroupProperties != null &&
                                SpecService.Optinons.NumOptions.GroupProperties.Contains(item.Key, StringComparer.OrdinalIgnoreCase))
                            {
                                NumGroupProperties.Add(item.Key, value);
                            }
                        }

                        // Проверка типа блока
                        var typeBlock = options.BlocksFilter.Type;
                        if (typeBlock != null)
                        {
                            Property propType;
                            if (Properties.TryGetValue(typeBlock.BlockPropName, out propType))
                            {
                                if (!typeBlock.Name.Equals(propType.Value, StringComparison.OrdinalIgnoreCase))
                                {
                                    // Свойство типа не соответствует требованию  
                                    err += $"Свойство '{typeBlock.BlockPropName}'='{propType.Value}' " + 
                                        $"не соответствует требуемому значению '{typeBlock.Name}'. ";
                                }
                                Type = propType.Value;
                            }
                            // В блоке нет свойства Типа
                            else
                            {
                                err += $"Нет обязательного свойства {typeBlock.BlockPropName}. ";
                            }
                        }
                        
                        // Проверка обязательных атрибутов                              
                        foreach (var atrMustHave in options.BlocksFilter.AttrsMustHave)
                        {
                            if (!Properties.ContainsKey(atrMustHave))
                            {
                                err += $"Нет обязательного свойства: '{atrMustHave}'. ";
                                continue;
                            }                            
                        }

                        // определение Группы
                        Property groupProp;
                        if (Properties.TryGetValue(options.GroupPropName, out groupProp))
                        {
                            Group = groupProp.Value;
                        }

                        // Ключевое свойство
                        Property keyProp;
                        if (Properties.TryGetValue(options.KeyPropName, out keyProp))
                        {
                            Key = keyProp.Value;
                            AtrKey = keyProp.Atr;
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
                   Properties.Count == other.Properties.Count &&
                   Properties.All(i => 
                                    other.Properties.ContainsKey(i.Key) &&
                                    Properties[i.Key].Value.Equals(other.Properties[i.Key].Value)
                                );
        }
    }
}