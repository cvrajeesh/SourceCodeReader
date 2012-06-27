
namespace SourceCodeReader.Web.Tests.LanguageServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NUnit.Framework;
using SourceCodeReader.Web.LanguageServices;
using System.Collections;

    [TestFixture]
    public class CSharpSyntaxNavigationBuilderTests
    {

        [Ignore]
        [Test, TestCaseSource(typeof(NavigatableSyntaxTestCasesFactory), "TestCases")]
        public string Tests_For_Builder(string sourceCode)
        {
            // Assemble
            // TODO: Move to setup, for some weired reason, setup is not getting alled from TestDriven.Net
            CSharpSyntaxNavigationBuilder codeNavigationBuilder = new CSharpSyntaxNavigationBuilder();

            // Act
            return codeNavigationBuilder.GetCodeAsNavigatableHtml(sourceCode);
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
            Customer customer = new <a href=""javascript:$.findReferences('ObjectCreation', 'Customer', 135)"">Customer</a>();
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
