using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleExample
{
    public class ProxyDecorator<T> : DispatchProxy
    {
        // 关键词 RealProxy
        private T decorated;
        private event Action<MethodInfo, object[]> _afterAction; // 动作之后执行
        private event Action<MethodInfo, object[]> _beforeAction; // 动作之前执行
        //其他自定义属性，事件和方法

        public ProxyDecorator()
        {
        }

        /// <summary>
        /// 创建代理实例
        /// </summary>
        /// <param name="decorated">代理的接口类型</param>
        /// <returns></returns>
        public T Create(T decorated)
        {
            object proxy = Create<T, ProxyDecorator<T>>(); // 调用 DispatchProxy 的 Create 创建一个新的T
            ((ProxyDecorator<T>)proxy).decorated = decorated; // 这里必须这样赋值，会自动为 proxy 添加一个新的属性
            return (T)proxy;
        }

        /// <summary>
        /// 创建代理实例
        /// </summary>
        /// <param name="decorated">代理的接口类型</param>
        /// <param name="beforeAction">方法执行前执行的事件</param>
        /// <param name="afterAction">方法执行后执行的事件</param>
        /// <returns></returns>
        public T Create(T decorated, Action<MethodInfo, object[]> beforeAction, Action<MethodInfo, object[]> afterAction)
        {
            object proxy = Create<T, ProxyDecorator<T>>(); // 调用DispatchProxy 的Create  创建一个新的T
            ProxyDecorator<T> proxyDecorator = (ProxyDecorator<T>)proxy;
            proxyDecorator.decorated = decorated;
            proxyDecorator._afterAction = afterAction;
            proxyDecorator._beforeAction = beforeAction;

            // proxyDecorator._loggingScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            return (T)proxy;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod == null) throw new Exception($"方法[{targetMethod.Name}]无效");
            try
            {

                // _beforeAction 事件
                this._beforeAction?.Invoke(targetMethod, args);

                object result = targetMethod.Invoke(decorated, args);
                System.Diagnostics.Debug.WriteLine(result); // 打印输出面板
                if(result is Task resultTask && resultTask != null)
                {
                    resultTask.ContinueWith(task => // ContinueWith 创建一个延续，该延续接收调用方提供的状态信息并执行 目标系统 tasks。 
                    {
                        if(task.Exception != null)
                        {
                            LogException(task.Exception.InnerException ?? task.Exception, targetMethod);
                        }
                        else
                        {
                            object taskResult = null;

                            var type = task.GetType();
                            var typeInfo = type.GetTypeInfo();
                            if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                            {
                                var property = typeInfo.GetProperties().FirstOrDefault(p => p.Name == "Result");
                                taskResult = property?.GetValue(task);
                            }

                            // _afterAction 事件
                            this._afterAction?.Invoke(targetMethod, args);
                        }
                    });
                }
                else
                {
                    try
                    {
                        // _afterAction 事件
                        this._afterAction?.Invoke(targetMethod, args);
                    }
                    catch (Exception ex)
                    {
                        // Do not stop method execution if exception 
                        LogException(ex);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    LogException(ex.InnerException ?? ex, targetMethod);
                    throw ex.InnerException ?? ex;
                }
                else
                {
                    throw;
                }
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// aop异常的处理
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="methodInfo"></param>
        private void LogException(Exception exception, MethodInfo methodInfo = null)
        {
            try
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"Class {decorated.GetType().FullName}");
                errorMessage.AppendLine($"Method {methodInfo?.Name} threw exception");
                errorMessage.AppendLine(exception.Message);
                //_logError?.Invoke(errorMessage.ToString());  记录到文件系统
                Console.WriteLine(errorMessage.ToString());
            }
            catch (Exception)
            {
                // ignored  
                //Method should return original exception  
            }
        }
    }
}
