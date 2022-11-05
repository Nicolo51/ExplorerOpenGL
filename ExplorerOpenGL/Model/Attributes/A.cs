using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExplorerOpenGL.Model.Attributes
{
    public class mere 
    {
        public StringBuilder xml;

        public int id;
        public string someData; 
        public mere()
        {
            xml = new StringBuilder();
            someData = "test";
            id = 1; 
        }
        public virtual XDocument GetXml()
        {
            XDocument doc = new XDocument();
            //tajoute tes nodes et tes truc 
            doc.Add(new XElement("map", 
                new XElement("id", id), 
                new XElement("someData", someData)));
            return doc;
        }
    }
    public class fille : mere
    {
        float otherData; 
        public fille()
            :base()
        {
            otherData = 2f; 
        }
        public override XDocument GetXml()
        {
            XDocument doc = base.GetXml(); // tu recupères ce que ta classe mere a déjà enregistré sur le xml
            XElement concat = new XElement("otherdata", otherData); //tu gères les éléments propres a la classe fille
            doc.Element("map").Add(concat);// tu ajoutes les éléments de ta classe fille a l'xml généré par ta classe mère
            return doc; //tu return tout 
        }
    }
}
