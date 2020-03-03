using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

using iChronoMe.Core.Types;

using Newtonsoft.Json;

namespace iChronoMe.Core.Classes
{
    public class SmoothXmlSerializer
    {
        TimeSpan tsMapping = TimeSpan.FromTicks(0);

        public void Serialize(TextWriter textWriter, object o)
        {
            tsMapping = TimeSpan.FromTicks(0);
            var swStart = DateTime.Now;
            /*string json = JsonConvert.SerializeObject(o);
            json.ToString();
            var tsJson = DateTime.Now - swStart;
            swStart = DateTime.Now;*/

            var doc = new XmlDocument();

            if (o.GetType().Namespace.StartsWith("System.Collections") || o.GetType().IsArray)
            {
                string cName = "Array";
                string cType = null;
                foreach (var gt in o.GetType().GenericTypeArguments)
                {
                    cType = gt.Name;
                    cName += "Of" + cType;
                }
                var root = doc.CreateElement(cName);
                doc.AppendChild(root);

                AddCollection(root, o, cType);
            }
            else
            {
                var root = doc.CreateElement(o.GetType().Name);
                doc.AppendChild(root);
                SerializeObject(root, o);
            }

            doc.Save(textWriter);
            var tsXml = DateTime.Now - swStart;
            xLog.Debug(o.GetType().Name + ": " + tsXml.TotalMilliseconds + "    mapping: " + tsMapping.TotalMilliseconds);
#if DEBxxUG
            var tsXml = DateTime.Now - swStart;
            int iJson = json.Length;
            int iXml = doc.OuterXml.Length;
            float nPercent = tsXml.Ticks * 100 / tsJson.Ticks;

            this.ToString();

            if (textWriter is StreamWriter)
            {
                if ((textWriter as StreamWriter).BaseStream is FileStream)
                {
                    string cXmlFile = ((textWriter as StreamWriter).BaseStream as FileStream).Name;
                    File.WriteAllText(cXmlFile + ".json", json);
                }
            }
#endif
        }

        public T Deserialize<T>(TextReader textReader)
        {
            tsMapping = TimeSpan.FromTicks(0);
            var swStart = DateTime.Now;
            XmlDocument doc = new XmlDocument();
            doc.Load(textReader);
            var root = doc.DocumentElement;
            try
            {
                if (typeof(T).Namespace.StartsWith("System.Collections") || typeof(T).IsArray)
                {
                    Type baseType = null;
                    foreach (var gt in typeof(T).GenericTypeArguments)
                    {
                        baseType = gt;
                    }

                    return (T)DeserializeCollection(root, baseType, typeof(T));
                }
                else
                {
                    return (T)DeserializeObject(root, typeof(T));
                }
            }
            finally
            {
                var tsXml = DateTime.Now - swStart;
                xLog.Debug(typeof(T).Name + ": " + tsXml.TotalMilliseconds + "    mapping: " + tsMapping.TotalMilliseconds);

#if DEBxxUG
                var tsXml = DateTime.Now - swStart;
                if (textReader is StreamReader)
                {
                    if ((textReader as StreamReader).BaseStream is FileStream)
                    {
                        string cXmlFile = ((textReader as StreamReader).BaseStream as FileStream).Name;
                        if (File.Exists(cXmlFile+".new.json"))
                        {
                            swStart = DateTime.Now;
                            var o = JsonConvert.DeserializeObject<T>(File.ReadAllText(cXmlFile + ".new.json"));
                            var tsJson = DateTime.Now - swStart;
                            float nPercent = tsXml.Ticks * 100 / tsJson.Ticks;
                            this.ToString();
                        }
                    }
                }
#endif
            }
        }

        static Dictionary<string, Dictionary<string, string>> mappingPropertyNames = new Dictionary<string, Dictionary<string, string>>();
        static Dictionary<string, Dictionary<string, string>> mappingPropertyNamesReverse = new Dictionary<string, Dictionary<string, string>>();
        static Dictionary<string, List<string>> mappingIncludeClasses = new Dictionary<string, List<string>>();
        static Dictionary<string, List<string>> mappingCreateSubElements = new Dictionary<string, List<string>>();
        static Dictionary<string, string> enumMappings = new Dictionary<string, string>();
        static Dictionary<string, object> enumMappingsReverse = new Dictionary<string, object>();
        static Dictionary<string, string> classMappings = new Dictionary<string, string>();
        static Dictionary<string, Assembly> typeAssemblies = new Dictionary<string, Assembly>();

        private void SerializeObject(XmlElement el, object o)
        {
            string cFullCassName = o.GetType().FullName;

            if (!mappingPropertyNames.ContainsKey(cFullCassName))
            {
                CheckObjectMapping(o.GetType());
            }
            var mapping = mappingPropertyNames[cFullCassName];
            var includeClasses = mappingIncludeClasses[cFullCassName];
            var createSubElements = mappingCreateSubElements[cFullCassName];

            Type t = o.GetType();
            if (includeClasses.Count > 1)
                el.SetAttribute("_type", t.Name);
            if (mapping.Count == 0)
                el.InnerText = GetXmlValue(o);
            else
            {
                foreach (string cProperty in mapping.Keys)
                {
                    try
                    {
                        string cXmlName = mapping[cProperty];

                        var val = o.GetType().GetProperty(cProperty)?.GetValue(o) ?? o.GetType().GetField(cProperty)?.GetValue(o) ?? null;
                        if (val == null)
                            continue;

                        if (val.GetType().Namespace.StartsWith("System.Collections") || val.GetType().IsArray)
                        {
                            AddCollection(el, val, cXmlName);
                        }
                        else
                        {
                            if (createSubElements.Contains(cProperty))
                            {
                                var sub = el.OwnerDocument.CreateElement(cXmlName);
                                el.AppendChild(sub);

                                SerializeObject(sub, val);
                            }
                            else
                            {
                                string cVal = GetXmlValue(val);
                                el.SetAttribute(cXmlName, cVal);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }
            }
        }

        string GetXmlValue(object val)
        {
            string cVal;
            if (val is string)
                cVal = (string)val;
            else if (val is bool)
                cVal = (bool)val ? "1" : "0";
            else if (val is int)
                cVal = ((int)val).ToString(CultureInfo.InvariantCulture);
            else if (val is double)
                cVal = ((double)val).ToString(CultureInfo.InvariantCulture);
            else if (val is float)
                cVal = ((float)val).ToString(CultureInfo.InvariantCulture);
            else if (val is DateTime)
                cVal = ((DateTime)val).ToString("s", CultureInfo.InvariantCulture);
            else if (val is Enum)
                cVal = GetEnumValue((Enum)val);
            else if (val is xColor)
                cVal = ((xColor)val).ToHex(true);
            else
                cVal = string.Format("{0}", val);
            return cVal;
        }

        void AddCollection(XmlElement el, object o, string cMainXmlName)
        {
            if (o is IDictionary)
            {
                var dict = o as IDictionary;
                foreach (var x in dict)
                {
                    string cXmlName = string.IsNullOrEmpty(cMainXmlName) ? x.GetType().GetProperty("Value").GetType().Name : cMainXmlName;
                    var sub = el.OwnerDocument.CreateElement(cXmlName);
                    sub.SetAttribute("_key", x.GetType().GetProperty("Key").GetValue(x).ToString());
                    el.AppendChild(sub);

                    SerializeObject(sub, x.GetType().GetProperty("Value").GetValue(x));
                }
            }
            else if (o is IList)
            {
                var list = o as IList;
                foreach (var x in list)
                {
                    string cXmlName = string.IsNullOrEmpty(cMainXmlName) ? x.GetType().Name : cMainXmlName;
                    var sub = el.OwnerDocument.CreateElement(cXmlName);
                    el.AppendChild(sub);

                    SerializeObject(sub, x);
                }
            }
        }
        private string GetEnumValue(Enum e)
        {
            return enumMappings[string.Format("{0}_{1}", e.GetType().Name, e)];
        }

        private void CheckObjectMapping(Type t)
        {
            string cFullCassName = t.FullName;

            if (!mappingPropertyNames.ContainsKey(cFullCassName))
            {
                var swStart = DateTime.Now;
                var mapping = new Dictionary<string, string>();
                var includeClasses = new List<Type>() { t };
                var createSubElements = new List<string>();

                try
                {
                    var bt = t.BaseType;
                    while (bt != null)
                    {
                        foreach (var att in bt.CustomAttributes)
                        {
                            if (att.AttributeType == typeof(XmlIncludeAttribute))
                            {
                                foreach (var arg in att.ConstructorArguments)
                                {
                                    if (arg.Value.ToString().Contains(t.FullName))
                                        includeClasses.Add(bt);
                                }
                            }
                        }
                        bt.ToString();
                        bt = bt.BaseType;
                    }
                    if (includeClasses.Contains(t))
                    {
                        foreach (var inf in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (inf.CanRead && inf.CanWrite)
                            {
                                string cXmlName = inf.Name;
                                bool bCreateNewElement = !IsSimple(inf.PropertyType);
                                foreach (var attr in inf.GetCustomAttributes())
                                {
                                    if (attr is XmlIgnoreAttribute)
                                        cXmlName = null;
                                    if (attr is XmlElementAttribute)
                                    {
                                        var x = (attr as XmlElementAttribute);
                                        if (!string.IsNullOrEmpty(x.ElementName))
                                            cXmlName = x.ElementName;
                                        bCreateNewElement = true;
                                    }
                                    if (attr is XmlAttributeAttribute)
                                    {
                                        var x = (attr as XmlAttributeAttribute);
                                        if (!string.IsNullOrEmpty(x.AttributeName))
                                            cXmlName = x.AttributeName;
                                        bCreateNewElement = false;
                                    }
                                }
                                if (inf.PropertyType == typeof(xColor))
                                    bCreateNewElement = false;
                                if (inf.PropertyType.IsSubclassOf(typeof(Enum)))
                                    CheckEnumMapping(inf.PropertyType);
                                if (!string.IsNullOrEmpty(cXmlName))
                                {
                                    inf.PropertyType.ToString();

                                    mapping.Add(inf.Name, cXmlName);
                                    if (bCreateNewElement)
                                        createSubElements.Add(inf.Name);
                                }
                            }
                        }
                        foreach (var inf in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
                        {
                            if (inf.IsPublic && !inf.IsStatic)
                            {
                                string cXmlName = inf.Name;
                                bool bCreateNewElement = !IsSimple(inf.FieldType);
                                foreach (var attr in inf.GetCustomAttributes())
                                {
                                    if (attr is XmlIgnoreAttribute)
                                        cXmlName = null;
                                    if (attr is XmlElementAttribute)
                                    {
                                        var x = (attr as XmlElementAttribute);
                                        if (!string.IsNullOrEmpty(x.ElementName))
                                            cXmlName = x.ElementName;
                                        bCreateNewElement = true;
                                    }
                                    if (attr is XmlAttributeAttribute)
                                    {
                                        var x = (attr as XmlAttributeAttribute);
                                        if (!string.IsNullOrEmpty(x.AttributeName))
                                            cXmlName = x.AttributeName;
                                        bCreateNewElement = false;
                                    }
                                }
                                if (inf.FieldType == typeof(xColor))
                                    bCreateNewElement = false;
                                if (inf.FieldType.IsSubclassOf(typeof(Enum)))
                                    CheckEnumMapping(inf.FieldType);
                                if (!string.IsNullOrEmpty(cXmlName))
                                {
                                    inf.FieldType.ToString();

                                    mapping.Add(inf.Name, cXmlName);
                                    if (bCreateNewElement)
                                        createSubElements.Add(inf.Name);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                try
                {
                    List<string> includeClassesNames = new List<string>();
                    foreach (var x in includeClasses)
                    {
                        includeClassesNames.Add(x.FullName);
                        typeAssemblies[x.FullName] = x.Assembly;
                    }

                    mappingPropertyNames.Add(cFullCassName, mapping);
                    mappingIncludeClasses.Add(cFullCassName, includeClassesNames);
                    mappingCreateSubElements.Add(cFullCassName, createSubElements);
                    classMappings.Add(t.Name, cFullCassName);
                    var mappingReverse = new Dictionary<string, string>();
                    foreach (string key in mapping.Keys)
                    {
                        string val = mapping[key];
                        mappingReverse.Add(val, key);
                    }
                    mappingPropertyNamesReverse.Add(cFullCassName, mappingReverse);

                    foreach (var tSub in t.Assembly.GetTypes())
                    {
                        if (tSub.IsSubclassOf(t))
                            CheckObjectMapping(tSub);
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                tsMapping += DateTime.Now - swStart;
            }
        }

        private void CheckEnumMapping(Type t)
        {
            string cT = t.Name + "_";
            if (!enumMappings.ContainsKey(cT))
            {
                enumMappings.Add(cT, "done");
                bool bUseString = false;

                foreach (var att in t.CustomAttributes)
                {
                    if (att.AttributeType == typeof(XmlTypeAttribute))
                    {
                        foreach (var arg in att.ConstructorArguments)
                        {
                            if ("string".Equals(arg.Value))
                                bUseString = true;
                        }
                    }
                }

                foreach (var x in Enum.GetValues(t))
                {
                    string cId = string.Format("{0}{1}", cT, x);
                    string cVal = bUseString ? x.ToString() : ((int)x).ToString();

                    var Z = t.GetField(x.ToString());
                    foreach (var a in Z.GetCustomAttributes())
                    {
                        if (a is XmlEnumAttribute)
                            cVal = (a as XmlEnumAttribute).Name;
                    }
                    enumMappings.Add(cId, cVal);
                    enumMappingsReverse.Add(string.Format("{0}{1}", cT, cVal), x);
                }
            }
        }

        private ICollection DeserializeCollection(XmlElement node, Type baseType, Type collectionType = null, string cXmlName = null)
        {
            if (baseType == null && collectionType != null)
                baseType = collectionType.GenericTypeArguments[0];
            if (collectionType == null)
            {
                Type objectType = typeof(List<>);
                collectionType = objectType.MakeGenericType(baseType);
            }

            //List<object> list = new List<object>();
            //var res = typeof(T).GetConstructor(new Type[0]).Invoke(new Type[0]);
            var collection = Activator.CreateInstance(collectionType);
            var add = collectionType.GetMethod("Add");
            if (add == null)
                return (ICollection)collection;

            try
            {
                foreach (XmlNode el in node.ChildNodes)
                {
                    object o = null;
                    if (el is XmlText)
                        o = GetObjectFromXmlValue(baseType, el.InnerText, null);
                    else if (el is XmlElement)
                    {
                        if (el.Name.Equals(cXmlName) || el.Name.ToLower().Equals(baseType.Name.ToLower()))
                        {
                            if (baseType == typeof(string) || baseType == typeof(int) || baseType == typeof(double) || baseType == typeof(float) || baseType == typeof(bool) || baseType == typeof(DateTime))
                                o = DeserializeProperty((XmlElement)el, null, baseType, null);
                            else
                                o = DeserializeObject((XmlElement)el, baseType);
                        }
                        else
                            this.ToString();
                    }
                    if (o != null)
                        add.Invoke(collection, new object[] { o });
                }
            }
            catch (Exception ex)
            {
                sys.LogException(ex);
            }

            if (baseType.IsArray)
                return null;

            return (ICollection)collection;
        }

        private object DeserializeObject(XmlElement node, Type baseType, object instance = null)
        {
            if (baseType == typeof(xColor) && !string.IsNullOrEmpty(node.InnerText))
                return xColor.FromHex(node.InnerText);

            string cFullCassName = baseType.FullName;

            if (!mappingPropertyNames.ContainsKey(cFullCassName))
            {
                CheckObjectMapping(baseType);
            }
            if (!mappingPropertyNames.ContainsKey(cFullCassName))
                throw new Exception("No Mapping found for Node " + node.Name);

            string cNodeType = node.GetAttribute("_type");
            if (string.IsNullOrEmpty(cNodeType))
                cNodeType = node.GetAttribute("xsi:type");
            if (!string.IsNullOrEmpty(cNodeType))
            {                
                baseType = typeAssemblies[classMappings[cNodeType]].GetType(classMappings[cNodeType]);
                cFullCassName = baseType.FullName;
            }
            if (baseType == null)
                throw new Exception("BaseType unknown");

            try
            {
                if (instance == null)
                    instance = Activator.CreateInstance(baseType);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            if (instance == null)
                throw new Exception("unable to create Instance of " + baseType.FullName);

            var mapping = mappingPropertyNames[cFullCassName];
            var mappingRev = mappingPropertyNamesReverse[cFullCassName];
            var includeClasses = mappingIncludeClasses[cFullCassName];
            var createSubElements = mappingCreateSubElements[cFullCassName];

            foreach (XmlAttribute attr in node.Attributes)
            {
                try
                {
                    string cXmlName = attr.Name;
                    string cProperty = cXmlName;
                    if (mappingRev.ContainsKey(cXmlName))
                        cProperty = mappingRev[cXmlName];

                    var prop = baseType.GetProperty(cProperty);
                    var field = baseType.GetField(cProperty);
                    MemberInfo member = prop == null ? field as MemberInfo : prop as MemberInfo;

                    if (member == null)
                        continue;

                    object oVal = null;
                    Type t = null;
                    try
                    {
                        if (prop != null)
                        {
                            if (!prop.CanWrite)
                                continue;
                            t = prop.PropertyType;
                            oVal = prop.GetValue(instance);
                        }
                        else if (field != null)
                        {
                            t = field.FieldType;
                            oVal = field.GetValue(instance);
                        }
                    }
                    catch (Exception ex)
                    {
                        xLog.Error(ex, "DeserializeObject: try get memeber type and value");
                    }
                    if (t == null)
                        continue;

                    oVal = DeserializeProperty(node, cProperty, t, cXmlName, oVal, createSubElements.Contains(cProperty));

                    if (oVal != null)
                    {
                        if (prop != null)
                            prop.SetValue(instance, oVal);
                        else if (field != null)
                            field.SetValue(instance, oVal);
                    }
                }

                catch (Exception ex)
                {
                    ex.ToString();
                }
            }

            int iNode = 0;
            foreach (XmlNode elSub in node)
            {
                iNode++;
                try
                {
                    string cXmlName = elSub.Name;
                    string cProperty = cXmlName;
                    if (mappingRev.ContainsKey(cXmlName))
                        cProperty = mappingRev[cXmlName];

                    var prop = baseType.GetProperty(cProperty);
                    var field = baseType.GetField(cProperty);
                    MemberInfo member = prop == null ? field as MemberInfo : prop as MemberInfo;

                    if (member == null)
                        continue;

                    object oVal = null;
                    Type t = null;
                    try
                    {
                        if (prop != null)
                        {
                            if (!prop.CanWrite)
                                continue;
                            t = prop.PropertyType;
                            oVal = prop.GetValue(instance);
                        }
                        else if (field != null)
                        {
                            t = field.FieldType;
                            oVal = field.GetValue(instance);
                        }
                    }
                    catch (Exception ex)
                    {
                        xLog.Error(ex, "DeserializeObject: SubNode: try get memeber type and value");
                    }
                    if (t == null)
                        continue;

                    if (t.Namespace.StartsWith("System.Collections") || t.IsArray)
                    {
                        var tItem = t.IsArray ?
                            Type.GetType(t.Namespace + "." + t.Name.Substring(0, t.Name.Length - 2)) :
                            t.GenericTypeArguments[0];

                        if (elSub.HasChildNodes && IsSimple(tItem))
                            elSub.ToString();

                        if ((elSub.HasChildNodes && IsSimple(tItem) && string.IsNullOrEmpty(elSub.InnerText)))
                            oVal = DeserializeCollection((XmlElement)elSub, tItem, t);
                        else
                        {
                            if (oVal == null)
                            {
                                if (t.IsArray)
                                {
                                    Type objectType = typeof(List<>);
                                    var collectionType = objectType.MakeGenericType(t);
                                    oVal = Activator.CreateInstance(t, node.ChildNodes.Count);
                                }
                                else
                                {
                                    oVal = Activator.CreateInstance(t);
                                }
                                if (prop != null)
                                    prop.SetValue(instance, oVal);
                                else if (field != null)
                                    field.SetValue(instance, oVal);
                            }
                            var add = t.GetMethod("Add");

                            object o = null;
                            if (elSub is XmlText)
                                o = GetObjectFromXmlValue(tItem, elSub.InnerText, null);
                            else if (elSub is XmlElement)
                            {
                                if (IsSimple(tItem))
                                    o = DeserializeProperty((XmlElement)elSub, null, tItem, null);
                                else
                                    o = DeserializeObject((XmlElement)elSub, tItem);
                            }
                            if (o != null)
                            {
                                if (t.IsArray)
                                    (oVal as Array).SetValue(o, iNode - 1);
                                else
                                    add.Invoke(oVal, new object[] { o });
                            }

                            oVal = null; //wird nur beim neu erstellen der collection gesetzt
                        }
                    }
                    else
                    {
                        if (elSub is XmlText)
                            oVal = GetObjectFromXmlValue(t, elSub.InnerText, null);
                        else if (IsSimple(t))
                            oVal = DeserializeProperty((XmlElement)elSub, null, t, null);
                        else
                            oVal = DeserializeObject((XmlElement)elSub, t);
                    }

                    if (oVal != null)
                    {
                        if (prop != null)
                            prop.SetValue(instance, oVal);
                        else if (field != null)
                            field.SetValue(instance, oVal);
                    }
                }

                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
            return instance;
        }

        private object DeserializeProperty(XmlElement node, string cProperty, Type t, string cXmlName, object oVal = null, bool bCheckSubNodes = false)
        {

            if (t.Namespace.StartsWith("System.Collections") && (bCheckSubNodes))
            {
                oVal = DeserializeCollection(node, t.GenericTypeArguments[0], t, cProperty);
            }
            else
            {
                if (!string.IsNullOrEmpty(cXmlName) && !node.HasAttribute(cXmlName))
                {
                    if (bCheckSubNodes || t == typeof(xColor))
                    {
                        foreach (XmlElement elSub in node)
                        {
                            if (elSub.Name.Equals(cXmlName))
                            {
                                oVal = DeserializeObject(elSub, t, oVal);
                                break;
                            }
                        }
                    }
                    else
                    {
                        foreach (XmlElement elSub in node)
                        {
                            if (elSub.Name.Equals(cXmlName))
                            {
                                if (t.Namespace.StartsWith("System.Collections") || t.IsArray)
                                    oVal = DeserializeCollection(elSub, t.GenericTypeArguments[0], t, cProperty);
                                else
                                    oVal = DeserializeProperty(elSub, null, t, null, oVal);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    string cXmlValue = string.IsNullOrEmpty(cXmlName) ? node.InnerText : node.GetAttribute(cXmlName);
                    oVal = GetObjectFromXmlValue(t, cXmlValue, oVal);
                }
            }
            return oVal;
        }

        private object GetObjectFromXmlValue(Type t, string cXmlValue, object oVal)
        {
            if (t == typeof(string))
                oVal = cXmlValue;
            else if (t == typeof(bool))
                oVal = "1".Equals(cXmlValue) || "true".Equals(cXmlValue);
            else if (t == typeof(int))
            {
                int i;
                int.TryParse(cXmlValue, out i);
                oVal = i;
            }
            else if (t == typeof(double))
            {
                double d;
                double.TryParse(cXmlValue, NumberStyles.Float, CultureInfo.InvariantCulture, out d);
                oVal = d;
            }
            else if (t == typeof(float))
            {
                float d;
                float.TryParse(cXmlValue, NumberStyles.Float, CultureInfo.InvariantCulture, out d);
                oVal = d;
            }
            else if (t == typeof(DateTime))
            {
                DateTime dt;
                DateTime.TryParse(cXmlValue, out dt);
                oVal = dt;
            }
            else if (t.IsSubclassOf(typeof(Enum)))
            {
                oVal = DeserializeEnumValue(t, cXmlValue);
            }
            else if (t == typeof(xColor))
                oVal = xColor.FromHex(cXmlValue);
            else
                cXmlValue.ToString();//cVal = string.Format("{0}", val);
            return oVal;
        }

        private object DeserializeEnumValue(Type enumType, string cValue)
        {
            string cId = string.Format("{0}_{1}", enumType.Name, cValue);
            if (enumMappingsReverse.ContainsKey(cId))
                return enumMappingsReverse[cId];
            int i;
            if (int.TryParse(cValue, out i))
            {
                return Enum.ToObject(enumType, i);
            }
            return Enum.Parse(enumType, cValue);
        }

        private bool IsSimple(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // nullable type, check if the nested type is simple.
                return IsSimple(type.GetGenericArguments()[0]);
            }
            return type.IsPrimitive
              || type.IsEnum
              || type.Equals(typeof(string))
              || type.Equals(typeof(decimal))
              || type.Equals(typeof(DateTime));
        }
    }
}