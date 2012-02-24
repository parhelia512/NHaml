﻿using NUnit.Framework;
using Moq;
using NHaml4.Compilers;
using NHaml4.Parser;
using NHaml4.Walkers.CodeDom;
using NHaml4.Tests.Walkers.CodeDom;
using NHaml4.Parser.Rules;
using NHaml4.IO;
using NHaml4.Tests.Mocks;
using System;
using NHaml4.TemplateBase;

namespace NHaml4.Tests.Walkers
{
    [TestFixture]
    public class HamlDocumentWalker_Tests
    {
        [Test]
        public void Walk_TextNode_AppendsCorrectTag()
        {
            // Arrange
            var content = new HamlLine("Simple content", 0);
            var document = new HamlDocument();
            Type baseType = typeof(Template);
            document.AddChild(new HamlNodeTextContainer(content));

            // Act
            var builder = new ClassBuilderMock();
            new HamlDocumentWalker(builder).Walk(document, "", baseType);

            // Assert
            Assert.That(builder.Build(""), Is.EqualTo(content.Content));
        }

        [Test]
        public void Walk_SingleLineFile_CallsClassBuilderBuild()
        {
            // Arrange
            const string className = "ClassName";
            Type baseType = typeof(Template);
            var document = new HamlTreeParser(new NHaml4.IO.HamlFileLexer()).ParseDocumentSource("Simple content");
            
            // Act
            var builder = new Mock<ITemplateClassBuilder>();
            
            new HamlDocumentWalker(builder.Object).Walk(document, className, baseType);

            // Assert
            builder.Verify(x => x.Build(className, baseType));
        }
    }
}
