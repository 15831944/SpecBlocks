using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using SpecBlocks;
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
            IsNumbering = false;
        }

        /// <summary>
        /// Создание спецификации
        /// </summary>
        public void CreateSpec()
        {            
            Optinons = GetSpecOptions();
            if (Optinons == null)
            {
                throw new Exception("Настройки таблицы не определены.");
            }
            // Клас создания таблицы по заданным настройкам
            SpecTable specTable = new SpecTable();
            specTable.CreateTable();
        }

        public List<IGrouping<SpecItem, SpecItem>> SelectAndGroupBlocks()
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

        public SpecOptions GetSpecOptions()
        {
            SpecOptions specOptions = null;
            if (File.Exists(specCustom.File))
            {
                try
                {
                    // Загрузка настроек таблицы из файла XML
                    specOptions = SpecOptions.Load(specCustom.File);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, $"Ошибка при попытке загрузки настроек таблицы из XML файла {specCustom.File}");
                }
            }

            if (specOptions == null)
            {
                // Создать дефолтные
                specOptions = specCustom.GetDefaultOptions();
                // Сохранение дефолтных настроек 
                try
                {
                    specOptions.Save(specCustom.File);
                }
                catch (Exception exSave)
                {
                    Logger.Log.Error(exSave, $"Попытка сохранение настроек в файл {specCustom.File}");
                }
            }

            return specOptions;
        }
    }
}
