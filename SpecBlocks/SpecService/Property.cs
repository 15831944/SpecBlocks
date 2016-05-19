using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using SpecBlocks.Options;

namespace SpecBlocks
{
    /// <summary>
    /// Свойство элемента
    /// </summary>
    internal class Property 
    {       
        /// <summary>
        /// Тип свойства - атрибут или дин свойство
        /// </summary>
        public EnumBlockProperty Type { get; set; }
        /// <summary>
        /// Имя свойства
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Значение
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Атрибут блока
        /// </summary>
        public AttributeReference Atr { get; set; }

        public Property(DBText value, string key)
        {
            Type = EnumBlockProperty.Attribute;
            Atr = value as AttributeReference;
            Name =  key;
            Value = value.TextString;
        }
    }
}
