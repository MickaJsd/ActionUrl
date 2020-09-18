using System.Runtime.Serialization;
using System;
using System.Collections;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting.Messaging;
using System.Runtime.CompilerServices;

using System.Web.Mvc;
namespace GeneratorConsoleUI
{
    //internal class Controller
    //{
    //}

    //internal class actionResult
    //{
    //}

    //internal class jsonResult : actionResult
    //{
    //}

    //internal class HomeController : Controller
    //{
    //    public actionResult index()
    //    {
    //        return null;
    //    }

    //    public jsonResult create()
    //    {
    //        return null;
    //    }
    //}

    //internal class UserController : Controller
    //{
    //    public void index()
    //    {
    //    }

    //    public jsonResult create()
    //    {
    //        return null;
    //    }
    //}

    internal class Program
    {
        private static void Main(string[] args)
        {
            
            var serializer = new AssemblyJsSerializer.MvcActionSerializer();
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
            var jsActions = serializer.Serialize<Controller, ActionResult>(
                                Assembly.LoadFile(args[0]),
                                (type, method) => $"()=>_getUrl(\"{serializer.GetTypeName(type)}\",\"{serializer.GetMethodName(method)}\")");

            Console.WriteLine(jsActions);
            Pause();
        }

        private static void Pause()
        {
            Console.WriteLine("Pause...");
            Console.ReadLine();
        }

    }
}
