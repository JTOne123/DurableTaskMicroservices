﻿//  ----------------------------------------------------------------------------------
//  Copyright daenet Gesellschaft für Informationstechnologie mbH
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  http://www.apache.org/licenses/LICENSE-2.0
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  ----------------------------------------------------------------------------------
using Daenet.DurableTaskMicroservices.Common.Base;
using Daenet.DurableTaskMicroservices.Common.Entities;
using Daenet.DurableTaskMicroservices.Core;
using DurableTask.Core;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Daenet.DurableTaskMicroservices.Common.Adapters
{
    /// <summary>
    /// Creates a single file in queue folder and invoke the mapper which maps the instance to a single text line.
    /// </summary>
    /// <typeparam name="TFileSendAdapterInput"></typeparam>
    public class FileSendAdapter<TFileSendAdapterInput> : SendAdapterBase<TFileSendAdapterInput, Null>
        where TFileSendAdapterInput : FileSendAdapterInput
    {
        /// <summary>
        /// Executes sending of data to folder. This adapter creates a single file for every 'input'.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        protected override Null SendData(TaskContext context, TFileSendAdapterInput input, ILogger logger)
        {
            try
            {
                var config = this.GetConfiguration<FileSendAdapterConfig>(input.Orchestration);
                  if (!Directory.Exists(config.QueueFolder))
                    throw new Exception("No receive folder!");

                  dynamic dynInput = config;

                  string fileName = Path.Combine(config.QueueFolder, Guid.NewGuid().ToString() + "_" + config.FileName);

                  fileName = String.Format(fileName, getPropertyValue(input));

                  using (StreamWriter sw = new StreamWriter(fileName))
                  {
                      var mapper = Factory.GetAdapterMapper(config.MapperQualifiedName);

                      object line = mapper.Map(config);

                      sw.WriteLine(line as string);
                  }
            }
            catch (Exception ex)
            {
                throw;
            }

            return new Null();
        }


        /// <summary>
        /// Gets the value of the property by using reflection.
        /// </summary>
        /// <param name="input">The input instance of the task.</param>
        /// will be used for formatting of the file name.</param>
        /// <returns></returns>
        private object getPropertyValue(FileSendAdapterInput input)
        {
            var cfg = this.GetConfiguration<FileSendAdapterConfig>(input.Orchestration);

            string propname = null;
            if (cfg != null)
                propname = cfg.PropertyName;

            if (input == null)
                return String.Empty;

            if (input.Data == null)
                return String.Empty;

            if (propname == null)
                return String.Empty;

            var prop = input.GetType().GetProperty(propname, global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.Public);
            if (prop == null)
                return String.Empty;

            return prop.GetValue(input.Data);
        }
    }
}
