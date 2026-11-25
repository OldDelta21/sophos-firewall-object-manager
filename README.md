# Sophos Firewall Object Manager

An unofficial Windows desktop tool for importing, exporting, and scheduling Sophos Firewall address objects, groups, and rules.

## Download

Download **SophosFirewallObjectManager-win-x64.exe** from [the latest release](https://github.com/OldDelta21/sophos-firewall-object-manager/releases/latest). The standalone Windows x64 executable includes .NET and runs without an installer.

Open the existing .NET 8 Windows Forms project in Visual Studio with the .NET Desktop Development workload. Right-click **Form1.cs** and choose **View Designer** to edit the layout. The controls live in **Form1.Designer.cs**, and the form and executable use the icon from the Sophos Firewall XML API reference.

**Build** in Visual Studio produces a self-contained Windows x64 executable in `bin\<configuration>\win-x64\`. Ship the EXE from that directory. Compilation files and debugging dependencies are staged under `obj\build\`. Run the bundled EXE from a stable location before registering scheduled imports, since Windows Task Scheduler saves its path.

## Connect and transfer

Choose **Save connection** to remember the admin origin, username, password, SFOS documentation family, and API override. The profile is encrypted with Windows DPAPI for the current Windows user and restored on launch. **Forget connection** removes the profile. Saved tasks keep their own independent encrypted settings.

Closing the main window hides it in the Windows notification area. Double-click the icon or choose **Open** to restore the window. Right-click the icon and choose **Exit** to quit. Scheduled imports continue to use Windows Task Scheduler after the window is closed or the app exits.

1. Enable XML API access in Sophos Firewall and allow the PC's address. Enter the HTTPS admin origin, username and password. Windows must trust the appliance certificate and the address must match the certificate. **Test connection** checks authentication and shows the XML API version when the firewall reports it. **API override** accepts a specific XML API version if needed.
2. Choose **File**, **HTTPS URL** or **Paste**, then select the input format. The URL must point directly to the file. For a refreshed URL, preview again to fetch the current content. The preview is the exact XML submitted when you apply it.
3. Choose **Create only**, **Update only**, or **Create or update by name**. The combined mode uses a name filter to decide whether to add or update and requires the entity to support Get by Name. Apply stops at the first unconfirmed write. The Activity tab records confirmed writes; earlier writes remain applied.
4. To export, enter an XML entity tag such as `IPHost`, `IPHostGroup`, or `FirewallRule`. The resulting XML or JSON may contain sensitive appliance configuration. Exported objects can require read-only fields to be removed and dependencies to be recreated before import on another appliance.

### Address input

Use **Add to source list** to combine files, HTTPS URLs, and pasted inputs in one import. Each source keeps its own format. **Browse** can add multiple files in one selection. **Import sources** shows the inputs that will be previewed and saved with a task. With an empty source list, the app uses the current input fields. The preview combines address sources before grouping and rule generation. Identical named objects are merged. Conflicting definitions or unreadable sources stop the preview before any writes.

In **Import sources**, select one row to change its list name and optional **First N entries** limit, then click **Update selected source**. Sources that share a list name are combined; different names produce independently named objects, groups and generated rules. An entry limit of zero requests the full feed. **Remove selected** removes the highlighted rows, and **Clear sources** empties the list and the current input fields.

**Add feed template** offers DShield, Spamhaus DROP (including EDROP), FireHOL Level 2, OISD Big and StevenBlack hosts. OISD domain rules become exact FQDNs unless wildcard expansion is enabled. StevenBlack hosts-file sinkhole mappings become domain names. The original sinkhole IP is not imported. OISD Big and StevenBlack are larger than the combined Sophos host ceiling, so set an explicit subset limit before attempting to import them as objects.

For **HTTPS URL** address imports, paste the URL alone. The app recognizes these feeds and handles their formats automatically:

| Feed | URL |
| --- | --- |
| DShield | [block.txt](https://feeds.dshield.org/block.txt), tab-separated network records |
| Spamhaus DROP | [drop_v4.json](https://www.spamhaus.org/drop/drop_v4.json), one JSON object per line. Old DROP TXT tasks resolve to this URL. EDROP was merged into DROP and should be paused if configured separately. |
| FireHOL Level 2 | [firehol_level2.netset](https://raw.githubusercontent.com/firehol/blocklist-ipsets/master/firehol_level2.netset), one IPv4 address or network per line |

Use **Individual hosts** for these feeds because they contain CIDRs. FireHOL Level 2 can approach or exceed the firewall's host capacity, including objects already on the appliance. A deprecated abuse.ch SSLBL IP-only URL is reported as an error, and the app keeps the previous firewall configuration.

If **Test connection** reports a TLS name mismatch, use the DNS name in the admin certificate, or configure an admin certificate with a subject alternative name matching the address you use. If the chain is untrusted, have your administrator deploy the verified issuing CA through Windows trust policy. See [Sophos certificate setup](https://docs.sophos.com/nsg/sophos-firewall/21.5/Help/en-us/webhelp/onlinehelp/AdministratorHelp/Certificates/HowToArticles/CertificatesRemoveUntrustedCertificateError/index.html).

To authenticate a private appliance certificate without changing Windows trust, obtain the SHA-256 fingerprint of the exact admin server certificate from a trusted export or your firewall administrator. Paste it into **Admin certificate SHA-256**, test the connection, and save it. The app requires an exact certificate match for that firewall connection and still checks expiration. The profile and scheduled imports remember the fingerprint. When the firewall rotates the certificate, verify the new fingerprint before updating each saved connection or task. HTTPS import URLs continue to use Windows certificate validation.

**Test connection** can also inspect a rejected certificate without sending credentials and prompt for approval. Compare the displayed fingerprint against a trusted source before selecting **Yes**. The app retries using the exact approved certificate and saves the connection after a successful login. To remove the approval, clear the fingerprint and save the connection. For an existing task, load its settings and save them again after updating its fingerprint.

- TXT uses one address per line. Blank lines and lines starting with `#` are ignored.
- CSV uses an `address` header and an optional `name` header. Example: `address,name` followed by `192.0.2.10,Example-server`.
- JSON accepts an array of address strings or objects with `address` and optional `name`. Example: `["192.0.2.10", {"address":"198.51.100.0/24","name":"Example-network"}]`.

Inputs can include IPv4, IPv6, CIDR, two endpoints separated by a hyphen, and fully qualified domain names. Omitting a name creates a deterministic name from the object prefix and address. Address import creates individual IP or FQDN objects. Upsert updates an object with the same name; a changed unnamed address creates a new object. Rerunning a URL import never deletes objects or rules absent from the new source.

### Generic entity input

Use **Entity XML / JSON** to import a single Sophos entity, an `<Entities>` wrapper containing entities in dependency order, or exported JSON with an `entities` array of XML strings. Use the XML tag and fields documented for the target firmware. Coverage follows the XML operations the firewall exposes. REST-only features, operations requiring attachment uploads or extra authentication fields, and full appliance backups are outside this workflow.

### Address lists and logged rules

**Group imported IP hosts** and **Group imported FQDNs** create groups after their members exist. The app separates IPv4, IPv6, and FQDN groups and assigns names such as `imported-ipv4-1` and `imported-fqdn-1`. Each type with two or more members is split into chunks of at most 256. This is an app compatibility cap based on a documented Sophos policy-matching issue, not a universal appliance maximum. IP list objects cannot be members of IP host groups. [Sophos SFOS 22.0 release notes](https://docs.sophos.com/releasenotes/output/en-us/nsg/sf_220_rn.html)

Mixed TXT files can contain addresses and domain names on separate lines, comments, and trailing `@tag` annotations. **Add FQDN wildcards for subdomains** opts into wildcard entries in addition to exact domains.

**Create or update by name** creates missing objects and updates existing names. **Create only** attempts to create every item and stops if a name already exists. **Update only** changes existing items and stops if a name is missing. For a new group with already-created hosts, use **Create or update by name**. To generate rules for both zone directions, turn on **Also create reverse-direction rules**. The app switches zones and the source/destination match together to keep the imported objects on the same side of the connection.

Choose **Single IPv4 address list** to create a Sophos `IPHost` of type `IPList` with the exact name entered. Refresh it using **Create or update by name**. A refresh replaces the membership and skips the write if the address set is unchanged. Choose **Individual hosts** for mixed inputs, ranges, CIDRs or FQDNs.

Choose **Allow** or **Drop** to generate firewall rules with traffic logging enabled. Specify both zones and select whether the imported objects match the source or destination. The app writes referenced objects first and places new Drop rules at the top and new Allow rules at the bottom. Rule evaluation is order-dependent. Rules start disabled unless **Enable the generated rule** is selected. Use a dedicated prefix for each list of rules. A scheduled refresh disables obsolete rules it previously managed when fewer rules are needed. Individual host objects and rules remain on the appliance when you delete a schedule.

The app enforces a maximum per IP-list object of 1,000 on an appliance reporting the SFOS 19.0 XML API and 800 otherwise. It also enforces the documented 1,000-member MAC-list limit and rejects plans above the published 16,000-host ceiling across host types. Split larger sources into separate named IPv4 lists. Other entity limits and remaining appliance capacity are enforced by the firewall. An appliance capacity error stops the operation and the Activity tab identifies prior confirmed writes. The app batches generated rules and separately limits input size as a resource safeguard.

### Scheduled imports

Use **Scheduled imports → New task** after filling in the form. Enter the task name and repeat interval, then save. The app reads the source again for each run and applies the saved import settings. The Windows Task Scheduler launches the same executable while the Windows account is signed in, even if the app window is closed. Keep the executable at the registered path. A sleeping or signed-out computer cannot fetch sources on time.

Set **First run (local time)** to choose when the repeat interval starts. **Run now** runs the saved task immediately. Saved source lists and start times use the same encrypted storage as the rest of the task.

The connection profile and scheduled-import data are stored under `%LOCALAPPDATA%\SophosFirewallObjectManager`. The `Jobs` subfolder contains the saved tasks and run history across app and computer restarts. Use the same Windows account to access this data.

A task uses a newer saved connection password and certificate fingerprint when the firewall address and username match. After a password rotation, save the new password once in the connection profile. A certificate renewed under a trusted Windows CA with the same hostname works automatically when using normal Windows validation. A changed private certificate needs a new verified pin before it can be used.

Long imports can keep running while the firewall responds. Individual requests still time out, and interactive runs can be canceled. After an interruption, preview the same source and prefix and use **Create or update by name** to skip matching hosts. Re-save tasks created in older builds to replace their previous Windows Task Scheduler time limit.

Load a task's settings to edit them, use **Run now** to run it on demand, or use **Pause / resume** to change whether it runs automatically. **Refresh** reads the recorded run history. Settings, credentials, and up to 20 run summaries with recent activity are encrypted for the current Windows user. Tasks fail without changing the target when an empty or invalid source prevents the plan from being generated. For recurring updates use **Create or update by name** and a stable object name. In multi-host mode, earlier objects remain available even if they fall out of the source.

## SFOS references

### SFOS 23.0 REST API

The firewall's **Administration → API access** page offers its firmware-specific OpenAPI download and REST API keys. The supplied `firewall_config_openapi.yaml` documents bearer authentication at `/api/firewall-config/v1`, paginated address exports, named IPv4 address lists, address groups, and IPv4 and IPv6 rule operations. The app currently uses the XML API. REST is a separate integration that can coexist with XML for older firmware.

For a REST integration, keep keys in the existing Windows-user encrypted storage and bind the selected API and credential to each saved task. Use the installed firmware's OpenAPI schema for request fields and retain the published Sophos appliance limits when a schema omits a maximum. REST uses the same HTTPS admin origin, so the certificate must still match that origin and chain to a trusted issuer.

[REST API reference](https://docs.sophos.com/nsg/sophos-firewall/rest-api/index.html) · [REST API access and keys](https://docs.sophos.com/nsg/sophos-firewall/23.0/Help/en-us/webhelp/onlinehelp/AdministratorHelp/Administration/APIRestAPI/APIAccessAndKeys/index.html)

### XML API by release family

Sophos publishes XML API documentation per release family. Use the release notes to find each maintenance release and build number. The selected family opens its documentation and does not force an API version in requests.

| SFOS | XML API | Builds and maintenance releases |
| --- | --- | --- |
| 19.0 | [Reference](https://docs.sophos.com/nsg/sophos-firewall/19.0/api/index.html) | GA 317, MR1 365, MR2 472, MR3 517. [Release notes](https://docs.sophos.com/releasenotes/output/en-us/nsg/sf_190_rn.html) |
| 19.5 | [Sophos XML API walkthrough](https://techvids.sophos.com/watch/7oEPqyoKz7ZSPn27Pze8hu). The old public schema link redirects to newer firmware. | GA 197, MR1 278, MR2 624, MR3 652, MR4 718. [Release notes](https://docs.sophos.com/releasenotes/output/en-us/nsg/sf_195_rn.html) |
| 20.0 | [Reference](https://docs.sophos.com/nsg/sophos-firewall/20.0/api/index.html) | GA 222, MR1 342, MR2 378, MR3 427. [Release notes](https://docs.sophos.com/releasenotes/output/en-us/nsg/sf_200_rn.html) |
| 21.0 | [Reference](https://docs.sophos.com/nsg/sophos-firewall/21.0/api/index.html) | GA 169, MR1 237/272/277, MR2 349. [Release notes](https://docs.sophos.com/releasenotes/output/en-us/nsg/sf_210_rn.html) |
| 21.5 | [Reference](https://docs.sophos.com/nsg/sophos-firewall/21.5/api/index.html) | GA 171, MR1 261, MR2 323. [Release notes](https://docs.sophos.com/releasenotes/output/en-us/nsg/sf_215_rn.html) |
| 22.0 | [Reference](https://docs.sophos.com/nsg/sophos-firewall/22.0/api/index.html) | GA 365/411, MR1 490, MR2 546, MR3 576. [Release notes](https://docs.sophos.com/releasenotes/output/en-us/nsg/sf_220_rn.html) |
| 23.0 | [Reference](https://docs.sophos.com/nsg/sophos-firewall/23.0/api/index.html) | GA 371. [Release notes](https://docs.sophos.com/releasenotes/output/en-us/nsg/sf_230_rn.html) |

The app uses the shared XML API from these firmware families. Validate entity support, host selectors, and firewall rules on a test appliance before using them in production.

