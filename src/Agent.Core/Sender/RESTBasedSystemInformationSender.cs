using System;

using SignalKo.SystemMonitor.Agent.Core.Exceptions;
using SignalKo.SystemMonitor.Agent.Core.Sender.Configuration;
using SignalKo.SystemMonitor.Common.Model;

namespace SignalKo.SystemMonitor.Agent.Core.Sender
{
    public class RESTBasedSystemInformationSender : ISystemInformationSender
    {
        private readonly IRESTBasedSystemInformationSenderConfigurationProvider systemInformationSenderConfigurationProvider;

        private readonly IRESTClientFactory restClientFactory;

        private readonly IRESTRequestFactory requestFactory;

        public RESTBasedSystemInformationSender(IRESTBasedSystemInformationSenderConfigurationProvider systemInformationSenderConfigurationProvider, IRESTClientFactory restClientFactory, IRESTRequestFactory requestFactory)
        {
            if (systemInformationSenderConfigurationProvider == null)
            {
                throw new ArgumentNullException("systemInformationSenderConfigurationProvider");
            }

            if (restClientFactory == null)
            {
                throw new ArgumentNullException("restClientFactory");
            }

            if (requestFactory == null)
            {
                throw new ArgumentNullException("requestFactory");
            }

            this.systemInformationSenderConfigurationProvider = systemInformationSenderConfigurationProvider;
            this.restClientFactory = restClientFactory;
            this.requestFactory = requestFactory;
        }

        public void Send(SystemInformation systemInformation)
        {
            if (systemInformation == null)
            {
                throw new ArgumentNullException("systemInformation");
            }

            // get service configuration
            IRESTServiceConfiguration serviceConfiguration = this.systemInformationSenderConfigurationProvider.GetConfiguration();
            if (serviceConfiguration == null)
            {
                throw new SystemInformationSenderSetupException("Service configuration is null.");
            }

            if (!serviceConfiguration.IsValid())
            {
                throw new SystemInformationSenderSetupException("Service configuration \"{0}\" is invalid.", serviceConfiguration);
            }

            // create REST client
            var restClient = this.restClientFactory.GetRESTClient(serviceConfiguration.BaseUrl);
            if (restClient == null)
            {
                throw new SystemInformationSenderSetupException(
                    "Could not create a REST client using the supplied base URL \"{0}\".", serviceConfiguration.BaseUrl);
            }

            // create request
            var request = this.requestFactory.CreatePutRequest(serviceConfiguration.ResourcePath);
            if (request == null)
            {
                throw new FatalSystemInformationSenderException(
                    "Could not create a request object for the base Url \"{0}\" and the resource path \"{1}\".",
                    serviceConfiguration.BaseUrl,
                    serviceConfiguration.ResourcePath);
            }

            request.AddBody(systemInformation);

            // Send request
            var response = restClient.Execute<SystemInformation>(request);

            // Evaluate response
            if (response.ErrorException != null)
            {
                throw new SendSystemInformationFailedException(
                    string.Format("Sending object \"{0}\" via a REST call to \"{1}\" failed. Please try again later.", systemInformation, serviceConfiguration),
                    response.ErrorException);
            }
        }
    }
}