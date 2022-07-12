# IBM Datapower

The IBM DataPower Orchestrator allows for the management of certificates in the IBM Datapower platform. Inventory and Management functions are supported.

#### Integration status: Pilot - Ready for use in test environments. Not for use in production.

## About the Keyfactor Windows Orchestrator AnyAgent

This repository contains a Windows Orchestrator AnyAgent, which is a plugin to the Keyfactor Windows Orchestrator. Within the Keyfactor Platform, Orchestrators are used to manage “certificate stores” &mdash; collections of certificates and roots of trust that are found within and used by various applications.

The Windows Orchestrator is part of the Keyfactor software distribution and is available via the Keyfactor customer portal. For general instructions on installing AnyAgents, see the “Keyfactor Command Orchestrator Installation and Configuration Guide” section of the Keyfactor documentation. For configuration details of this specific AnyAgent, see below in this readme.

Note that in Keyfactor Version 9, the Windows Orchestrator have been replaced by the Universal Orchestrator. While this AnyAgent continues to work with the Windows Orchestrator, and the Windows Orchestrator is supported alongside the Universal Orchestrator talking to Keyfactor version 9, AnyAgent plugins cannot be used with the Universal Orchestrator.

---




---

﻿*** 

**1) Create the new Certificate store Type for the New DataPower AnyAgent**

In Keyfactor Command create a new Certificate Store Type similar to the one below:
- NOTE: This orchestrator _does not_ actually support the Create function per the image below

![image.png](/Images/CertStoreTypes.png)


- **Name** – Required. The display name of the new Certificate Store Type
- **Short Name** – Required. MUST match Short Name Defined in DatapowerAnyAgent.dll.config.  It is configurable but the names must match!  You will find this configuration file in the same plugin directory that the IBM DataPower AnyAgent code runs in.  The default name in the config file is "Data Power" if you want to use that and not change anything.

		<!--This should match the *short* name of the store type you created in Keyfactor Portal-->
		<add key="StoreType" value="Data Power"/>

- **Needs Server, Blueprint Allowed** – checked as shown
- **Requires Store Password, Supports Entry Password** – unchecked as shown
- **Supports Custom Alias** – Forbidden. Not used.
- **Use PowerShell** – Unchecked
- **Store PathType** – Freeform (user will enter the the location of the store).
- **Private Keys** – Optional
- **PFX Password Style** – Default
- **Job Types** – Inventory, Add, and Remove are the 4 job types implemented by this AnyAgent
- **Custom Parameters** :
 
    These are completely optional but if you want to use them to create a prefix to the certificate Objects, Keys and Files you can use them so your key will be named [prefix]Name.  If you choose to use them, they must exactly match what is shown below (all string values that are not required):

1.   **Name:** CryptoCertObjectPrefix
     **Display Name:** Crypto Certificate Object Prefix

2.   **Name:** CertFilePrefix
     **Display Name:** Certificate File Prefix

3.   **Name:** CryptoKeyObjectPrefix
     **Display Name:** Crypto Key Object Prefix

4.   **Name:** KeyFilePrefix
     **Display Name:**  Key File Prefix
    
    
**2) Register the DataPower AnyAgent with Keyfactor**

Open the Keyfactor Windows Agent Configuration Wizard and perform the tasks as illustrated below:

![image.png](/Images/ConfigWizard1.png)

- Click **<Next>**

![image.png](/Images/ConfigWizard2.png)

If you have configured the agent service previously, you should be able to skip to just click **<Next>**. Otherwise, enter the service account Username and Password you wish to run the Keyfactor Windows Agent Service under, click **<Update Windows Service Account>** and click **<Next>**.

![image.png](/Images/ConfigWizard3.png)

If you have configured the agent service previously, you should be able to skip to just re-enter the password to the service account the agent service will run under, click **<Validate Keyfactor Connection>** and then **<Next>**.

![image.png](/Images/ConfigWizard4.png)

Select the agent you are adding capabilities for (in this case, IBM Data Power Cert, and also select the specific capabilities (Inventory and Management in this example). Click **<Next>**.

![image.png](/Images/ConfigWizard5.png)

For each AnyAgent implementation, check Load assemblies containing extension modules from other location , browse to the location of the compiled AnyAgent dlls, and click **<Validate Capabilities>**. Once all AnyAgents have been validated, click **<Apply Configuration>**.

![image.png](/Images/ConfigComplete.png)

If the Keyfactor Agent Configuration Wizard configured everything correctly, you should see the dialog above.

**3) Create a Cert Store within the Keyfactor Portal**

Navigate to Certificate Locations => Certificate Stores within Keyfactor Command to add an IBM Data Power certificate store. Below are the values that should be entered.

![image.png](/Images/CertStores1.png)

- **Category** – Required. The IBM Data Power Cert category name must be selected
- **Container** – Optional. Select a container if utilized.
- **Client Machine** – Required. The server name or IP Address of the IBM DataPower API
- **Store Path** – Required.  This will be one of the following based on what you are looking to add to the store.
1. **[DomainName]\cert** where [DomainName] is the name of the domain in DataPower you are looking to manage and inventory.

2.  **cert** - This will use the default domain in DataPower to manage and inventory **domain** certs

3. **[DomainName]\pubcert** - This will give you the ability to Inventory the Pub Cert Folder on the specified domain where [DomainName] is the name of the domain in DataPower you are looking to inventory.  

4. **pubcert** - This will use the default domain in DataPower to manage and inventory **public certs** certs


 ***

### License
[Apache](https://apache.org/licenses/LICENSE-2.0)

