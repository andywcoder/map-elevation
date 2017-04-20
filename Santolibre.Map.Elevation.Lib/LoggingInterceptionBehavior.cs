using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity.InterceptionExtension;
using NLog;

namespace Santolibre.Map.Elevation.Lib
{
    public class LoggingInterceptionBehavior : IInterceptionBehavior
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            var parameters = new List<string>();
            var parameterNames = input.MethodBase.GetParameters().Select(x => x.Name).ToList();
            var parameterTypes = input.MethodBase.GetParameters().Select(x => x.ParameterType).ToList();
            for (var i = 0; i < parameterNames.Count; i++)
            {
                if (input.Arguments[i] != null)
                {
                    string value = null;
                    if (input.Arguments[i] is List<string>)
                    {
                        value = string.Join(",", (List<string>)input.Arguments[i]);
                    }
                    else
                    {
                        value = input.Arguments[i].ToString();
                    }

                    if (value.Length > 2048)
                    {
                        value = value.Substring(0, 2048) + "...";
                    }

                    parameters.Add($"{parameterNames[i]}={value}");
                }
                else
                {
                    parameters.Add($"{parameterNames[i]}=null");
                }
            }
            Logger.Log(LogLevel.Trace, $"Method call {input.Target.GetType().Name}.{input.MethodBase.Name}{(parameters.Any() ? ", " + string.Join(", ", parameters) : "")}");

            var result = getNext()(input, getNext);

            if (result.Exception != null)
            {
                Logger.Log(LogLevel.Trace, $"Method error {input.Target.GetType().Name}.{input.MethodBase.Name}, Exception={GetExceptionInfo(result.Exception)}");
            }

            return result;
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public bool WillExecute
        {
            get { return true; }
        }

        private string GetExceptionInfo(Exception e)
        {
            string errorMessage = e.Message + " " + e.StackTrace;
            if (e.InnerException != null)
            {
                errorMessage += e.InnerException.Message + " " + e.InnerException.StackTrace;
                if (e.InnerException.InnerException != null)
                {
                    errorMessage += e.InnerException.InnerException.Message + " " + e.InnerException.InnerException.StackTrace;
                }
            }
            return errorMessage;
        }
    }
}
