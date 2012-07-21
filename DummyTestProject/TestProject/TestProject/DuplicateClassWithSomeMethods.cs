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

        public void Method2(string parameter)
        {
            Console.WriteLine("Calling Method2 with " + parameter);
        }
    }
}
