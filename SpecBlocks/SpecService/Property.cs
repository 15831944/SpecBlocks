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
    internal class Property : IEquatable<Property>
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

        public Property(string value, string key, DBText atr)
        {
            Type = EnumBlockProperty.Attribute;
            Atr = atr as AttributeReference;
            Name =  key;
            Value = value;
        }

        public bool Equals(Property other)
        {
            if (ReferenceEquals(this, other)) return true;
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase) &&
                    Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
