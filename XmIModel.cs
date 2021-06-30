using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

public class XmiConfig
{
    public const string Namespace = "href://org.omg/UML/1.3";
}

public class XmiMetamodel
{
    [XmlAttribute("xmi.name")]
    public string name;
    [XmlAttribute("xmi.version")]
    public string version;
}

public class XmiHeader
{
    [XmlElement("XMI.metamodel")]
    public XmiMetamodel metamodel;

}


public class XmiContentUmlModel
{
    [XmlAttribute("name")]
    public string name;

    [XmlAttribute("xmi.id")]
    public string id;

    [XmlArray("Namespace.ownedElement")]
    [XmlArrayItem(Type = typeof(UmlClass))]
    [XmlArrayItem(Type = typeof(UmlGeneralization))]
    [XmlArrayItem(Type = typeof(UmlAssociation))]
    public List<UmlElement> ownedElement;
}


public class XmiContent
{
    [XmlElement("Model", Namespace = XmiConfig.Namespace)]
    public XmiContentUmlModel model;
}

[XmlRoot("XMI")]
public class XmiDocument
{
    [XmlElement("XMI.header")]
    public XmiHeader header;
    [XmlElement("XMI.content")]
    public XmiContent content;
}

[XmlInclude(typeof(UmlClassifierFeatureAttribute))]
[XmlInclude(typeof(UmlClassifierFeatureOperation))]
public class UmlClassifierFeatureElement
{
    [XmlAttribute("name")]
    public string name;
    [XmlAttribute("visibility")]
    public string visibility;
    [XmlAttribute("xmi.id")]
    public string id;
}
[XmlType("Attribute", Namespace = XmiConfig.Namespace)]
public class UmlClassifierFeatureAttribute : UmlClassifierFeatureElement
{

}
[XmlType("Operation", Namespace = XmiConfig.Namespace)]
public class UmlClassifierFeatureOperation : UmlClassifierFeatureElement
{

}
[XmlInclude(typeof(UmlClass))]
[XmlInclude(typeof(UmlGeneralization))]
[XmlInclude(typeof(UmlAssociation))]
[XmlType("Element", Namespace = XmiConfig.Namespace)]
public class UmlElement
{
    [XmlAttribute("namespace")]
    public string @namespace;
    [XmlAttribute("xmi.id")]
    public string id;
}
[XmlType("Class", Namespace = XmiConfig.Namespace)]
public class UmlClass : UmlElement
{
    [XmlAttribute("name")]
    public string name;
    [XmlAttribute("isAbstract")]
    public bool isAbstract;
    [XmlAttribute("isInterface")]
    public bool isInterface;

    [XmlArray("Classifier.feature")]
    [XmlArrayItem(Type = typeof(UmlClassifierFeatureAttribute))]
    [XmlArrayItem(Type = typeof(UmlClassifierFeatureOperation))]
    public List<UmlClassifierFeatureElement> elements;
}
[XmlType("Generalization", Namespace = XmiConfig.Namespace)]
public class UmlGeneralization : UmlElement
{
    [XmlAttribute("name")]
    public string name;
    [XmlAttribute("parent")]
    public string parent;
    [XmlAttribute("child")]
    public string child;
}

public class UmlAssociationEnd
{
    [XmlAttribute("aggregation")]
    public string aggregation;
    [XmlAttribute("association")]
    public string association;
    [XmlAttribute("type")]
    public string type;
    [XmlAttribute("xmi.id")]
    public string id;
}

public class UmlAssociationConnection
{
    [XmlElement("AssociationEnd")]
    public List<UmlAssociationEnd> end;
}


[XmlType("Association", Namespace = XmiConfig.Namespace)]
public class UmlAssociation : UmlElement
{
    [XmlElement("Association.connection")]
    public UmlAssociationConnection conn;
}