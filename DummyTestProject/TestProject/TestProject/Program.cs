using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestProject
{
    public enum MyEnum
    {
        Test,
        Test1
    }

    public struct MyStruct
    {
        public MyStruct()
        {
        }

        public void StructMethod()
        {            
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ClassWithSomeMethods objectWithSomeMethods = new ClassWithSomeMethods();
            DuplicateClassWithSomeMethods duplicateObjectWithSomeMethods = new DuplicateClassWithSomeMethods();
            objectWithSomeMethods.Method1();
            objectWithSomeMethods.Method2("Param 1");
            duplicateObjectWithSomeMethods.Method1();
        }
    }

    public interface ITest
    {
        void StartTesting();
    }

    public class Test : ITest
    {
        public void StartTesting()
        {
            throw new NotImplementedException();
        }
    }
}
