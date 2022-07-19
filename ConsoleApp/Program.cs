using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    // Console app is used to actually test code, you can't really do much with the actual code base but
    // Below you can see I'm using reflection to get a private variable and modify it
    class Program
    {
        static Instance instance = new Instance();

        static void Main(string[] args)
        {
            instance.Main();
        }
    }

    public class Instance
    {
        Dictionary<int, string> commandsMap = new Dictionary<int, string>();
        FieldInfo commandsMapFieldInfo = null;

        public void Main()
        {
            var inputManager = new InputManager();
            var fieldInfos = inputManager?.GetType()?.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var item in fieldInfos)
            {
                if (item.Name == "commandsMap")
                {
                    commandsMapFieldInfo = item;

                    // TODO: Find out why this isn't working?
                    var commandMapsValue = commandsMapFieldInfo.GetValue(inputManager);
                    commandsMap = (Dictionary<int, string>)commandMapsValue;
                }
                commandsMap.Add(3, "Three");
                //logs.Add($"{item.Name}: {item.FieldType}");
            }

            var commandsMapNew = (Dictionary<int, string>)commandsMapFieldInfo.GetValue(inputManager);
            foreach (var item in commandsMapNew)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }

            Console.ReadLine();
        }
    }

    public class InputManager
    {
        Dictionary<int, string> commandsMap = new Dictionary<int, string>();

        public InputManager()
        {
            commandsMap.Add(0, "Zero");
            commandsMap.Add(1, "One");
            commandsMap.Add(2, "Two");
        }
    }


}
