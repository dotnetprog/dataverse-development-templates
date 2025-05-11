using Blueprint.Plugins.Platform;
using Blueprint.Plugins.Services.EnvironmentVariable;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;

namespace Blueprint.Plugins
{
    /// <summary>
    /// Base class for all plug-in classes.
    /// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
    /// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        private readonly ICollection<Step> registeredEvents;

        /// <summary>
        /// Gets or sets the name of the plugin class.
        /// </summary>
        /// <value>The name of the child class.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "PluginBase")]
        protected string PluginClassName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginBase"/> class.
        /// </summary>
        /// <param name="pluginClassName">The <see cref=" cred="Type"/> of the derived class.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "PluginBase")]
        internal PluginBase(string EntityName)
        {
            PluginClassName = this.GetType().Name;
            this.RegisteredEvents = new Collection<Step>();
            this.RegisteredEvents.Register(EntityName,
            #region Update Events
                new Step(Stage.PostOperation, Message.Update, ExecutionMode.Async, ExecutePostAsyncUpdate),
                new Step(Stage.PostOperation, Message.Update, ExecutionMode.Sync, ExecutePostUpdate),
                new Step(Stage.PreOperation, Message.Update, ExecutionMode.Sync, ExecutePreUpdate),
                new Step(Stage.PreValidation, Message.Update, ExecutionMode.Sync, ExecutePreValidateUpdate),
            #endregion
            #region Create Events
                new Step(Stage.PostOperation, Message.Create, ExecutionMode.Async, ExecutePostAsyncCreate),
                new Step(Stage.PreOperation, Message.Create, ExecutionMode.Sync, ExecutePreCreate),
                new Step(Stage.PostOperation, Message.Create, ExecutionMode.Sync, ExecutePostCreate),
                new Step(Stage.PreValidation, Message.Create, ExecutionMode.Sync, ExecutePreValidateCreate),
            #endregion
            #region Delete Events
                new Step(Stage.PostOperation, Message.Delete, ExecutionMode.Async, ExecutePostAsyncDelete),
                new Step(Stage.PreOperation, Message.Delete, ExecutionMode.Sync, ExecutePreDelete),
                new Step(Stage.PostOperation, Message.Delete, ExecutionMode.Sync, ExecutePostDelete),
                new Step(Stage.PreValidation, Message.Delete, ExecutionMode.Sync, ExecutePreValidateDelete)
            #endregion
                );
        }
        protected Collection<Step> RegisteredEvents { get; private set; }
        /// <summary>
        /// Main entry point for he business logic that the plug-in is to execute.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <remarks>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Execute")]
        public void Execute(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new InvalidPluginExecutionException("serviceProvider");
            }

            // Construct the local plug-in context.
            var localPluginContext = new LocalPluginContext(serviceProvider);

            localPluginContext.Trace($"Entered {PluginClassName}.Execute() " +
                 $"Correlation Id: {localPluginContext.PluginExecutionContext.CorrelationId}, " +
                 $"Initiating User: {localPluginContext.PluginExecutionContext.InitiatingUserId}");

            try
            {
                // Invoke the custom implementation 
                var currentStep = this.RegisteredEvents.Where(s => ValidateExecution(s, localPluginContext.PluginExecutionContext)).FirstOrDefault();
                if (currentStep == null)
                {
                    return;
                }
                localPluginContext.Trace($"{this.PluginClassName} is firing for Entity: {localPluginContext.PluginExecutionContext.PrimaryEntityName},Message: {localPluginContext.PluginExecutionContext.MessageName}");
                currentStep.Action.Invoke(localPluginContext);
                // now exit - if the derived plug-in has incorrectly registered overlapping event registrations,
                // guard against multiple executions.
                return;
            }
            catch (FaultException<OrganizationServiceFault> orgServiceFault)
            {
                localPluginContext.Trace($"Exception: {orgServiceFault.ToString()}");

                // Handle the exception.
                throw new InvalidPluginExecutionException($"OrganizationServiceFault: {orgServiceFault.Message}", orgServiceFault);
            }
            finally
            {
                localPluginContext.Trace($"Exiting {PluginClassName}.Execute()");
            }
        }
        private bool ValidateExecution(Step step, IPluginExecutionContext pluginContext)
        {
            return (int)step.Stage == pluginContext.Stage &&
                step.Message.ToString() == pluginContext.MessageName &&
                (int)step.mode == pluginContext.Mode &&
                (step.EntityName == pluginContext.PrimaryEntityName ||
                string.IsNullOrWhiteSpace(step.EntityName));
        }
        /// <summary>
        /// Placeholder for a custom plug-in implementation. 
        /// </summary>
        /// <param name="localPluginContext">Context for the current plug-in.</param>
        protected virtual void ExecutePostAsyncUpdate(ILocalPluginContext localPluginContext)
        {
            // do nothing
        }
        protected virtual void ExecutePostUpdate(ILocalPluginContext localPluginContext)
        {
            // do nothing
        }
        protected virtual void ExecutePreUpdate(ILocalPluginContext localPluginContext)
        {
            // do nothing
        }
        protected virtual void ExecutePreValidateUpdate(ILocalPluginContext localPluginContext)
        {
            // do nothing
        }
        protected virtual void ExecutePreCreate(ILocalPluginContext localPluginContext)
        {
            // do nothing
        }
        protected virtual void ExecutePreValidateCreate(ILocalPluginContext localPluginContext)
        {
            // do nothing
        }
        /// <summary>
        /// Placeholder for a custom plug-in implementation. 
        /// </summary>
        /// <param name="localPluginContext">Context for the current plug-in.</param>
        protected virtual void ExecutePostCreate(ILocalPluginContext localPluginContext)
        {

        }
        protected virtual void ExecutePostAsyncCreate(ILocalPluginContext localPluginContext)
        {
            // do nothing
        }
        protected virtual void ExecutePostAsyncDelete(ILocalPluginContext localPluginContext)
        {
            // do nothing
        }
        protected virtual void ExecutePostDelete(ILocalPluginContext localPluginContext)
        {
            // do nothing
        }
        protected virtual void ExecutePreDelete(ILocalPluginContext localPluginContext)
        {
            // do nothing
        }
        protected virtual void ExecutePreValidateDelete(ILocalPluginContext localPluginContext)
        {
            // do nothing
        }
        protected void ApplyBusinessRules(ILocalPluginContext localPluginContext, params IBusinessRule[] rules)
        {
            foreach (var rule in rules)
            {
                localPluginContext.Trace($"Execution business rule started: {rule.GetType().Name}");
                rule.Apply(localPluginContext);
                localPluginContext.Trace($"Execution business rule completed: {rule.GetType().Name}");
            }
        }
    }

    //This interface provides an abstraction on top of IServiceProvider for commonly used PowerApps CDS Plugin development constructs
    public interface ILocalPluginContext
    {
        // The PowerApps CDS organization service for current user account
        IOrganizationService CurrentUserService { get; }

        // The PowerApps CDS organization service for system user account
        IOrganizationService SystemUserService { get; }

        // IPluginExecutionContext contains information that describes the run-time environment in which the plugin executes, information related to the execution pipeline, and entity business information
        IPluginExecutionContext PluginExecutionContext { get; }

        // Synchronous registered plugins can post the execution context to the Microsoft Azure Service Bus.
        // It is through this notification service that synchronous plug-ins can send brokered messages to the Microsoft Azure Service Bus
        IServiceEndpointNotificationService NotificationService { get; }

        // Provides logging run time trace information for plug-ins. 
        ITracingService TracingService { get; }

        ICustomEnvironmentVariableService EnvironmentVariableService { get; }

        // Writes a trace message to the CDS trace log
        void Trace(string message);

        Entity GetTarget();
        Entity GetPreImage();
        Entity GetPostImage();
    }

    /// <summary>
    /// Plug-in context object. 
    /// </summary>
    public class LocalPluginContext : ILocalPluginContext
    {
        const string PREIMAGE_KEY = "PreImage";
        const string POSTIMAGE_KEY = "PostImage";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "LocalPluginContext")]
        internal IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// The PowerApps CDS organization service for current user account.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "LocalPluginContext")]
        public IOrganizationService CurrentUserService { get; private set; }

        /// <summary>
        /// The PowerApps CDS organization service for system user account.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "LocalPluginContext")]
        public IOrganizationService SystemUserService { get; private set; }

        /// <summary>
        /// IPluginExecutionContext contains information that describes the run-time environment in which the plug-in executes, information related to the execution pipeline, and entity business information.
        /// </summary>
        public IPluginExecutionContext PluginExecutionContext { get; private set; }

        /// <summary>
        /// Synchronous registered plug-ins can post the execution context to the Microsoft Azure Service Bus. <br/> 
        /// It is through this notification service that synchronous plug-ins can send brokered messages to the Microsoft Azure Service Bus.
        /// </summary>
        public IServiceEndpointNotificationService NotificationService { get; private set; }

        /// <summary>
        /// Provides logging run-time trace information for plug-ins. 
        /// </summary>
        public ITracingService TracingService { get; private set; }

        private Lazy<ICustomEnvironmentVariableService> EnvironmentServiceLazy { get; set; }
        public ICustomEnvironmentVariableService EnvironmentVariableService => EnvironmentServiceLazy.Value;
        /// <summary>
        /// Helper object that stores the services available in this plug-in.
        /// </summary>
        /// <param name="serviceProvider"></param>
        public LocalPluginContext(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new InvalidPluginExecutionException("serviceProvider");
            }

            // Obtain the execution context service from the service provider.
            PluginExecutionContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the tracing service from the service provider.
            TracingService = new LocalTracingService(serviceProvider);

            // Get the notification service from the service provider.
            NotificationService = (IServiceEndpointNotificationService)serviceProvider.GetService(typeof(IServiceEndpointNotificationService));

            // Obtain the organization factory service from the service provider.
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            // Use the factory to generate the organization service.
            CurrentUserService = factory.CreateOrganizationService(PluginExecutionContext.UserId);

            // Use the factory to generate the organization service.
            SystemUserService = factory.CreateOrganizationService(null);
            this.EnvironmentServiceLazy = new Lazy<ICustomEnvironmentVariableService>(() =>
                                          new CachedEnvironmentVariableService(new CrmEnvironmentVariableService(SystemUserService)));

        }

        /// <summary>
        /// Writes a trace message to the CRM trace log.
        /// </summary>
        /// <param name="message">Message name to trace.</param>
        public void Trace(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || TracingService == null)
            {
                return;
            }

            if (PluginExecutionContext == null)
            {
                TracingService.Trace(message);
            }
            else
            {
                TracingService.Trace($"{message}, Correlation Id: {PluginExecutionContext.CorrelationId}, Initiating User: {PluginExecutionContext.InitiatingUserId}");
            }
        }

        public Entity GetTarget()
        {
            if (!this.PluginExecutionContext.InputParameters.Contains("Target"))
                return null;


            var target = this.PluginExecutionContext.InputParameters["Target"];
            if (target is Entity)
            {
                return (Entity)target;
            }
            if (target is EntityReference)
            {
                var reference = (EntityReference)target;
                return new Entity { Id = reference.Id, LogicalName = reference.LogicalName };
            }

            throw new NotImplementedException($"GetTarget is not implemented for type {target.GetType().Name}");
        }

        public Entity GetPreImage()
        {
            if (!this.PluginExecutionContext.PreEntityImages.Contains(PREIMAGE_KEY))
            {
                return null;
            }
            return this.PluginExecutionContext.PreEntityImages[PREIMAGE_KEY];
        }

        public Entity GetPostImage()
        {
            if (!this.PluginExecutionContext.PostEntityImages.Contains(POSTIMAGE_KEY))
            {
                return null;
            }
            return this.PluginExecutionContext.PostEntityImages[POSTIMAGE_KEY];
        }
    }

    // Specialized ITracingService implementation that prefixes all traced messages with a time delta for Plugin performance diagnostics
    public class LocalTracingService : ITracingService
    {
        private readonly ITracingService _tracingService;

        private DateTime _previousTraceTime;

        public LocalTracingService(IServiceProvider serviceProvider)
        {
            DateTime utcNow = DateTime.UtcNow;

            var context = (IExecutionContext)serviceProvider.GetService(typeof(IExecutionContext));

            DateTime initialTimestamp = context.OperationCreatedOn;

            if (initialTimestamp > utcNow)
            {
                initialTimestamp = utcNow;
            }

            _tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            _previousTraceTime = initialTimestamp;
        }

        public void Trace(string message, params object[] args)
        {
            var utcNow = DateTime.UtcNow;

            // The duration since the last trace.
            var deltaMilliseconds = utcNow.Subtract(_previousTraceTime).TotalMilliseconds;

            _tracingService.Trace($"[+{deltaMilliseconds:N0}ms)] - {message}");

            _previousTraceTime = utcNow;
        }
    }
}
