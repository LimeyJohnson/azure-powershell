﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.Azure;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Management.Compute;
using Microsoft.WindowsAzure.Management.Compute.Models;
using System.Management.Automation;

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.HostedServices
{
    /// <summary>
    /// Migrate ASM deployment to ARM
    /// </summary>
    [Cmdlet(VerbsCommon.Move, "AzureService"), OutputType(typeof(OperationStatusResponse))]
    public class MoveAzureServiceCommand : ServiceManagementBaseCmdlet
    {
        private const string AbortParameterSetName = "AbortMigrationParameterSet";
        private const string CommitParameterSetName = "CommitMigrationParameterSet";
        private const string PrepareNewParameterSetName = "PrepareNewMigrationParameterSet";
        private const string PrepareExistingParameterSetName = "PrepareExistingMigrationParameterSet";

        private string DestinationVNetType;

        [Parameter(
            Position =0,
            Mandatory = true,
            ParameterSetName = AbortParameterSetName,
            HelpMessage = "Abort migration")]
        public SwitchParameter Abort
        {
            get;
            set;
        }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = CommitParameterSetName,
            HelpMessage = "Commit migration")]
        public SwitchParameter Commit
        {
            get;
            set;
        }

        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = PrepareNewParameterSetName,
            HelpMessage = "Prepare migration")]
        [Parameter(
            Position = 0,
            Mandatory = true,
            ParameterSetName = PrepareExistingParameterSetName,
            HelpMessage = "Prepare migration")]
        public SwitchParameter Prepare
        {
            get;
            set;
        }

        [Parameter(
            Position = 1,
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "Service name to be migrated")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(
            Position = 2,
            Mandatory = true,
            HelpMessage = "Deployment name to be migrated")]
        [ValidateNotNullOrEmpty]
        public string DeploymentName
        {
            get;
            set;
        }

        [Parameter(
            Position = 3,
            Mandatory = true,
            ParameterSetName = PrepareNewParameterSetName,
            HelpMessage = "Prepare migration to new virtual network")]
        public SwitchParameter CreateNewVirtualNetwork
        {
            get;
            set;
        }

        [Parameter(
            Position = 3,
            Mandatory = true,
            ParameterSetName = PrepareExistingParameterSetName,
            HelpMessage = "Prepare migration to existing virtual network")]
        public SwitchParameter UseExistingVirtualNetwork
        {
            get;
            set;
        }


        [Parameter(
            Position = 4,
            Mandatory = true,
            ParameterSetName = PrepareExistingParameterSetName,
            HelpMessage = "Resource group name of existing virtual network for migration")]
        [ValidateNotNullOrEmpty]
        public string VirtualNetworkResourceGroupName
        {
            get;
            set;
        }

        [Parameter(
            Position = 5,
            Mandatory = true,
            ParameterSetName = PrepareExistingParameterSetName,
            HelpMessage = "Virtual network name for migration")]
        [ValidateNotNullOrEmpty]
        public string VirtualNetworkName
        {
            get;
            set;
        }

        [Parameter(
            Position = 6,
            Mandatory = true,
            ParameterSetName = PrepareExistingParameterSetName,
            HelpMessage = "Subnet name for migration")]
        [ValidateNotNullOrEmpty]
        public string SubnetName
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            if (this.Abort.IsPresent)
            {
                ExecuteClientActionNewSM(
                null,
                CommandRuntime.ToString(),
                () => this.ComputeClient.Deployments.AbortMigration(this.ServiceName, DeploymentName));
            }
            else if (this.Commit.IsPresent)
            {
                ExecuteClientActionNewSM(
                null,
                CommandRuntime.ToString(),
                () => this.ComputeClient.Deployments.CommitMigration(this.ServiceName, DeploymentName));
            }
            else
            {
                if (this.CreateNewVirtualNetwork.IsPresent)
                {
                    DestinationVNetType = DestinationVirtualNetwork.New;
                }
                else
                {
                    DestinationVNetType = DestinationVirtualNetwork.Existing;
                }

                var parameter = (this.ParameterSetName == PrepareExistingParameterSetName)
                    ? new PrepareDeploymentMigrationParameters
                    {
                        DestinationVirtualNetwork = this.DestinationVNetType,
                        ResourceGroupName = this.VirtualNetworkResourceGroupName,
                        SubNetName = this.SubnetName,
                        VirtualNetworkName = this.VirtualNetworkName
                    }
                    : new PrepareDeploymentMigrationParameters
                    {
                        DestinationVirtualNetwork = this.DestinationVNetType,
                        ResourceGroupName = string.Empty,
                        SubNetName = string.Empty,
                        VirtualNetworkName = string.Empty
                    };

                ExecuteClientActionNewSM(
                    null,
                    CommandRuntime.ToString(),
                    () => this.ComputeClient.Deployments.PrepareMigration(this.ServiceName, DeploymentName, parameter));
            }
        }
    }
}
