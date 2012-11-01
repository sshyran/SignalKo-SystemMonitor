﻿if (typeof (jQuery) === 'undefined') {
	throw new Error("jQuery is required to run this component");
}

if (typeof (ko) === 'undefined') {
	throw new Error("Knockout is required to run this component");
}

if (typeof (SystemMonitor) === 'undefined') {
	throw new Error("SystemMonitor is required to run this component");
}

$.extend(SystemMonitor, {

	"AgentConfiguration": (function (moduleConfiguration) {

		var collectorTypeSystemInformation = "System Information";
		var httpStatusCodeCheck = "HTTP Status Code Check";
		var webPageContentCheck = "Web Page Content Check";
		var responseTimeCheck = "Response Time Check";

		var collectorTypes = [collectorTypeSystemInformation, httpStatusCodeCheck, webPageContentCheck, responseTimeCheck];

		function systemInformationCollectorDefinition() {
			var self = this;
		}

		function httpStatusCodeCheckDefinition() {
			var self = this;

			self.CheckUrl = ko.observable();
			self.Hostheader = ko.observable();
			self.ExpectedStatusCode = ko.observable();
		}

		function webPageContentCheckDefinition() {
			var self = this;

			self.CheckUrl = ko.observable();
			self.Hostheader = ko.observable();
			self.CheckPattern = ko.observable();
		}

		function responseTimeCheckDefinition() {
			var self = this;

			self.CheckUrl = ko.observable();
			self.Hostheader = ko.observable();
			self.MaxResponseTimeInMilliseconds = ko.observable(1000);
		}

		function agentInstanceCollectorDefinitionViewModel(collectorType) {
			var self = this;

			self.CollectorType = collectorType;
			self.CheckIntervalInSeconds = ko.observable(1);

			switch (collectorType) {
				case collectorTypeSystemInformation:
					{
						_.extend(self, new systemInformationCollectorDefinition());
						break;
					}
				case httpStatusCodeCheck:
					{
						_.extend(self, new httpStatusCodeCheckDefinition());
						break;
					}
				case webPageContentCheck:
					{
						_.extend(self, new webPageContentCheckDefinition());
						break;
					}
				case responseTimeCheck:
					{
						_.extend(self, new responseTimeCheckDefinition());
						break;
					}
				default:
					throw new Error("The collector type '{0}' is unknown.".format(collectorType));
			}
		}

		function agentInstanceConfigurationViewModel(instanceName) {
			var self = this;

			self.MachineName = instanceName;
			self.AgentIsEnabled = ko.observable(true);
			self.AgentIsEnabled.ForEditing = ko.computed({
				read: function () {
					return self.AgentIsEnabled().toString();
				},
				write: function (newValue) {
					return self.AgentIsEnabled(newValue === "true");
				},
				owner: self
			}, this);

			self.CollectorDefinitions = ko.observableArray();
			self.AvailableCollectorTypes = ko.computed(function () {
				var collectorDefinitions = self.CollectorDefinitions();
				var availableCollectorTypes = collectorTypes;
				for (var i = 0; i < collectorDefinitions.length; i++) {
					var collectorDefinitionViewModel = collectorDefinitions[i];
					availableCollectorTypes = _.without(availableCollectorTypes, collectorDefinitionViewModel.CollectorType);
				}

				return availableCollectorTypes;
			}, this);

			self.AddCollectorDefinition = function (collectorType) {
				console.debug("Adding a collector definition of type '{0}'".format(collectorType));

				var collectorDefinition = new agentInstanceCollectorDefinitionViewModel(collectorType);
				self.CollectorDefinitions.push(collectorDefinition);
				console.log(self.CollectorDefinitions());
			};

			/// <summary>Remove the supplied collector definition view model.</summary>
			/// <param name="collectorDefinitionViewModel" type="agentInstanceCollectorDefinitionViewModel">The collector definition to remove.</param>
			self.RemoveCollectorDefinition = function (collectorDefinitionViewModel) {
				self.CollectorDefinitions.remove(collectorDefinitionViewModel);
			};
		}

		function agentConfigurationViewModel(viewModelConfiguration) {
			var self = this;

			self.Hostaddress = ko.observable();
			self.Hostname = ko.observable();
			self.SystemInformationSenderPath = ko.observable();
			self.AgentsAreEnabled = ko.observable(true);
			self.CheckIntervalInSeconds = ko.observable();
			self.AgentInstanceConfigurations = ko.observableArray();

			self.AgentsAreEnabled.ForEditing = ko.computed({
				read: function () {
					return self.AgentsAreEnabled().toString();
				},
				write: function (newValue) {
					return self.AgentsAreEnabled(newValue === "true");
				},
				owner: self
			}, this);

			var showSuccessMessage = function (message) {
				if (self.SuccessCallback && typeof (self.SuccessCallback) === 'function') {
					self.SuccessCallback(message);
				}
			};

			var showErrorMessage = function (message) {
				if (self.ErrorCallback && typeof (self.ErrorCallback) === 'function') {
					self.SuccessCallback(message);
				}
			};

			self.LoadConfiguration = function () {
				$.ajax({
					url: self.GetAgentConfigurationApiUrl(),
					type: "GET",
					success: function (agentConfiguration) {
						if (!agentConfiguration) {
							showErrorMessage("Cannot load empty agent configuration.");
							return;
						}

						self.Hostaddress(agentConfiguration.Hostaddress);
						self.Hostname(agentConfiguration.Hostname);
						self.SystemInformationSenderPath(agentConfiguration.SystemInformationSenderPath);
						self.AgentsAreEnabled(agentConfiguration.AgentsAreEnabled);
						self.CheckIntervalInSeconds(agentConfiguration.CheckIntervalInSeconds);

						showSuccessMessage("Agent configuration loaded.");
					},
					error: function () {
						showErrorMessage("Cannot retrieve agent configuration from server.");
					}
				});
			};

			self.SaveConfiguration = function () {
				$.ajax({
					url: self.GetAgentConfigurationApiUrl(),
					type: "POST",
					contentType: "application/json",
					data: JSON.stringify({
						"Hostaddress": self.Hostaddress(),
						"Hostname": self.Hostname(),
						"SystemInformationSenderPath": self.SystemInformationSenderPath(),
						"AgentsAreEnabled": self.AgentsAreEnabled(),
						"CheckIntervalInSeconds": self.CheckIntervalInSeconds()
					}),
					success: function () {
						showSuccessMessage("Agent configuration has bee saved successfully.");
					},
					error: function () {
						showErrorMessage("Cannot save agent configuration to server.");
					}
				});
			};

			var applyConfiguration = function (configuration) {
				if (!configuration) {
					return;
				}

				/**
					Get the AgentConfiguration API URL.
					@name GetAgentConfigurationApiUrl
				*/
				self.GetAgentConfigurationApiUrl = function () {
					return configuration.AgentConfigurationApiUrl;
				};

				/**
					Initialize the agent-instance configuration view models.
				*/
				var agentInstanceViewModels = [];
				if (configuration.KnownAgents && configuration.KnownAgents.length > 0) {
					for (var i = 0; i < configuration.KnownAgents.length; i++) {
						agentInstanceViewModels.push(new agentInstanceConfigurationViewModel(configuration.KnownAgents[i]));
					}
				}
				self.AgentInstanceConfigurations = ko.observableArray(agentInstanceViewModels);

				/**
					Initialize the success- and error callback functions.
				*/
				self.SuccessCallback = configuration.SuccessCallback;
				self.ErrorCallback = configuration.ErrorCallback;
			};

			applyConfiguration(viewModelConfiguration);

			self.LoadConfiguration();
		}

		var viewModel = new agentConfigurationViewModel(moduleConfiguration);
		ko.applyBindings(viewModel);

		return viewModel;

	})(agentConfigurationOptions)
});