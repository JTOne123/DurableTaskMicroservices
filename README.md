# DurableTaskMicroservices

Microservice Framework based on Durable Task Framework

## Introduction to DurableTaskMicroservices

TODO

### Reasons to use DurableTaskMicroservices

TODO

## Hosts

This repository contains multiple hosts, you can decide which host fits best to your needs.
You are free to implement your own host and contribute it via pull request.

### WindowsServiceHost

A simple windows service used to host the DurableTaskFramework.

#### Orchestrations loading

1. The *WindowsServiceHost* searches for all `*.config.xml` files in the working directory.
These files must contain your XML serialized `Microservice`.
1. Now *WindowsServiceHost* gets all Types which are used in the `Microservices`.
To do this, the host gets all *.dlls in the working folder and check for the existence of `IntegrationAssemblyAttribute`.
1. Then the host starts the `TaskHubWorker` and create an `OrchestrationInstance` (if none are running).


#### How to Install the Service

The installation process is quite simple. First you should compile the DtfService solution and copy the output into the deployment folder (you should create one).
Then you should compile your orchestrations project and copy the output dll/config (xml/json) to the deployment folder. For each interface (orchestration) there should be one configuration. For example Test.Orch1.config.xml or Test.Orch2.config.json.

Now you should install the service with the installservice.bat file.

`Installservice.bat Orchestration1 "WindowsServiceHost.exePath" "Description of your Windows Server"`

#### How to Uninstall the service

`uninstallservice.bat Orchestration1`
