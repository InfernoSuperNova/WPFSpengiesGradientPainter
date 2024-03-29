using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Space_Engineers_Gradient_Painter
{
    internal class SEBlueprint : SEFolder
    {
        public SEBlueprint(string path, int level, MainWindow mainWindow)
    : base(path, level, mainWindow)
        {
            base.directoryPath = path;
            base.directoryName = Path.GetFileName(path);
        }

        public override string ToString()
        {
            string indent = "";
            for (int i = 0; i < base.level; i++)
            {
                indent += "  ";
            }
            return indent + "**" + base.directoryName + "\n";
        }
    }
}
