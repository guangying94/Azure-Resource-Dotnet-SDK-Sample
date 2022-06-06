# Sample repo to use Azure Resource Manager SDK
This repository is a sample console app to create an Azure Virtual Machine (Custom Image) using C# SDK. The code assumes that you already provisioned Virtual Network and respective subnet for Network Interface Card. It doesn't contain any Network Security Group, please modify accordingly based on the requirement.

### How to use
1. Create service principal.
2. Update environment variable.
   ```bash
   # Sample for bash shell
   export AZURE_CLIENT_ID="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
   export AZURE_CLIENT_SECRET="xxxxx"
   export AZURE_TENANT_ID="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
   export AZURE_SUBSCRIPTION_ID="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
   ```
3. Modify parameters in **appsettings.json** to your need.

The code can be port over to other type of deployment such as containers, serverless options like Azure Functions etc.

## Custom VM Image
The code creates a Windows Server custom image, built by [Azure Image Builder](https://docs.microsoft.com/en-us/azure/virtual-machines/image-builder-overview).

Instruction on creating custom image with Azure Image Builder:
1. [Windows](https://docs.microsoft.com/en-us/azure/virtual-machines/windows/image-builder-galleryP)
2. [Linux](https://docs.microsoft.com/en-us/azure/virtual-machines/linux/image-builder-gallery)


## Reference
1. [Managing Resources using Azure .NET SDK](https://docs.microsoft.com/en-us/samples/azure-samples/azure-samples-net-management/resources-manage-resources/)
2. [Azure SDK Release](https://azure.github.io/azure-sdk/releases/latest/)