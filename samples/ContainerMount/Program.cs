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
                HcsNotificationWatcher watcher;

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

                            h.EnumerateComputeSystems("{}", out computeSystems);
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

                    case "-terminate":
                        {
                            IHcs h = HcsFactory.GetHcs();
                            string computeSystems;
                            string searchString = String.Format("{{\"Ids\":[\"{0}\"]}}", args[1]);
                            IntPtr computeSystem;

                            Console.Out.WriteLine(searchString);
                            h.EnumerateComputeSystems(searchString, out computeSystems);
                            Console.Out.WriteLine(computeSystems);
                            h.OpenComputeSystem(args[1], out computeSystem);
                            h.TerminateComputeSystem(computeSystem, null);
                            h.CloseComputeSystem(computeSystem);
                        }
                        break;

                    case "-start":
                        {
                            IHcs h = HcsFactory.GetHcs();
                            string computeSystems;
                            string searchString = String.Format("{{\"Ids\":[\"{0}\"]}}", args[1]);
                            IntPtr computeSystem;

                            Console.Out.WriteLine(searchString);
                            h.EnumerateComputeSystems(searchString, out computeSystems);
                            Console.Out.WriteLine(computeSystems);
                            h.OpenComputeSystem(args[1], out computeSystem);
                            watcher = new HcsNotificationWatcher(
                                computeSystem,
                                h.RegisterComputeSystemCallback,
                                h.UnregisterComputeSystemCallback,
                                new HCS_NOTIFICATIONS[]{
                                    HCS_NOTIFICATIONS.HcsNotificationSystemExited,
                                    HCS_NOTIFICATIONS.HcsNotificationSystemCreateCompleted,
                                    HCS_NOTIFICATIONS.HcsNotificationSystemStartCompleted
                                    }
                                );

                            h.StartComputeSystem(computeSystem, null);

                            while (!watcher.WatchAsync(HCS_NOTIFICATIONS.HcsNotificationSystemStartCompleted).IsCompleted)
                            {
                                watcher.Wait(HCS_NOTIFICATIONS.HcsNotificationSystemCreateCompleted, 10);
                                NotificationResult foo = watcher.WatchAsync(HCS_NOTIFICATIONS.HcsNotificationSystemStartCompleted).Result;
                            }

                            NotificationResult result = watcher.WatchAsync(HCS_NOTIFICATIONS.HcsNotificationSystemStartCompleted).Result;

                            h.CloseComputeSystem(computeSystem);
                        }
                        break;

                    case "-shutdown":
                        {
                            IHcs h = HcsFactory.GetHcs();
                            string computeSystems;
                            string searchString = String.Format("{{\"Ids\":[\"{0}\"]}}", args[1]);
                            IntPtr computeSystem;

                            Console.Out.WriteLine(searchString);
                            h.EnumerateComputeSystems(searchString, out computeSystems);
                            Console.Out.WriteLine(computeSystems);
                            h.OpenComputeSystem(args[1], out computeSystem);
                            watcher = new HcsNotificationWatcher(
                                computeSystem,
                                h.RegisterComputeSystemCallback,
                                h.UnregisterComputeSystemCallback,
                                new HCS_NOTIFICATIONS[]{
                                    HCS_NOTIFICATIONS.HcsNotificationSystemExited,
                                    HCS_NOTIFICATIONS.HcsNotificationSystemShutdownCompleted,
                                    }
                                );

                            h.ShutdownComputeSystem(computeSystem, null);

                            while (!watcher.WatchAsync(HCS_NOTIFICATIONS.HcsNotificationSystemShutdownCompleted).IsCompleted)
                            {
                                watcher.Wait(HCS_NOTIFICATIONS.HcsNotificationSystemShutdownCompleted, 10);
                                NotificationResult foo = watcher.WatchAsync(HCS_NOTIFICATIONS.HcsNotificationSystemShutdownCompleted).Result;
                            }

                            NotificationResult result = watcher.WatchAsync(HCS_NOTIFICATIONS.HcsNotificationSystemShutdownCompleted).Result;

                            h.CloseComputeSystem(computeSystem);
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

                    case "-networks-raw":
                        {
                            IHns h = HnsFactory.GetHns();
                            string result;

                            h.Call("GET", "/networks", "", out result);

                            Console.Out.WriteLine(result);
                        }
                        break;

                    case "-create-network":
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

                    case "-delete-network":
                        {
                            IHns hns = HnsFactory.GetHns();
                            string result;

                            hns.Call("DELETE", "/networks/" + args[1], "", out result);
                            Console.Out.WriteLine(result);
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

                    case "-endpoints-raw":
                        {
                            IHns h = HnsFactory.GetHns();
                            string result;

                            h.Call("GET", "/endpoints", "", out result);

                            Console.Out.WriteLine(result);
                        }
                        break;

                    case "-create-endpoint":
                        {
                            /*
                             * IHns h = HnsFactory.GetHns();
                            */
                            string request = @"{""VirtualNetwork"": """ + args[1] + @"""}";

                            HNSEndpoint endpoint = HostNetworkingService.HNSEndpointRequest("POST", "", request);

                            Console.Out.WriteLine("Id: " + endpoint.ID);
                            Console.Out.WriteLine("Name: " + endpoint.Name);
                            Console.Out.WriteLine("IPAddress: " + endpoint.IPAddress);
                            Console.Out.WriteLine("MacAddress: " + endpoint.MacAddress);
                            Console.Out.WriteLine("VirtualNetworkName: " + endpoint.VirtualNetworkName);
                            Console.Out.WriteLine();
                        }
                        break;

                    case "-delete-endpoint":
                        {
                            IHns hns = HnsFactory.GetHns();
                            string result;

                            hns.Call("DELETE", "/endpoints/" + args[1], "", out result);
                            Console.Out.WriteLine(result);
                        }
                        break;

                    case "-createvm":
                        {
                            string request_start = @"
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
                    ""DeviceType"": ""ScsiDrive"",
                    ""OptionalData"": """"
                },
                ""Console"": ""ComPort1""
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
                            ""Type"": ""VirtualDisk""," +
#if true
                            @"""Path"": ""c:\\hyperv\\core-1.0.20210408.vhdx""" +
#else
                            @"""Path"": ""c:\\hyperv\\core-1.0.20210224.vhdx""" +
#endif
@"
                        }
                    }
                }
            },
            ""NetworkAdapters"": {
                ""Primary network"": {
                    ""EndpointId"": """;
                            string request_end =
@"""
                }
            },
            ""VideoMonitor"": {},
            ""Keyboard"": { },
            ""Mouse"": { },
            ""ComPorts"": {
                ""0"" : {
                    ""NamedPipe"": ""\\\\.\\pipe\\vmpipe"",
                    ""OptimizeForDebugger"": false
                },
                ""1"" : {
                    ""NamedPipe"": ""\\\\.\\pipe\\vmpipe2"",
                    ""OptimizeForDebugger"": false
                }
            }
        }
    }
}";

                            IHcs h = HcsFactory.GetHcs();
                            IntPtr identity = IntPtr.Zero;
                            IntPtr computeSystem;
                            string properties;
                            // assumes args[1] is endpoint id
                            h.CreateComputeSystem(id.ToString(), request_start + args[1] + request_end, identity, out computeSystem);
                            watcher = new HcsNotificationWatcher(
                                computeSystem,
                                h.RegisterComputeSystemCallback,
                                h.UnregisterComputeSystemCallback,
                                new HCS_NOTIFICATIONS[]{
                                    HCS_NOTIFICATIONS.HcsNotificationSystemExited,
                                    HCS_NOTIFICATIONS.HcsNotificationSystemCreateCompleted,
                                    HCS_NOTIFICATIONS.HcsNotificationSystemStartCompleted
                                    }
                                );

                            watcher.Wait(HCS_NOTIFICATIONS.HcsNotificationSystemCreateCompleted);

                            h.GetComputeSystemProperties(computeSystem, "{}", out properties);
                            Console.Out.WriteLine(properties);
                        }
                        break;

                    case "-createwsl":
                        {
                            string request = @"
{
    ""Owner"": ""WSL"",
    ""SchemaVersion"": { ""Major"": 2,""Minor"": 2},
    ""VirtualMachine"": {
        ""StopOnReset"": true,
        ""Chipset"": {
            ""UseUtc"": true," +
#if false
            @"""LinuxKernelDirect"": {
                ""KernelFilePath"": ""C:\\WINDOWS\\system32\\lxss\\tools\\kernel"",
                ""InitRdPath"": ""C:\\WINDOWS\\system32\\lxss\\tools\\initrd.img"",
                ""KernelCmdLine"": ""earlyprintk=ttyS0 earlycon=uart8250,mmio32,0x68A10000 console=ttyS0 initrd=\\initrd.img panic=-1 pty.legacy_count=0 nr_cpus=2""
            }" +
#elif false
            @"""LinuxKernelDirect"": {
                ""KernelFilePath"": ""C:\\WINDOWS\\system32\\lxss\\tools\\kernel"",
                ""InitRdPath"": ""C:\\WINDOWS\\system32\\lxss\\tools\\initrd.img"",
                ""KernelCmdLine"": ""earlyprintk=ttyS1 earlycon=uart8250,mmio32,0x68A20000 console=ttyS1 initrd=\\initrd.img panic=-1 pty.legacy_count=0 nr_cpus=2""
            }" +
#elif true
            @"""Uefi"": {
                ""BootThis"": {
                    ""DeviceType"": ""VmbFs"",
                    ""VmbFsRootPath"": ""C:\\WINDOWS\\system32\\lxss\\tools"",
                    ""DevicePath"": ""\\kernel"",
                    ""DiskNumber"": 0,
                    ""OptionalData"": ""earlyprintk=ttyS0 earlycon=uart8250,mmio32,0x68A10000 console=ttyS0 initrd=\\initrd.img panic=-1 pty.legacy_count=0 nr_cpus=2""
                },
                ""Console"": ""ComPort1""
            }" +
#else
            @"""Uefi"": {
                ""BootThis"": {
                    ""DeviceType"": ""VmbFs"",
                    ""VmbFsRootPath"": ""C:\\WINDOWS\\system32\\lxss\\tools"",
                    ""DevicePath"": ""\\kernel"",
                    ""DiskNumber"": 0,
                    ""OptionalData"": ""earlyprintk=ttyAMA0 earlycon=pl011,0xeffeb000 console=ttyAMA0 initrd=\\initrd.img panic=-1 pty.legacy_count=0 nr_cpus=2""
                },
                ""Console"": ""ComPort2""
            }" +
#endif
@"
        },
        ""ComputeTopology"": {
            ""Memory"": {
            ""SizeInMB"": 26124,
            ""AllowOvercommit"": true,
            ""EnableColdDiscardHint"": true,
            ""EnableDeferredCommit"": true
            },
            ""Processor"": {
                ""Count"": 2
            }
        },
        ""Devices"": {
            ""Scsi"": {
                ""0"": {
                    ""Attachments"": { }
                }
            },
            ""Plan9"": { },
            ""Battery"": { },
            ""ComPorts"": {
                ""0"" : {
                    ""NamedPipe"": ""\\\\.\\pipe\\vmpipe"",
                    ""OptimizeForDebugger"": false
                },
                ""1"" : {
                    ""NamedPipe"": ""\\\\.\\pipe\\vmpipe1"",
                    ""OptimizeForDebugger"": false
                }
            }
        }
    },
    ""ShouldTerminateOnLastHandleClosed"": false
}";

/*
,
            ""HvSocket"": {
                ""HvSocketConfig"": {
                    ""DefaultBindSecurityDescriptor"":""D:P(A;;FA;;;SY)(A;;FA;;;S-1-12-1-3719691770-1285959622-2873335948-1678436016)"",
                    ""DefaultConnectSecurityDescriptor"":""D:P(A;;FA;;;SY)(A;;FA;;;S-1-12-1-3719691770-1285959622-2873335948-1678436016)""
                }
            }
*/

                            IHcs h = HcsFactory.GetHcs();
                            IntPtr identity = IntPtr.Zero;
                            IntPtr computeSystem;
                            string properties;

                            h.CreateComputeSystem(id.ToString(), request, identity, out computeSystem);
                            watcher = new HcsNotificationWatcher(
                                computeSystem,
                                h.RegisterComputeSystemCallback,
                                h.UnregisterComputeSystemCallback,
                                new HCS_NOTIFICATIONS[]{
                                    HCS_NOTIFICATIONS.HcsNotificationSystemExited,
                                    HCS_NOTIFICATIONS.HcsNotificationSystemCreateCompleted,
                                    HCS_NOTIFICATIONS.HcsNotificationSystemStartCompleted
                                    }
                                );

                            watcher.Wait(HCS_NOTIFICATIONS.HcsNotificationSystemCreateCompleted);

                            h.GetComputeSystemProperties(computeSystem, "{}", out properties);
                            Console.Out.WriteLine(properties);
                        }
                        break;

                    default:
                        Console.Out.WriteLine("Unknown command");
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
