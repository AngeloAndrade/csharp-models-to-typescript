﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpModelsToJson
{
    internal static class Util
    {
        internal static bool IsObsolete(SyntaxList<AttributeListSyntax> attributeLists) =>
            attributeLists.Any(attributeList =>
                attributeList.Attributes.Any(attribute =>
                    attribute.Name.ToString().Equals("Obsolete") || attribute.Name.ToString().Equals("ObsoleteAttribute")));

        internal static string GetObsoleteMessage(SyntaxList<AttributeListSyntax> attributeLists)
        {
            foreach (var attributeList in attributeLists)
            {
                var obsoleteAttribute =
                    attributeList.Attributes.FirstOrDefault(attribute =>
                        attribute.Name.ToString().Equals("Obsolete") || attribute.Name.ToString().Equals("ObsoleteAttribute"));

                if (obsoleteAttribute != null)
                {
                    return obsoleteAttribute.ArgumentList == null
                            ? null
                            : obsoleteAttribute.ArgumentList.Arguments.ToString()?.TrimStart('@').Trim('"');
                }
            }

            return null;
        }

        internal static string GetSummaryMessage(SyntaxNode @class)
        {
            var documentComment = @class.GetDocumentationCommentTriviaSyntax();

            if (documentComment == null)
                return null;

            var summaryElement = documentComment.Content
               .OfType<XmlElementSyntax>()
               .FirstOrDefault(_ => _.StartTag.Name.LocalName.Text == "summary");

            if (summaryElement == null)
                return null;

            var summaryText = summaryElement.DescendantTokens()
                .Where(_ => _.Kind() == SyntaxKind.XmlTextLiteralToken)
                .Select(_ => _.Text.Trim())
                .ToList();
                 
            var summaryContent = summaryElement.Content.ToString();
            summaryContent = Regex.Replace(summaryContent, @"^\s*///\s*", string.Empty, RegexOptions.Multiline);
            summaryContent = Regex.Replace(summaryContent, "^<para>", Environment.NewLine, RegexOptions.Multiline);
            summaryContent = Regex.Replace(summaryContent, "</para>", string.Empty);

            return summaryContent.Trim();
        }

        public static DocumentationCommentTriviaSyntax GetDocumentationCommentTriviaSyntax(this SyntaxNode node)
        {
            if (node == null)
            {
                return null;
            }

            foreach (var leadingTrivia in node.GetLeadingTrivia())
            {
                var structure = leadingTrivia.GetStructure() as DocumentationCommentTriviaSyntax;

                if (structure != null)
                {
                    return structure;
                }
            }

            return null;
        }
    }
}
