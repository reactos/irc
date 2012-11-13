using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TechBot.Library
{
    public abstract class XmlCommand : Command
    {
        protected XmlDocument m_XmlDocument;

        protected XmlCommand()
        {
            m_XmlDocument = new XmlDocument();
            if (Path.IsPathRooted(XmlFile))
                m_XmlDocument.Load(XmlFile);
            else
                m_XmlDocument.Load(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, XmlFile));
        }

        public abstract string XmlFile { get; }

        public XmlDocument XmlDocument
        {
            get { return m_XmlDocument; }
        }
    }
}