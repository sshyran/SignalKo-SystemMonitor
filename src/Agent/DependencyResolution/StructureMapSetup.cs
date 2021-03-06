using SignalKo.SystemMonitor.Agent.Core;
using SignalKo.SystemMonitor.Agent.Core.Collectors;
using SignalKo.SystemMonitor.Agent.Core.Collectors.HttpStatusCodeCheck;
using SignalKo.SystemMonitor.Agent.Core.Collectors.SystemPerformance;
using SignalKo.SystemMonitor.Agent.Core.Configuration;
using SignalKo.SystemMonitor.Agent.Core.Coordination;
using SignalKo.SystemMonitor.Agent.Core.Queueing;
using SignalKo.SystemMonitor.Agent.Core.Sender;
using SignalKo.SystemMonitor.Common.Model;
using SignalKo.SystemMonitor.Common.Services;

using StructureMap;

namespace SignalKo.SystemMonitor.Agent.DependencyResolution
{
	public static class StructureMapSetup
	{
		public static void Setup()
		{
			ObjectFactory.Configure(
				config =>
				{
					/* common */
					config.For<IEncodingProvider>().Use<DefaultEncodingProvider>();
					config.For<IMemoryUnitConverter>().Use<MemoryUnitConverter>();
					config.For<ITimeProvider>().Use<UTCTimeProvider>();

					/* collector */
					config.For<ISystemInformationProvider>().Use<SystemInformationProvider>();
					config.For<IUrlComponentExtractor>().Use<UrlComponentExtractor>();
					config.For<IMachineNameProvider>().Use<EnvironmentMachineNameProvider>();

					/* system performance */
					config.For<ILogicalDiscInstanceNameProvider>().Use<LogicalDiscInstanceNameProvider>();
					config.For<ISystemPerformanceDataProvider>().Use<SystemPerformanceDataProvider>();
					config.For<IProcessorStatusProvider>().Use<ProcessorStatusProvider>();
					config.For<ISystemStorageStatusProvider>().Use<SystemStorageStatusProvider>();
					config.For<ISystemMemoryStatusProvider>().Use<SystemMemoryStatusProvider>();

					/* http status code */
					config.For<IHttpStatusCodeFetcher>().Use<HttpStatusCodeFetcher>();
					config.For<IHttpStatusCodeCheckResultProvider>().Use<HttpStatusCodeCheckResultProvider>();

					/* coordination */
					config.For<IAgentCoordinationServiceFactory>().Use<AgentCoordinationServiceFactory>();

					/* queuing */
					var workQueue = new SystemInformationMessageQueue();
					var errorQueue = new SystemInformationMessageQueue();
					var messageQueueProvider = new SystemInformationMessageQueueProvider(workQueue, errorQueue);
					config.For<IMessageQueueProvider<SystemInformation>>().Singleton().Use(() => messageQueueProvider);

					config.For<IMessageQueuePersistence<SystemInformation>>().Use<JSONSystemInformationMessageQueuePersistence>();
					config.For<IJSONMessageQueuePersistenceConfigurationProvider>().Use<AppConfigJSONMessageQueuePersistenceConfigurationProvider>();

					config.For<IMessageQueueFeederFactory>().Use<SystemInformationMessageQueueFeederFactory>();
					config.For<IMessageQueueWorkerFactory>().Use<SystemInformationMessageQueueWorkerFactory>();

					/* sender configuration */
					config.For<IAgentControlDefinitionAccessor>().Use<AgentControlDefinitionAccessor>();
					config.For<IAgentControlDefinitionProvider>().Use<AgentControlDefinitionProvider>();
					config.For<IAgentControlDefinitionServiceUrlProvider>().Use<AppConfigAgentControlDefinitionServiceUrlProvider>();
					config.For<IRESTBasedSystemInformationSenderConfigurationProvider>().Use<RESTBasedSystemInformationSenderConfigurationProvider>();

					/* sender */
					config.For<IRESTClientFactory>().Use<RESTClientFactory>();
					config.For<IRESTRequestFactory>().Use<JSONRequestFactory>();
					config.For<ISystemInformationSender>().Use<RESTBasedSystemInformationSender>();

					config.For<ISystemInformationDispatchingService>().Use<SystemInformationDispatchingService>();
				});
		}
	}
}
