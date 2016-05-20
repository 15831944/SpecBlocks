using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using SpecBlocks;
using SpecBlocks.Numbering;
using SpecBlocks.Options;

namespace SpecBlocks
{
    public class SpecService
    {
        private ISpecCustom specCustom;
        public static SpecOptions Optinons { get; private set; }
        internal static Document Doc { get; private set; }
        internal static bool IsNumbering { get; private set; }

        public SpecService(ISpecCustom specCustom)
        {
            Doc = Application.DocumentManager.MdiActiveDocument;
            this.specCustom = specCustom;            
        }

        /// <summary>
        /// Создание спецификации
        /// </summary>
        public void CreateSpec()
        {
            IsNumbering = false;
            Optinons = GetSpecOptions();
            if (Optinons == null)
            {
                throw new Exception("Настройки таблицы не определены.");
            }
            // Клас создания таблицы по заданным настройкам
            SpecTable specTable = new SpecTable();
            specTable.CreateTable();
        }

        /// <summary>
        /// Нумерация элементов по настройкам
        /// </summary>
        public void Numbering()
        {
            IsNumbering = true;
            Database db = Doc.Database;
            ItemNumberingComparer iNumComparer = ItemNumberingComparer.New;

            using (var t = db.TransactionManager.StartTransaction())
            {                
                // Сгруппировано по префиксу
                var groups = SelectAndGroupBlocks();
                
                foreach (var groupByPrefix in groups)
                {
                    // группировка по размерам и прочему
                    var sizes = groupByPrefix.GroupBy(g => g, iNumComparer).OrderByDescending(o => o.Key, iNumComparer);
                    int index = 1;
                    foreach (var size in sizes)
                    {
                        // группировка по доп параметру нумерации
                        var groupSizeExNums = size.GroupBy(g => g.ExGroupNumbering).OrderBy(o=>o.Key, AcadLib.Comparers.AlphanumComparator.New);
                        bool hasExNum = groupSizeExNums.Skip(1).Any();
                        int exIndex = 1;
                        foreach (var sizeExNums in groupSizeExNums)
                        {                            
                            foreach (var item in sizeExNums)
                            {
                                if (item.AtrKey != null)
                                {
                                    item.AtrKey.UpgradeOpen();
                                    string mark = item.NumPrefix + index + (hasExNum ? "." + exIndex.ToString(): "");
                                    item.AtrKey.TextString = mark;
                                    item.Key = mark;
                                    Inspector.AddError($"{item.BlName} {SpecService.Optinons.KeyPropName}={item.Key}", item.IdBlRef,
                                            icon: System.Drawing.SystemIcons.Information);
                                }
                            }
                            exIndex++;
                        }
                        index++;
                    }                    
                }
                t.Commit();
            }
        }

        /// <summary>
        /// Выбор и группировка блоков по префиксу
        /// </summary>
        /// <returns></returns>
        internal List<IGrouping<string, SpecItem>> SelectAndGroupBlocks()
        {
            IsNumbering = true;
            Optinons = GetSpecOptions();
            if (Optinons == null)            
                throw new Exception("Настройки таблицы не определены.");
            // Выбор блоков
            var sel = new SelectBlocks();
            sel.Select();
            // Фильтр блоков
            var items = SpecItem.FilterSpecItems(sel);
            // Группировка элементов
            return SpecGroup.GroupingForNumbering(items);
        }

        internal SpecOptions GetSpecOptions()
        {
            SpecOptions specOptions = null;
            //if (File.Exists(specCustom.File))
            //{
            //    try
            //    {
            //        // Загрузка настроек таблицы из файла XML
            //        specOptions = SpecOptions.Load(specCustom.File);
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.Log.Error(ex, $"Ошибка при попытке загрузки настроек таблицы из XML файла {specCustom.File}");
            //    }
            //}

            if (specOptions == null)
            {
                // Создать дефолтные
                specOptions = specCustom.GetDefaultOptions();
                // Сохранение дефолтных настроек 
                //try
                //{
                //    specOptions.Save(specCustom.File);
                //}
                //catch (Exception exSave)
                //{
                //    Logger.Log.Error(exSave, $"Попытка сохранение настроек в файл {specCustom.File}");
                //}
            }
            return specOptions;
        }
    }
}
