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
            string jsActions = Run(args[0]);

            System.Console.WriteLine(jsActions);
            Pause();
        }

        public static string Run(string assemblyPath)
        {
            AssemblyJsSerializer.MvcActionSerializer serializer = new AssemblyJsSerializer.MvcActionSerializer();
            return serializer.SerializeFormat<System.Web.Mvc.Controller, System.Web.Mvc.ActionResult>(
                                System.Reflection.Assembly.LoadFile(assemblyPath),
                                "()=>_getUrl(\"{0}\",\"{1}\")");

        }

        private static void Pause()
        {
            System.Console.WriteLine("Pause...");
            System.Console.ReadLine();
        }
    }
}
