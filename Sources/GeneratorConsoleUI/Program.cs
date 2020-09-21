using System.Web.Mvc;
namespace GeneratorConsoleUI
{

    internal class Program
    {
        private static void Main(string[] args)
        {

            /*
                code généré attendu : 
                {
                    'controller1' : {
                        'action1': _getUrl("controller1","action1"),
                        'action2': _getUrl("controller1","action2"),
                        'action3': _getUrl("controller1","action3")
                    },
                    'controller2' : {
                        'action1': _getUrl("controller2","action1"),
                        'action2': _getUrl("controller2","action2"),
                        'action3': _getUrl("controller2","action3")
                    },
                    ...
                }

            */
            Run(args[0]);

            Pause();
        }

        public static void Run(string assemblyPath)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(assemblyPath);
            AssemblyJsSerializer.MvcActionSerializer<Controller, ActionResult> serializer =
                new AssemblyJsSerializer.MvcActionSerializer<Controller, ActionResult>(assembly);
            serializer.FieldFormat = "()=>_getUrl(\"{0}\",\"{1}\")";
            serializer.SerializeToFile("c:\\test.tst");

        }

        private static void Pause()
        {
            System.Console.WriteLine("Pause...");
            System.Console.ReadLine();
        }
    }
}
