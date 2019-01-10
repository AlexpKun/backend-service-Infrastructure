using Amazon.Lambda.Core;
using Ragnaros.Messaging.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Ragnaros.Hosting.AWS
{
    /// <summary>
    /// MessageTriggerFunction is the base class that is implemented in a ASP.NET Core message API. The derived class implements
    /// the Init method similar to Main function in the ASP.NET Core. The function handler for the Lambda function will point
    /// to this base class FunctionHandlerAsync method.
    /// </summary>
    public abstract class MessageTriggerFunction<T> where T : IMessage
    {
        private AspNetCoreStartupMode _startupMode;
        private IHost _host;
        private ILogger _logger;

        /// <summary>
        /// The modes for when the ASP.NET Core framework will be initialized.
        /// </summary>
        public enum AspNetCoreStartupMode
        {
            /// <summary>
            /// Initialize during the construction of MessageTriggerFunction
            /// </summary>
            Constructor,

            /// <summary>
            /// Initialize during the first incoming message
            /// </summary>
            FirstMessage
        }

        /// <summary>
        /// Default Constructor. The ASP.NET Core Framework will be initialized as part of the construction.
        /// </summary>
        protected MessageTriggerFunction()
            : this(AspNetCoreStartupMode.Constructor)
        {

        }

        /// <param name="startupMode">Configure when the ASP.NET Core framework will be initialized</param>
        protected MessageTriggerFunction(AspNetCoreStartupMode startupMode)
        {
            _startupMode = startupMode;

            if (_startupMode == AspNetCoreStartupMode.Constructor)
            {
                Start();
            }
        }

        private bool IsStarted
        {
            get
            {
                return _host != null;
            }
        }

        /// <summary>
        /// Should be called in the derived constructor 
        /// </summary>
        protected void Start()
        {
            IHostBuilder builder = CreateHostBuilder();
            Init(builder);

            _host = builder.Build();
            _host.Start();

            _logger = ActivatorUtilities.CreateInstance<Logger<MessageTriggerFunction<T>>>(_host.Services);
        }

        /// <summary>
        /// Method to initialize the host builder before starting the host. In a typical service this is similar to the main function. 
        /// Setting the Startup class is required in this method.
        /// </summary>
        /// <example>
        /// <code>
        /// protected override void Init(IHostBuilder builder)
        /// {
        ///     builder
        ///         .UseStartup&lt;Startup&gt;();
        /// }
        /// </code>
        /// </example>
        /// <param name="builder"></param>
        protected abstract void Init(IHostBuilder builder);
        
        protected virtual IHostBuilder CreateHostBuilder()
        {
            var builder = new HostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    if (env.IsDevelopment())
                    {
                        var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                        if (appAssembly != null)
                        {
                            config.AddUserSecrets(appAssembly, optional: true);
                        }
                    }

                    config.AddEnvironmentVariables();
                }).ConfigureLogging((hostingContext, logging) =>
                {
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LAMBDA_TASK_ROOT")))
                    {
                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                        logging.AddDebug();
                    }
                    else
                    {
                        logging.AddLambdaLogger(hostingContext.Configuration, "Logging");
                    }
                });

            return builder;
        }

        /// <summary>
        /// This method is what the Lambda function handler points to.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="lambdaContext"></param>
        /// <returns></returns>
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task FunctionHandlerAsync(T message, ILambdaContext lambdaContext)
        {
            if (!IsStarted)
            {
                Start();
            }

            using (IServiceScope scope = _host.Services.CreateScope())
            {
                var messageConsumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer<T>>();

                await messageConsumer.ConsumeAsync(message);
            }
        }
    }
}