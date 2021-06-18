using System;
using System.Reflection;

namespace ConsoleExample
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var dispatch = new ProxyDecorator<ICaculator>().Create(new Caculator(), 
                (m,args) => 
                {
                    Console.WriteLine($"{m.Name}调用前, 参数: {string.Join(", ", args)}");
                }, 
                (m, args) => 
                {
                    Console.WriteLine($"{m.Name}调用后, 参数: {string.Join(", ", args)}");
                });

            int zAsync = await dispatch.AddAsync(1, 2);
            int z = dispatch.Add(1, 2);

            Console.WriteLine($"Hello World! {z}, {zAsync}");
            Console.ReadKey();
        }
    }
}

/* netcore 之动态代理（微服务专题） https://www.cnblogs.com/netqq/p/11452374.html
   实现. net 下的动态代理 https://www.cnblogs.com/xiaotie/archive/2009/02/01/1381825.html */