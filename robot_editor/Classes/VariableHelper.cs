using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;

namespace robot_editor.Classes
{
    public class VariableHelper
    {
        public static ICollectionView PositionView { get; set; }
        public static ICollectionView PositionCollection { get; set; }


        public static Match FindMatches(Regex matchstring, string filename)
        {
            var text = File.ReadAllText(filename);


            // Dont Include Empty Values
            if (string.IsNullOrEmpty(matchstring.ToString())) return null;

            var m = matchstring.Match(text.ToLower());
            return m;
        }



    }
}
