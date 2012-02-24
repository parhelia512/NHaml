﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHaml4.Compilers;
using NHaml4.Parser;
using NHaml4.Crosscutting;
using NHaml4.Parser.Rules;

namespace NHaml4.Walkers.CodeDom
{
    public class HamlNodeTagWalker : HamlNodeWalker, INodeWalker
    {
        public HamlNodeTagWalker(ITemplateClassBuilder classBuilder, HamlHtmlOptions options)
            : base(classBuilder, options)
        { }

        public override void Walk(HamlNode node)
        {
            var nodeTag = node as HamlNodeTag;
            if (nodeTag == null)
                throw new InvalidCastException("HamlNodeTagWalker requires that HamlNode object be of type HamlNodeTag.");

            AppendTagStart(nodeTag);
            AppendAttributes(nodeTag);
            AppendTagBodyAndClose(nodeTag);
        }

        private void AppendTagStart(HamlNodeTag nodeTag)
        {
            if (nodeTag.IsLeadingWhitespaceTrimmed == false)
                ClassBuilder.Append(nodeTag.Indent);

            ClassBuilder.Append("<" + nodeTag.NamespaceQualifiedTagName);
        }

        private void AppendAttributes(HamlNodeTag nodeTag)
        {
            MakeClassAttribute(nodeTag);
            MakeIdAttribute(nodeTag);
            WalkHtmlStyleAttributes(nodeTag);
        }

        private void MakeClassAttribute(HamlNodeTag nodeTag)
        {
            var classValues = new List<string>();
            classValues.AddRange(from collection in nodeTag.Children
                                 from attr in collection.Children.OfType<HamlNodeHtmlAttribute>()
                                 where ((HamlNodeHtmlAttribute)attr).Name == "class"
                                 from attrFragment in attr.Children
                                 select attrFragment.Content);

            classValues.AddRange(nodeTag.Children.OfType<HamlNodeTagClass>()
                .Select(x => " " + x.Content));

            if (classValues.Any())
            {
                classValues[0] = classValues[0].Trim();
                ClassBuilder.AppendAttributeNameValuePair("class", classValues, '\'');
            }
        }

        private void MakeIdAttribute(HamlNodeTag nodeTag)
        {
            var idValues = new List<string>();
            idValues.AddRange(from collection in nodeTag.Children
                              from attr in collection.Children.OfType<HamlNodeHtmlAttribute>()
                              where ((HamlNodeHtmlAttribute)attr).Name == "id"
                              from attrFragment in attr.Children
                              select attrFragment.Content);


            var idTag = nodeTag.Children.LastOrDefault(x => x.GetType() == typeof(HamlNodeTagId));
            if (idTag != null) idValues.Insert(0, idTag.Content);

            if (idValues.Any())
            {
                for (int c = idValues.Count-1; c > 0; c--)
                    idValues.Insert(c, "_");
                ClassBuilder.AppendAttributeNameValuePair("id", idValues, '\'');
            }
        }

        private void WalkHtmlStyleAttributes(HamlNodeTag nodeTag)
        {
            var attributeTags = nodeTag.Children.Where(x => x.GetType() == typeof(HamlNodeHtmlAttributeCollection));
            foreach (var child in attributeTags)
            {
                new HamlNodeHtmlAttributeCollectionWalker(ClassBuilder, Options)
                    .Walk(child);
            }
        }

        private void AppendTagBodyAndClose(HamlNodeTag nodeTag)
        {
            if (nodeTag.IsSelfClosing || Options.IsAutoClosingTag(nodeTag.TagName))
                ClassBuilder.AppendSelfClosingTagSuffix();
            else
            {
                ClassBuilder.Append(">");
                base.Walk(nodeTag);
                if (IsPreCloseTagWhitespaceTrimmed(nodeTag))
                {
                    ClassBuilder.AppendFormat("</{0}>", nodeTag.NamespaceQualifiedTagName);
                }
                else
                {
                    ClassBuilder.AppendNewLine();
                    ClassBuilder.AppendFormat(nodeTag.Indent + "</{0}>", nodeTag.NamespaceQualifiedTagName);
                }
            }
        }

        private bool IsPreCloseTagWhitespaceTrimmed(HamlNodeTag nodeTag)
        {
            if (nodeTag.IsMultiLine == false)
                return true;
            else if (nodeTag.WhitespaceRemoval == WhitespaceRemoval.Internal)
                return true;

            var lastNonWhitespaceChild = GetLastNonWhitespaceChild(nodeTag) as HamlNodeTag;
            if (lastNonWhitespaceChild == null)
                return false;
            
            return (lastNonWhitespaceChild).WhitespaceRemoval == WhitespaceRemoval.Surrounding;
        }

        private HamlNode GetLastNonWhitespaceChild(HamlNodeTag nodeTag)
        {
            return nodeTag.Children.LastOrDefault(x => x.IsWhitespaceNode() == false);
        }
    }
}
