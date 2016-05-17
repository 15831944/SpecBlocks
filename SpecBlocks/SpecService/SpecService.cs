using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SpecBlocks;
using SpecBlocks.Options;

namespace SpecBlocks
{
    public class SpecService
    {
        private ISpecCustom specCustom;

        public SpecService(ISpecCustom specCustom)
        {
            this.specCustom = specCustom;
        }

        /// <summary>
        /// Создание спецификации
        /// </summary>
        public void CreateSpec()
        {
            SpecOptions specOpt = GetSpecOptions();
            if (specOpt == null)
            {
                throw new Exception("Настройки таблицы не определены.");
            }
            // Клас создания таблицы по заданным настройкам
            SpecTable specTable = new SpecTable(specOpt);
            specTable.CreateTable();
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
