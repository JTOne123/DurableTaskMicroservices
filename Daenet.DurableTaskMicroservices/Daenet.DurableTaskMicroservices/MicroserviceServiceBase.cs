﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Daenet.DurableTaskMicroservices.Common.Entities;
using DurableTask;
using DurableTask.Core;

namespace Daenet.DurableTask.Microservices
{
    public class MicroserviceBase
    {
        protected TaskHubClient m_HubClient;

        public const string cActivityIdCtxName = "ActivityId";

        /// <summary>
        /// Starts the new instance of the microservice by passing input arguments.
        /// This method will start the new instance of orchestration
        /// </summary>
        /// <param name="orchestrationQualifiedName">The full qualified name of orchestration to be started.</param>
        /// <param name="inputArgs">Input arguments.</param>
        /// <returns></returns>
        //public Task<MicroserviceInstance> StartServiceAsync(string orchestrationQualifiedName, object inputArgs, Dictionary<string, object> context = null)
        //{
        //    //return StartServiceAsync(Type.GetType(orchestrationQualifiedName), inputArgs, context);
        //}


        /// <summary>
        /// Starts the new instance of the microservice by passing input arguments.
        /// This method will start the new instance of orchestration
        /// </summary>
        /// <param name="orchestration">The type of orchestration to be started.</param>
        /// <param name="inputArgs">Input arguments.</param>
        /// <returns></returns>
        public Task<MicroserviceInstance> StartServiceAsync(Type orchestration, object inputArgs, Dictionary<string, object> context = null)
        {
            return createServiceInstanceAsync(orchestration, inputArgs, context);
        }




        /// <summary>
        /// Creates the instance of service from service name.
        /// </summary>
        /// <param name="orchestrationQualifiedName"></param>
        /// <param name="inputArgs"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //protected Task<MicroserviceInstance> CreateServiceInstanceAsync(string orchestrationQualifiedName, object inputArgs, Dictionary<string, object> context)
        //{
        //    return createServiceInstanceAsync(Type.GetType(orchestrationQualifiedName), inputArgs, context);
        //}

        /// <summary>
        /// Starts the new instance of the microservice by passing input arguments.
        /// This method will start the new instance of orchestration
        /// </summary>
        /// <param name="orchestrationQualifiedName">The full qualified name of orchestration to be started.</param>
        /// <param name="inputArgs">Input arguments.</param>
        /// <returns></returns>
        public async Task<MicroserviceInstance> StartServiceAsync(string orchestrationQualifiedName, object inputArgs, Dictionary<string, object> context = null, string version = "")
        {
            try
            {        
                ensureActIdInContext(context, inputArgs);

                var ms = new MicroserviceInstance()
                {
                    OrchestrationInstance = await m_HubClient.CreateOrchestrationInstanceAsync(orchestrationQualifiedName, version, inputArgs),
                };
                return ms;
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == "StorageException" && ex.Message.Contains(": (404) Not Found"))
                {
                    throw new Exception("The microservce host should be started at least once, before starting orchestration. This error may also indicate, that incorrect hub name is used.", ex);
                }
                throw;
            }
        }


        /// <summary>
        /// Here we create a context and ensure that ActivityId is provided.
        /// </summary>
        /// <param name="orchestration"></param>
        /// <param name="inputArgs"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<MicroserviceInstance> createServiceInstanceAsync(Type orchestration, object inputArgs, Dictionary<string, object> context, string version = null)
        {
            ensureActIdInContext(context, inputArgs);

            var ms = new MicroserviceInstance()
            {
                OrchestrationInstance = await m_HubClient.CreateOrchestrationInstanceAsync(orchestration, inputArgs),
            };
            return ms;
        }

        private static void ensureActIdInContext(Dictionary<string, object> context, object inputArgs)
        {
            OrchestrationInput orchestrationInput = inputArgs as OrchestrationInput;

            if (orchestrationInput != null)
            {
                if (orchestrationInput.Context == null)
                    orchestrationInput.Context = new Dictionary<string, object>(context);

                if (orchestrationInput.Context.ContainsKey(cActivityIdCtxName) == false)
                {
                    orchestrationInput.Context.Add(cActivityIdCtxName, Guid.NewGuid().ToString());
                }
            }
        }



        /// <summary>
        /// Wait on single instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="wait"></param>
        /// <returns></returns>
        public async Task WaitOnInstanceAsync(MicroserviceInstance instance, int wait = int.MaxValue)
        {
            await this.m_HubClient.WaitForOrchestrationAsync(instance.OrchestrationInstance, TimeSpan.FromMilliseconds(wait));
        }


        /// <summary>
        /// Wait on multiple instances.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="microservices"></param>
        public void WaitOnInstances(ServiceHost host, List<MicroserviceInstance> microservices)
        {
            List<Task> waitingTasks = new List<Task>();

            foreach (var microservice in microservices)
            {
                waitingTasks.Add(host.WaitOnInstanceAsync(microservice));
            }

            Task.WaitAll(waitingTasks.ToArray());
        }


    }
}
