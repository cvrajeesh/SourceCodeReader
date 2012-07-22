namespace TestProject
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class DuplicateClassWithSomeMethods
    {
        public void Method1()
        {
            Console.WriteLine("Calling Method1");
        }

        public ITest Test { get; set; }

        public DuplicateClassWithSomeMethods()
        {
            MyStruct mystruct = new MyStruct();
            mystruct.StructMethod();
            MyEnum myEnum = MyEnum.Test;
            if (myEnum == MyEnum.Test)
            {
                Console.WriteLine("");
            }
        }

        public void Method2(string parameter, ITest myTest)
        {
            Console.WriteLine("Calling Method2 with " + parameter);
            this.Test.StartTesting();
        }
    }
}
