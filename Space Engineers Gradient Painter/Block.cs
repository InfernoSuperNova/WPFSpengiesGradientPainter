using System.Windows.Media;
using System.Xml;
using VRageMath;

namespace Space_Engineers_Gradient_Painter
{


    internal class Block
    {
        public Vector3 position;

        public Vector3 colour;

        public System.Windows.Media.Color rawColour;

        public bool doesExist;

        public XmlNode block;

        public Block(Vector3 position, Vector3 colour, bool doesExist)
        {
            this.position = position;
            this.colour = colour;
            this.doesExist = doesExist;
        }
    }

}
