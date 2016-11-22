using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using robot_editor.Interfaces;

namespace robot_editor.Classes
{
    public class VariableBase :  IVariable
    {
        private string _comment = string.Empty;
        private string _declaration = string.Empty;
        private BitmapImage _icon;
        private string _name = string.Empty;
        private int _offset = -1;
        private string _path = string.Empty;
        private string _type = string.Empty;
        private string _value = string.Empty;
        public bool IsSelected { get; set; }


        #region Description
        /// <summary>
        /// The <see cref="Description" /> property's name.
        /// </summary>
        public const string DescriptionPropertyName = "Description";

        private string _description = string.Empty;

        /// <summary>
        /// Sets and gets the Description property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Description { get; set; }
        #endregion


        public BitmapImage Icon { get; set; } 

        public string Name { get; set; }

        public string Type { get; set; }

        public string Path { get; set; }

        public string Value { get; set; }

        public int Offset { get; set; }

        public string Comment { get; set; }

        public string Declaration { get; set; }
    }
}
