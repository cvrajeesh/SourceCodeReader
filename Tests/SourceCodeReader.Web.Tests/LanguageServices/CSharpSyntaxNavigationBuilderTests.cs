
namespace SourceCodeReader.Web.Tests.LanguageServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
    using SourceCodeReader.Web.LanguageServices;
    using System.Collections;
    using SourceCodeReader.Web.LanguageServices.DotNet;

    [TestFixture]
    public class CSharpSyntaxNavigationBuilderTests
    {
        private DotNetSyntaxNavigationBuilder codeNavigationBuilder;
        [SetUp]
        public void SetUp()
        {
            codeNavigationBuilder = new DotNetSyntaxNavigationBuilder();
        }

        [Ignore]
        [Test, TestCaseSource(typeof(NavigatableSyntaxTestCasesFactory), "TestCases")]
        public string Tests_For_Builder(string sourceCode)
        {
            // Assemble
            // TODO: Move to setup, for some weired reason, setup is not getting alled from TestDriven.Net
           // CSharpSyntaxNavigationBuilder codeNavigationBuilder = new CSharpSyntaxNavigationBuilder();

            // Act
           // return codeNavigationBuilder.GetCodeAsNavigatableHtml(sourceCode);
            throw new NotImplementedException();
        }

        public void Can_Build_Navigation_For_ObjectCreation()
        {
            // Assemble
            var sourceCode = @"
namespace Testing
{
    public class Test
    {
        public void TestMethod()
        {
            Customer customer = new Customer();
        }
    }
}";

            var expectedResultPattern = @"
namespace Testing
{
    public class Test
    {
        public void TestMethod()
        {
            Customer customer = new <a href=""javascript:$.findReferences('ObjectCreation', 'Customer', .+)"">Customer</a>();
        }
    }
}";

            // Act
          //  var result = codeNavigationBuilder.GetCodeAsNavigatableHtml(sourceCode);

            // Assert
            Assert.That(sourceCode, Is.StringMatching(expectedResultPattern));
        }
        
    }

    public class NavigatableSyntaxTestCasesFactory
    {
        public static IEnumerable TestCases
        {
            get
            {
            
                yield return new TestCaseData(
@"
namespace Testing
{
    public class Test
    {
        public void TestMethod()
        {
            Customer customer = new Customer();
        }
    }
}")
  .SetName("Can_Build_Navigation_For_ObjectCreation").Returns(
@"
namespace Testing
{
    public class Test
    {
        public void TestMethod()
        {
            Customer customer = new <a href=""javascript:$.findReferences('ObjectCreation', 'Customer', *)"">Customer</a>();
        }
    }
}");

                yield return new TestCaseData(
@"
namespace Testing
{
    public class Test
    {
        public void TestMethod()
        {
            Customer customer = new Customer();
            string result = customer.GetFullName();
        }
    }
}")
  .SetName("Can_Build_Navigation_For_MethodCall").Returns(
@"
namespace Testing
{
    public class Test
    {
        public void TestMethod()
        {
            Customer customer = new <a href=""javascript:$.findReferences('ObjectCreation', 'Customer', 135)"">Customer</a>();
            string result = customer.<a href=""javascript:$.findReferences('MethodCall', 'GetFullName', 185)"">GetFullName</a>();
        }
    }
}");


            }
        }
    }
}
