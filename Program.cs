using Azure.Identity;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;

namespace AzureSDK
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("Sample console app to create Windows VM");

            //Define variables
            string resourceGroupName = "SDK-VM";
            string windowsUserName = "azure-user";
            string windowsUserPassword = "Q!W@E#r4t5y6";
            string vnetName = "SDK-VNET";
            string subnetName = "default";
            string aibImageID = "/subscriptions/b841a6ea-692d-4c91-98b9-40ab6aa89212/resourceGroups/aibwinsig/providers/Microsoft.Compute/galleries/myIBSIG/images/winSvrimage/versions/0.25241.35683";

            //unique id
            var unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            string append = unixTimestamp.Substring(unixTimestamp.Length - 4);
            string vmName = $"win-vm-{append}";
            string publicIPName = $"pip-{append}";
            string nicName = $"nic-{append}";

            //ARM Client
            Console.WriteLine("Initialize ARM Client");
            ArmClient armClient = new ArmClient(new DefaultAzureCredential());
            SubscriptionResource subscription = armClient.GetDefaultSubscription();
            ResourceGroupCollection rgCollection = subscription.GetResourceGroups();
            ResourceGroupResource resourceGroup = await subscription.GetResourceGroups().GetAsync(resourceGroupName);

            PublicIPAddressResource publicIP = await CreatePublicIP(resourceGroup, publicIPName);
            NetworkInterfaceResource nic = await CreateNIC(resourceGroup, vnetName, subnetName, nicName, publicIP);
            VirtualMachineResource vm = await CreateVM(resourceGroup, nic, vmName, windowsUserName, windowsUserPassword, aibImageID);

            await Task.Delay(30000);
            StopVM(vm);
            await Task.Delay(30000);
            StartVM(vm);

            return 0;
        }

        public static async Task<PublicIPAddressResource> CreatePublicIP(ResourceGroupResource resourceGroup, string publicIPName)
        {
            Console.WriteLine("Creating Public IP");
            var pip = new PublicIPAddressData
            {
                PublicIPAddressVersion = Azure.ResourceManager.Network.Models.IPVersion.IPv4,
                PublicIPAllocationMethod = IPAllocationMethod.Dynamic,
                Location = resourceGroup.Data.Location
            };

            var publicIPAddressContainer = resourceGroup.GetPublicIPAddresses();
            ArmOperation<PublicIPAddressResource> _publicIPAddress = await publicIPAddressContainer.CreateOrUpdateAsync(Azure.WaitUntil.Completed, publicIPName, pip);
            Console.WriteLine("Public IP created");

            PublicIPAddressResource publicIPAddress = await resourceGroup.GetPublicIPAddressAsync(publicIPName);
            return publicIPAddress;
        }

        public static async Task<NetworkInterfaceResource> CreateNIC(ResourceGroupResource resourceGroup, string vnetName, string subnetName, string nicName, PublicIPAddressResource publicIPAddress)
        {
            Console.WriteLine("Creating NIC");
            VirtualNetworkResource vNET = await resourceGroup.GetVirtualNetworkAsync(vnetName);
            SubnetResource nicSubnet = await vNET.GetSubnetAsync(subnetName);

            var networkInterfaceData = new NetworkInterfaceData()
            {
                Location = resourceGroup.Data.Location,
                IPConfigurations =
    {
        new NetworkInterfaceIPConfigurationData
        {
            Name = "Primary",
            Primary = true,
            Subnet = new SubnetData(){Id = nicSubnet.Data.Id},
            PrivateIPAllocationMethod = IPAllocationMethod.Dynamic,
            PublicIPAddress = new PublicIPAddressData(){Id = publicIPAddress.Data.Id}
        }
    }
            };

            var nicContainer = resourceGroup.GetNetworkInterfaces();
            ArmOperation<NetworkInterfaceResource> _nic = await nicContainer.CreateOrUpdateAsync(Azure.WaitUntil.Completed, nicName, networkInterfaceData);
            Console.WriteLine("NIC created...");

            NetworkInterfaceResource nic = await resourceGroup.GetNetworkInterfaceAsync(nicName);
            return nic;
        }

        public static async Task<VirtualMachineResource> CreateVM(ResourceGroupResource resourceGroup, NetworkInterfaceResource nic, string vmName, string windowsUserName, string windowsUserPassword, string aibImageID)
        {
            Console.WriteLine("Creating VMs");
            VirtualMachineCollection vmCollection = resourceGroup.GetVirtualMachines();
            var input = new VirtualMachineData(resourceGroup.Data.Location)
            {
                HardwareProfile = new HardwareProfile()
                {
                    VmSize = VirtualMachineSizeTypes.StandardB2Ms
                },
                OSProfile = new OSProfile()
                {
                    ComputerName = vmName,
                    AdminUsername = windowsUserName,
                    AdminPassword = windowsUserPassword,
                },
                NetworkProfile = new NetworkProfile()
                {
                    NetworkInterfaces =
        {
            new NetworkInterfaceReference()
            {
                Id = nic.Data.Id,
                Primary = true
            }
        }
                },
                StorageProfile = new StorageProfile()
                {
                    OSDisk = new OSDisk(DiskCreateOptionTypes.FromImage)
                    {
                        OSType = OperatingSystemTypes.Windows,
                        Caching = CachingTypes.ReadWrite,
                        CreateOption = DiskCreateOptionTypes.FromImage,
                        ManagedDisk = new ManagedDiskParameters()
                        {
                            StorageAccountType = StorageAccountTypes.StandardLRS
                        }
                    },
                    ImageReference = new ImageReference()
                    {
                        Id = aibImageID
                    }
                }
            };

            ArmOperation<VirtualMachineResource> lro = await vmCollection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, vmName, input);

            Console.WriteLine("VM created");

            VirtualMachineResource vm = await resourceGroup.GetVirtualMachineAsync(vmName);
            return vm;
        }

        public static async void StopVM(VirtualMachineResource vm)
        {
            await vm.DeallocateAsync(Azure.WaitUntil.Completed);
            Console.WriteLine("The VM is deallocated");
        }

        public static async void StartVM(VirtualMachineResource vm)
        {
            await vm.PowerOnAsync(Azure.WaitUntil.Completed);
            Console.WriteLine("The VM is started");
        }
    }
}