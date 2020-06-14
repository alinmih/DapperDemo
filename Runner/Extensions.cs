using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace Runner
{
    public static class Extensions
    {
        public static void Output(this object item)
        {
            var serilizer = new SerializerBuilder().Build();
            var yaml = serilizer.Serialize(item);
            Console.WriteLine(yaml);
        }
    }
}
