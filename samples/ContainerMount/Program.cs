using Microsoft.Windows.ComputeVirtualization;
using Microsoft.Windows.ComputeVirtualization.Schema;
using System;

namespace ContainerMount
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var id = Guid.Parse("716025c3-441b-4ae5-a985-1b44fa698530");
                Layer[] layers;
                switch (args[0])
                {
                    case "-dismount":
                        ContainerStorage.DismountSandbox(args[1]);
                        break;

                    case "-mount":
                        layers = new Layer[] { new Layer { Id = id, Path = args[2] } };
                        var sandbox = ContainerStorage.MountSandbox(args[1], layers);
                        Console.Out.WriteLine(sandbox.MountPath);
                        break;

                    case "-create":
                        layers = new Layer[] { new Layer { Id = id, Path = args[2] } };
                        ContainerStorage.CreateSandbox(args[1], layers);
                        break;

                    case "-destroy":
                        ContainerStorage.DestroyLayer(args[1]);
                        break;

                    case "-process":
                        ContainerStorage.ProcessBaseLayer(args[1]);
                        break;

                    case "-processvm":
                        ContainerStorage.ProcessUtilityVMImage(args[1]);
                        break;

                    case "-list":
                        {
                            IHcs h = HcsFactory.GetHcs();
                            string computeSystems;
                            ContainerProperties[] containers;

                            h.EnumerateComputeSystems(args[1], out computeSystems);
                            containers = JsonHelper.FromJson<ContainerProperties[]>(computeSystems);
                            Console.Out.WriteLine(computeSystems);

                            foreach (ContainerProperties container in containers)
                            {
                                Console.Out.WriteLine("Id: " + container.Id.ToString());
                                Console.Out.WriteLine("Name: " + container.Name);
                                Console.Out.WriteLine("State: " + container.State);
                                Console.Out.WriteLine("RuntimeOsType: " + container.RuntimeOsType);
                                Console.Out.WriteLine("RuntimeId: " + container.RuntimeId.ToString());
                                Console.Out.WriteLine("Owner: " + container.Owner);
                                Console.Out.WriteLine();
                            }
                        }
                        break;

                    case "-properties":
                        {
                            IHcs h = HcsFactory.GetHcs();
                            string computeSystems;
                            ContainerProperties[] containers;
                            string searchString = String.Format("{{\"Ids\":[\"{0}\"]}}", args[1]);
                            IntPtr computeSystem;
                            string properties;
                            Container container;

                            Console.Out.WriteLine(searchString);
                            h.EnumerateComputeSystems(searchString, out computeSystems);
                            Console.Out.WriteLine(computeSystems);
                            h.OpenComputeSystem(args[1], out computeSystem);
                            h.GetComputeSystemProperties(computeSystem, "{}", out properties);
                            Console.Out.WriteLine(properties);
                            h.CloseComputeSystem(computeSystem);

                            container = HostComputeService.GetComputeSystem(args[1]);


                        }
                        break;

                    case "-networks":
                        {
                            /*
                             * IHns h = HnsFactory.GetHns();
                            */
                            HNSNetwork[] networks = HostNetworkingService.HNSListNetworkRequest("GET", (args.Length > 1 ? args[1] : ""), (args.Length > 2 ? args[2] : ""));

                            if (networks != null)
                            {
                                foreach (HNSNetwork n in networks)
                                {
                                    Console.Out.WriteLine("Id: " + n.ID);
                                    Console.Out.WriteLine("Name: " + n.Name);
                                    Console.Out.WriteLine("NetworkAdapterName: " + n.NetworkAdapterName);
                                    Console.Out.WriteLine("Type: " + n.Type);
                                    Console.Out.WriteLine();
                                }
                            }
                        }
                        break;

                    case "-createnetwork":
                        {
                            // New-HnsNetwork -Type ICS -JsonString '{"Name": "SuperHappyFunNetwork","Type": "ICS","Flags": 9}'
                            //    string request = @"{""Name"": ""SuperHappyFunNetwork"",""Flags"":""9"",""IsolateSwitch"":""True"",""Type"":""ICS""}";
                            string request = @"{""Name"": ""SuperHappyFunNetwork"",""Type"": ""ICS"",""Flags"": 9}";
                            string result;

                            IHns hns = HnsFactory.GetHns();

                            hns.Call("POST", "/networks", request, out result);
                            Console.Out.WriteLine(result);

                            //                        var result = HostNetworkingService.HNSNetworkRequest("POST", "", request);

                        }
                        break;

                    case "-endpoints":
                        {
                            /*
                             * IHns h = HnsFactory.GetHns();
                            */
                            HNSEndpoint[] endpoints = HostNetworkingService.HNSListEndpointRequest("GET", (args.Length > 1 ? args[1] : ""), (args.Length > 2 ? args[2] : ""));

                            if (endpoints != null)
                            {
                                foreach (var endpoint in endpoints)
                                {
                                    Console.Out.WriteLine("Id: " + endpoint.ID);
                                    Console.Out.WriteLine("Name: " + endpoint.Name);
                                    Console.Out.WriteLine("IPAddress: " + endpoint.IPAddress);
                                    Console.Out.WriteLine("MacAddress: " + endpoint.MacAddress);
                                    Console.Out.WriteLine("VirtualNetworkName: " + endpoint.VirtualNetworkName);
                                    Console.Out.WriteLine();
                                }
                            }
                        }
                        break;

                    case "-createvm":
                        {
/*                            string request = @"
{
    ""SchemaVersion"": {""Major"": 2,""Minor"": 1},
    ""Owner"": ""mariner"",
    ""ShouldTerminateOnLastHandleClosed"": true,
    ""VirtualMachine"": {
        ""Chipset"": {
            ""Uefi"": {
                ""BootThis"": {
                    ""DevicePath"": ""Primary disk"",
                    ""DiskNumber"": 0,
                    ""DeviceType"": ""ScsiDrive""
                }
            }
        },
        ""ComputeTopology"": {
            ""Memory"": {
                ""Backing"": ""Virtual"",
                ""SizeInMB"": 2048
            },
            ""Processor"": {
                ""Count"": 2
            }
        }
    }
}";
*/

                            string request = @"
{
    ""SchemaVersion"": {""Major"": 2,""Minor"": 1},
    ""Owner"": ""mariner"",
    ""ShouldTerminateOnLastHandleClosed"": false,
    ""VirtualMachine"": {
        ""Chipset"": {
            ""Uefi"": {
                ""BootThis"": {
                    ""DevicePath"": ""Primary disk"",
                    ""DiskNumber"": 0,
                    ""DeviceType"": ""ScsiDrive""
                }
            }
        },
        ""ComputeTopology"": {
            ""Memory"": {
                ""Backing"": ""Virtual"",
                ""SizeInMB"": 2048
            },
            ""Processor"": {
                ""Count"": 2
            }
        },
        ""Devices"": {
            ""Scsi"": {
                ""Primary disk"": {
                    ""Attachments"": {
                        ""0"": {
                            ""Type"": ""VirtualDisk"",
                            ""Path"": ""c:\\users\\neilsh\\Desktop\\mariner\\core-1.0.20210224.vhdx""
                        }
                    }
                }
            }
        }
    }
}";

                            IHcs h = HcsFactory.GetHcs();
                            IntPtr identity = IntPtr.Zero;
                            IntPtr computeSystem;

                            h.CreateComputeSystem(id.ToString(), request, identity, out computeSystem);

                            

                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.ToString());
            }
        }
    }
}


/*
 *                             string request2 = @"
{
    ""Owner"": ""mariner"",
    ""SchemaVersion"": {""Major"": 2,""Minor"": 2},
    ""VirtualMachine"": {
        ""StopOnReset"": true,
        ""Chipset"": {
            ""UseUtc"": true,
            ""LinuxKernelDirect"": {
                ""KernelFilePath"": ""C:\\WINDOWS\\system32\\lxss\\tools\\kernel"",
                ""InitRdPath"": ""C:\\WINDOWS\\system32\\lxss\\tools\\initrd.img"",
                ""KernelCmdLine"": ""initrd=\\initrd.img panic=-1 pty.legacy_count=0 nr_cpus=16""
            }
        },
        ""ComputeTopology"": {
            ""Memory"": {
                ""SizeInMB"": 26124,
                ""AllowOvercommit"": true,
                ""EnableColdDiscardHint"": true,
                ""EnableDeferredCommit"": true
            },
            ""Processor"": {
                ""Count"": 16
            }
        },
        ""Devices"": {
            ""Scsi"": {
                ""0"": {
                    ""Attachments"": { }
                }
            },
            ""HvSocket"": {
                ""HvSocketConfig"": {
                    ""DefaultBindSecurityDescriptor"": ""D:P(A;;FA;;;SY)(A;;FA;;;S-1-12-1-3719691770-1285959622-2873335948-1678436016)"",
                    ""DefaultConnectSecurityDescriptor"": ""D:P(A;;FA;;;SY)(A;;FA;;;S-1-12-1-3719691770-1285959622-2873335948-1678436016)""
                }
            },
            ""Plan9"": { },
            ""Battery"": { }
        }
    },
    ""ShouldTerminateOnLastHandleClosed"": true
}
                        ";
*/