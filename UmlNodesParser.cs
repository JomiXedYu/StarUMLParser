using System;
using System.Collections.Generic;

namespace StarUMLParser
{
    public class ClassDiagram
    {
        public string name;
        public bool isAbstract;
        public bool isInterface;
    }
    public class Association
    {
        public string assocType;
        public string x;
        public string y;
    }
    public class UmlNodesData
    {
        public List<ClassDiagram> nodes;
        public List<Association> assoc;
    }
    public class UmlNodesParser
    {
        private static Association ParseAssociation(XmiDocument doc, UmlElement association)
        {
            Association ret = new Association();
            if (association is UmlGeneralization)
            {
                ret.assocType = "Generalization";
                UmlGeneralization gen = association as UmlGeneralization;
                ret.x = FindNameByXmiId(doc, gen.parent);
                ret.y = FindNameByXmiId(doc, gen.child);
            }
            else if (association is UmlAssociation)
            {
                UmlAssociation assoc = association as UmlAssociation;
                string mode = assoc.conn.end[0].aggregation;
                ret.assocType = mode;
                ret.x = FindNameByXmiId(doc, assoc.conn.end[0].type);
                ret.y = FindNameByXmiId(doc, assoc.conn.end[1].type);
            }
            else
            {
                throw new ArgumentException("invalid type");
            }
            return ret;
        }

        private static string FindNameByXmiId(XmiDocument xmi, string xmiId)
        {
            foreach (UmlElement item in xmi.content.model.ownedElement)
            {
                if (item is UmlClass)
                {
                    if (item.id == xmiId)
                    {
                        return ((UmlClass)item).name;
                    }
                }
            }
            return null;
        }

        public static UmlNodesData Parse(XmiDocument doc)
        {
            UmlNodesData data = new UmlNodesData()
            {
                nodes = new List<ClassDiagram>(),
                assoc = new List<Association>()
            };

            foreach (UmlElement item in doc.content.model.ownedElement)
            {
                if (item is UmlClass)
                {
                    UmlClass umlClass = item as UmlClass;
                    data.nodes.Add(new ClassDiagram()
                    {
                        isAbstract = umlClass.isAbstract,
                        isInterface = umlClass.isInterface,
                        name = umlClass.name
                    });
                }
                else
                {
                    data.assoc.Add(ParseAssociation(doc, item));
                }
            }
            return data;
        }
    }
}
