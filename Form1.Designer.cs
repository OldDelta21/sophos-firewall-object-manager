namespace SOPHOS_Host___Addresses_Manager;

partial class Form1
{
    private System.ComponentModel.IContainer components = null!;
    private NotifyIcon trayIcon = null!;
    private ContextMenuStrip trayMenu = null!;
    private ToolStripMenuItem trayOpenItem = null!;
    private ToolStripMenuItem trayExitItem = null!;
    private Button saveConnectionButton = null!;
    private Button forgetConnectionButton = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing) components?.Dispose();
        base.Dispose(disposing);
    }

    private Panel banner = null!;
    private Label title = null!;
    private GroupBox connection = null!;
    private Label endpointLabel = null!;
    private TextBox endpointBox = null!;
    private Label userLabel = null!;
    private TextBox userBox = null!;
    private Label passwordLabel = null!;
    private TextBox passwordBox = null!;
    private Label familyLabel = null!;
    private ComboBox familyBox = null!;
    private Label versionLabel = null!;
    private TextBox versionBox = null!;
    private Label fingerprintLabel = null!;
    private TextBox fingerprintBox = null!;
    private Button connectButton = null!;
    private Button docsButton = null!;
    private GroupBox transfer = null!;
    private Label sourceLabel = null!;
    private ComboBox sourceBox = null!;
    private Label pathLabel = null!;
    private TextBox pathBox = null!;
    private Button browseButton = null!;
    private Label modeLabel = null!;
    private ComboBox modeBox = null!;
    private Label addressKindLabel = null!;
    private ComboBox addressKindBox = null!;
    private Label prefixLabel = null!;
    private TextBox prefixBox = null!;
    private Label operationLabel = null!;
    private ComboBox operationBox = null!;
    private Label exportLabel = null!;
    private ComboBox entityBox = null!;
    private Button exportButton = null!;
    private Button previewButton = null!;
    private Button applyButton = null!;
    private GroupBox blacklist = null!;
    private Label actionLabel = null!;
    private ComboBox actionBox = null!;
    private Label directionLabel = null!;
    private ComboBox directionBox = null!;
    private Label sourceZoneLabel = null!;
    private TextBox sourceZoneBox = null!;
    private Label destinationZoneLabel = null!;
    private TextBox destinationZoneBox = null!;
    private CheckBox enableBox = null!;
    private CheckBox groupIpsBox = null!;
    private CheckBox groupFqdnsBox = null!;
    private CheckBox wildcardsBox = null!;
    private CheckBox reverseRuleBox = null!;
    private TabControl tabs = null!;
    private TabPage inputPage = null!;
    private TabPage sourcesPage = null!;
    private ListBox sourceListBox = null!;
    private Label sourcesHelpLabel = null!;
    private ComboBox templateBox = null!;
    private Button addTemplateButton = null!;
    private Button clearSourcesButton = null!;
    private TextBox sourceNameBox = null!;
    private NumericUpDown sourceLimitBox = null!;
    private Label sourceLimitLabel = null!;
    private Button applySourceOptionsButton = null!;
    private Button addSourceButton = null!;
    private Button removeSourceButton = null!;
    private TextBox inputBox = null!;
    private TabPage previewPage = null!;
    private TextBox previewBox = null!;
    private TabPage activityPage = null!;
    private TextBox activityBox = null!;
    private TabPage schedulePage = null!;
    private Label taskListLabel = null!;
    private ListBox taskListBox = null!;
    private Label taskNameLabel = null!;
    private TextBox taskNameBox = null!;
    private Label intervalLabel = null!;
    private NumericUpDown intervalBox = null!;
    private Label startTimeLabel = null!;
    private DateTimePicker startTimeBox = null!;
    private CheckBox taskEnabledBox = null!;
    private TextBox taskDetailsBox = null!;
    private Button newTaskButton = null!;
    private Button deleteTaskButton = null!;
    private Button refreshTaskButton = null!;
    private Button loadTaskButton = null!;
    private Button runTaskButton = null!;
    private Button saveTaskButton = null!;
    private Button toggleTaskButton = null!;
    private ProgressBar progressBar = null!;
    private Button cancelButton = null!;
    private Label statusLabel = null!;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
        banner = new Panel();
        title = new Label();
        saveConnectionButton = new Button();
        forgetConnectionButton = new Button();
        connection = new GroupBox();
        endpointLabel = new Label();
        endpointBox = new TextBox();
        userLabel = new Label();
        userBox = new TextBox();
        passwordLabel = new Label();
        passwordBox = new TextBox();
        familyLabel = new Label();
        familyBox = new ComboBox();
        versionLabel = new Label();
        versionBox = new TextBox();
        connectButton = new Button();
        docsButton = new Button();
        fingerprintLabel = new Label();
        fingerprintBox = new TextBox();
        transfer = new GroupBox();
        sourceLabel = new Label();
        sourceBox = new ComboBox();
        pathLabel = new Label();
        pathBox = new TextBox();
        browseButton = new Button();
        addSourceButton = new Button();
        modeLabel = new Label();
        modeBox = new ComboBox();
        addressKindLabel = new Label();
        addressKindBox = new ComboBox();
        prefixLabel = new Label();
        prefixBox = new TextBox();
        operationLabel = new Label();
        operationBox = new ComboBox();
        exportLabel = new Label();
        entityBox = new ComboBox();
        exportButton = new Button();
        previewButton = new Button();
        applyButton = new Button();
        groupIpsBox = new CheckBox();
        groupFqdnsBox = new CheckBox();
        wildcardsBox = new CheckBox();
        blacklist = new GroupBox();
        actionLabel = new Label();
        actionBox = new ComboBox();
        directionLabel = new Label();
        directionBox = new ComboBox();
        sourceZoneLabel = new Label();
        sourceZoneBox = new TextBox();
        destinationZoneLabel = new Label();
        destinationZoneBox = new TextBox();
        enableBox = new CheckBox();
        reverseRuleBox = new CheckBox();
        tabs = new TabControl();
        inputPage = new TabPage();
        inputBox = new TextBox();
        sourcesPage = new TabPage();
        sourcesHelpLabel = new Label();
        sourceListBox = new ListBox();
        removeSourceButton = new Button();
        templateBox = new ComboBox();
        addTemplateButton = new Button();
        clearSourcesButton = new Button();
        sourceNameBox = new TextBox();
        sourceLimitLabel = new Label();
        sourceLimitBox = new NumericUpDown();
        applySourceOptionsButton = new Button();
        previewPage = new TabPage();
        previewBox = new TextBox();
        activityPage = new TabPage();
        activityBox = new TextBox();
        schedulePage = new TabPage();
        taskListLabel = new Label();
        taskListBox = new ListBox();
        taskNameLabel = new Label();
        taskNameBox = new TextBox();
        intervalLabel = new Label();
        intervalBox = new NumericUpDown();
        startTimeLabel = new Label();
        startTimeBox = new DateTimePicker();
        taskEnabledBox = new CheckBox();
        taskDetailsBox = new TextBox();
        newTaskButton = new Button();
        deleteTaskButton = new Button();
        refreshTaskButton = new Button();
        loadTaskButton = new Button();
        runTaskButton = new Button();
        saveTaskButton = new Button();
        toggleTaskButton = new Button();
        progressBar = new ProgressBar();
        cancelButton = new Button();
        statusLabel = new Label();
        trayMenu = new ContextMenuStrip(components);
        trayOpenItem = new ToolStripMenuItem();
        trayExitItem = new ToolStripMenuItem();
        trayIcon = new NotifyIcon(components);
        banner.SuspendLayout();
        connection.SuspendLayout();
        transfer.SuspendLayout();
        blacklist.SuspendLayout();
        tabs.SuspendLayout();
        inputPage.SuspendLayout();
        sourcesPage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)sourceLimitBox).BeginInit();
        previewPage.SuspendLayout();
        activityPage.SuspendLayout();
        schedulePage.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)intervalBox).BeginInit();
        trayMenu.SuspendLayout();
        SuspendLayout();
        // 
        // banner
        // 
        banner.BackColor = Color.FromArgb(0, 51, 102);
        banner.Controls.Add(title);
        banner.Controls.Add(saveConnectionButton);
        banner.Controls.Add(forgetConnectionButton);
        banner.Dock = DockStyle.Top;
        banner.Location = new Point(0, 0);
        banner.Name = "banner";
        banner.Size = new Size(1120, 57);
        banner.TabIndex = 1;
        // 
        // title
        // 
        title.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
        title.ForeColor = Color.White;
        title.Location = new Point(20, 10);
        title.Name = "title";
        title.Size = new Size(650, 39);
        title.TabIndex = 0;
        title.Text = "Sophos Firewall Object Manager";
        // 
        // saveConnectionButton
        // 
        saveConnectionButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        saveConnectionButton.ForeColor = Color.White;
        saveConnectionButton.Location = new Point(800, 12);
        saveConnectionButton.Name = "saveConnectionButton";
        saveConnectionButton.Size = new Size(145, 33);
        saveConnectionButton.TabIndex = 1;
        saveConnectionButton.Text = "Save connection";
        saveConnectionButton.Click += SaveConnectionClick;
        // 
        // forgetConnectionButton
        // 
        forgetConnectionButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        forgetConnectionButton.ForeColor = Color.White;
        forgetConnectionButton.Location = new Point(956, 12);
        forgetConnectionButton.Name = "forgetConnectionButton";
        forgetConnectionButton.Size = new Size(144, 33);
        forgetConnectionButton.TabIndex = 2;
        forgetConnectionButton.Text = "Forget connection";
        forgetConnectionButton.Click += ForgetConnectionClick;
        // 
        // connection
        // 
        connection.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        connection.Controls.Add(endpointLabel);
        connection.Controls.Add(endpointBox);
        connection.Controls.Add(userLabel);
        connection.Controls.Add(userBox);
        connection.Controls.Add(passwordLabel);
        connection.Controls.Add(passwordBox);
        connection.Controls.Add(familyLabel);
        connection.Controls.Add(familyBox);
        connection.Controls.Add(versionLabel);
        connection.Controls.Add(versionBox);
        connection.Controls.Add(connectButton);
        connection.Controls.Add(docsButton);
        connection.Controls.Add(fingerprintLabel);
        connection.Controls.Add(fingerprintBox);
        connection.Location = new Point(20, 66);
        connection.Name = "connection";
        connection.Size = new Size(1080, 121);
        connection.TabIndex = 2;
        connection.TabStop = false;
        connection.Text = "Firewall connection";
        // 
        // endpointLabel
        // 
        endpointLabel.Location = new Point(14, 22);
        endpointLabel.Name = "endpointLabel";
        endpointLabel.Size = new Size(250, 23);
        endpointLabel.TabIndex = 0;
        endpointLabel.Text = "HTTPS admin origin";
        // 
        // endpointBox
        // 
        endpointBox.Location = new Point(14, 45);
        endpointBox.Name = "endpointBox";
        endpointBox.PlaceholderText = "https://firewall.example:4444";
        endpointBox.Size = new Size(285, 23);
        endpointBox.TabIndex = 1;
        // 
        // userLabel
        // 
        userLabel.Location = new Point(312, 22);
        userLabel.Name = "userLabel";
        userLabel.Size = new Size(140, 23);
        userLabel.TabIndex = 2;
        userLabel.Text = "Username";
        // 
        // userBox
        // 
        userBox.Location = new Point(312, 45);
        userBox.Name = "userBox";
        userBox.Size = new Size(142, 23);
        userBox.TabIndex = 3;
        // 
        // passwordLabel
        // 
        passwordLabel.Location = new Point(468, 22);
        passwordLabel.Name = "passwordLabel";
        passwordLabel.Size = new Size(151, 23);
        passwordLabel.TabIndex = 4;
        passwordLabel.Text = "Password";
        // 
        // passwordBox
        // 
        passwordBox.Location = new Point(468, 45);
        passwordBox.Name = "passwordBox";
        passwordBox.Size = new Size(152, 23);
        passwordBox.TabIndex = 5;
        passwordBox.UseSystemPasswordChar = true;
        // 
        // familyLabel
        // 
        familyLabel.Location = new Point(634, 22);
        familyLabel.Name = "familyLabel";
        familyLabel.Size = new Size(97, 23);
        familyLabel.TabIndex = 6;
        familyLabel.Text = "SFOS docs";
        // 
        // familyBox
        // 
        familyBox.Items.AddRange(new object[] { "23.0", "22.0", "21.5", "21.0", "20.0", "19.5", "19.0" });
        familyBox.Location = new Point(634, 45);
        familyBox.Name = "familyBox";
        familyBox.Size = new Size(95, 23);
        familyBox.TabIndex = 7;
        // 
        // versionLabel
        // 
        versionLabel.Location = new Point(743, 22);
        versionLabel.Name = "versionLabel";
        versionLabel.Size = new Size(133, 23);
        versionLabel.TabIndex = 8;
        versionLabel.Text = "XML API override";
        // 
        // versionBox
        // 
        versionBox.Location = new Point(743, 45);
        versionBox.Name = "versionBox";
        versionBox.PlaceholderText = "Optional";
        versionBox.Size = new Size(127, 23);
        versionBox.TabIndex = 9;
        // 
        // connectButton
        // 
        connectButton.BackColor = Color.White;
        connectButton.FlatStyle = FlatStyle.Flat;
        connectButton.ForeColor = Color.FromArgb(0, 51, 102);
        connectButton.Location = new Point(887, 41);
        connectButton.Name = "connectButton";
        connectButton.Size = new Size(120, 33);
        connectButton.TabIndex = 10;
        connectButton.Text = "Test connection";
        connectButton.UseVisualStyleBackColor = false;
        connectButton.Click += ConnectClick;
        // 
        // docsButton
        // 
        docsButton.BackColor = Color.White;
        docsButton.FlatStyle = FlatStyle.Flat;
        docsButton.ForeColor = Color.FromArgb(0, 51, 102);
        docsButton.Location = new Point(1015, 41);
        docsButton.Name = "docsButton";
        docsButton.Size = new Size(53, 33);
        docsButton.TabIndex = 11;
        docsButton.Text = "Docs";
        docsButton.UseVisualStyleBackColor = false;
        docsButton.Click += DocsClick;
        // 
        // fingerprintLabel
        // 
        fingerprintLabel.Location = new Point(14, 87);
        fingerprintLabel.Name = "fingerprintLabel";
        fingerprintLabel.Size = new Size(175, 23);
        fingerprintLabel.TabIndex = 12;
        fingerprintLabel.Text = "Admin certificate SHA-256";
        // 
        // fingerprintBox
        // 
        fingerprintBox.AccessibleName = "Admin certificate SHA-256 fingerprint";
        fingerprintBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        fingerprintBox.Location = new Point(194, 83);
        fingerprintBox.Name = "fingerprintBox";
        fingerprintBox.PlaceholderText = "Optional. Use only a fingerprint verified from a trusted certificate export.";
        fingerprintBox.Size = new Size(874, 23);
        fingerprintBox.TabIndex = 13;
        // 
        // transfer
        // 
        transfer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        transfer.Controls.Add(sourceLabel);
        transfer.Controls.Add(sourceBox);
        transfer.Controls.Add(pathLabel);
        transfer.Controls.Add(pathBox);
        transfer.Controls.Add(browseButton);
        transfer.Controls.Add(addSourceButton);
        transfer.Controls.Add(modeLabel);
        transfer.Controls.Add(modeBox);
        transfer.Controls.Add(addressKindLabel);
        transfer.Controls.Add(addressKindBox);
        transfer.Controls.Add(prefixLabel);
        transfer.Controls.Add(prefixBox);
        transfer.Controls.Add(operationLabel);
        transfer.Controls.Add(operationBox);
        transfer.Controls.Add(exportLabel);
        transfer.Controls.Add(entityBox);
        transfer.Controls.Add(exportButton);
        transfer.Controls.Add(previewButton);
        transfer.Controls.Add(applyButton);
        transfer.Controls.Add(groupIpsBox);
        transfer.Controls.Add(groupFqdnsBox);
        transfer.Controls.Add(wildcardsBox);
        transfer.FlatStyle = FlatStyle.Flat;
        transfer.Location = new Point(20, 195);
        transfer.Name = "transfer";
        transfer.Size = new Size(1080, 220);
        transfer.TabIndex = 3;
        transfer.TabStop = false;
        transfer.Text = "Import and export";
        // 
        // sourceLabel
        // 
        sourceLabel.Location = new Point(14, 21);
        sourceLabel.Name = "sourceLabel";
        sourceLabel.Size = new Size(162, 23);
        sourceLabel.TabIndex = 0;
        sourceLabel.Text = "Input source";
        // 
        // sourceBox
        // 
        sourceBox.DropDownStyle = ComboBoxStyle.DropDownList;
        sourceBox.Items.AddRange(new object[] { "File", "HTTPS URL", "Paste" });
        sourceBox.Location = new Point(14, 43);
        sourceBox.Name = "sourceBox";
        sourceBox.Size = new Size(163, 23);
        sourceBox.TabIndex = 1;
        // 
        // pathLabel
        // 
        pathLabel.Location = new Point(191, 21);
        pathLabel.Name = "pathLabel";
        pathLabel.Size = new Size(400, 23);
        pathLabel.TabIndex = 2;
        pathLabel.Text = "File path or direct HTTPS URL";
        // 
        // pathBox
        // 
        pathBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pathBox.Location = new Point(191, 43);
        pathBox.Name = "pathBox";
        pathBox.Size = new Size(763, 23);
        pathBox.TabIndex = 3;
        // 
        // browseButton
        // 
        browseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        browseButton.BackColor = Color.White;
        browseButton.FlatStyle = FlatStyle.Flat;
        browseButton.ForeColor = Color.FromArgb(0, 51, 102);
        browseButton.Location = new Point(966, 39);
        browseButton.Name = "browseButton";
        browseButton.Size = new Size(101, 33);
        browseButton.TabIndex = 4;
        browseButton.Text = "Browse…";
        browseButton.UseVisualStyleBackColor = false;
        browseButton.Click += BrowseClick;
        // 
        // addSourceButton
        // 
        addSourceButton.Location = new Point(762, 177);
        addSourceButton.Name = "addSourceButton";
        addSourceButton.Size = new Size(139, 31);
        addSourceButton.TabIndex = 5;
        addSourceButton.Text = "Add to source list";
        addSourceButton.Click += AddSourceClick;
        // 
        // modeLabel
        // 
        modeLabel.Location = new Point(14, 78);
        modeLabel.Name = "modeLabel";
        modeLabel.Size = new Size(162, 23);
        modeLabel.TabIndex = 6;
        modeLabel.Text = "File format";
        // 
        // modeBox
        // 
        modeBox.DropDownStyle = ComboBoxStyle.DropDownList;
        modeBox.Items.AddRange(new object[] { "TXT", "CSV", "JSON", "Entity XML / JSON" });
        modeBox.Location = new Point(14, 101);
        modeBox.Name = "modeBox";
        modeBox.Size = new Size(163, 23);
        modeBox.TabIndex = 7;
        // 
        // addressKindLabel
        // 
        addressKindLabel.Location = new Point(191, 78);
        addressKindLabel.Name = "addressKindLabel";
        addressKindLabel.Size = new Size(235, 23);
        addressKindLabel.TabIndex = 8;
        addressKindLabel.Text = "Create address objects as";
        // 
        // addressKindBox
        // 
        addressKindBox.DropDownStyle = ComboBoxStyle.DropDownList;
        addressKindBox.Items.AddRange(new object[] { "Individual hosts", "Single IPv4 address list" });
        addressKindBox.Location = new Point(191, 101);
        addressKindBox.Name = "addressKindBox";
        addressKindBox.Size = new Size(234, 23);
        addressKindBox.TabIndex = 9;
        // 
        // prefixLabel
        // 
        prefixLabel.Location = new Point(439, 78);
        prefixLabel.Name = "prefixLabel";
        prefixLabel.Size = new Size(218, 23);
        prefixLabel.TabIndex = 10;
        prefixLabel.Text = "List name / object prefix";
        // 
        // prefixBox
        // 
        prefixBox.Location = new Point(439, 101);
        prefixBox.Name = "prefixBox";
        prefixBox.Size = new Size(220, 23);
        prefixBox.TabIndex = 11;
        prefixBox.Text = "imported";
        // 
        // operationLabel
        // 
        operationLabel.Location = new Point(671, 78);
        operationLabel.Name = "operationLabel";
        operationLabel.Size = new Size(177, 23);
        operationLabel.TabIndex = 12;
        operationLabel.Text = "Write mode";
        // 
        // operationBox
        // 
        operationBox.DropDownStyle = ComboBoxStyle.DropDownList;
        operationBox.Items.AddRange(new object[] { "Create or update by name", "Create only", "Update only" });
        operationBox.Location = new Point(671, 101);
        operationBox.Name = "operationBox";
        operationBox.Size = new Size(245, 23);
        operationBox.TabIndex = 13;
        // 
        // exportLabel
        // 
        exportLabel.Location = new Point(14, 145);
        exportLabel.Name = "exportLabel";
        exportLabel.Size = new Size(127, 23);
        exportLabel.TabIndex = 14;
        exportLabel.Text = "Export entity tag";
        // 
        // entityBox
        // 
        entityBox.Items.AddRange(new object[] { "IPHost", "IPHostGroup", "FQDNHost", "FQDNHostGroup", "MACHost", "Service", "ServiceGroup", "FirewallRule", "Zone" });
        entityBox.Location = new Point(145, 141);
        entityBox.Name = "entityBox";
        entityBox.Size = new Size(230, 23);
        entityBox.TabIndex = 15;
        // 
        // exportButton
        // 
        exportButton.BackColor = Color.White;
        exportButton.FlatStyle = FlatStyle.Flat;
        exportButton.ForeColor = Color.FromArgb(0, 51, 102);
        exportButton.Location = new Point(386, 138);
        exportButton.Name = "exportButton";
        exportButton.Size = new Size(101, 33);
        exportButton.TabIndex = 16;
        exportButton.Text = "Export…";
        exportButton.UseVisualStyleBackColor = false;
        exportButton.Click += ExportClick;
        // 
        // previewButton
        // 
        previewButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        previewButton.BackColor = Color.White;
        previewButton.FlatStyle = FlatStyle.Flat;
        previewButton.ForeColor = Color.FromArgb(0, 51, 102);
        previewButton.Location = new Point(762, 138);
        previewButton.Name = "previewButton";
        previewButton.Size = new Size(139, 33);
        previewButton.TabIndex = 17;
        previewButton.Text = "Preview import";
        previewButton.UseVisualStyleBackColor = false;
        previewButton.Click += PreviewClick;
        // 
        // applyButton
        // 
        applyButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        applyButton.BackColor = Color.FromArgb(0, 85, 170);
        applyButton.Enabled = false;
        applyButton.FlatStyle = FlatStyle.Popup;
        applyButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
        applyButton.ForeColor = Color.White;
        applyButton.Location = new Point(913, 138);
        applyButton.Name = "applyButton";
        applyButton.Size = new Size(154, 70);
        applyButton.TabIndex = 18;
        applyButton.Text = "Apply preview";
        applyButton.UseVisualStyleBackColor = false;
        applyButton.Click += ApplyClick;
        // 
        // groupIpsBox
        // 
        groupIpsBox.Checked = true;
        groupIpsBox.CheckState = CheckState.Checked;
        groupIpsBox.Location = new Point(14, 178);
        groupIpsBox.Name = "groupIpsBox";
        groupIpsBox.Size = new Size(205, 24);
        groupIpsBox.TabIndex = 19;
        groupIpsBox.Text = "Group imported IP hosts";
        // 
        // groupFqdnsBox
        // 
        groupFqdnsBox.Checked = true;
        groupFqdnsBox.CheckState = CheckState.Checked;
        groupFqdnsBox.Location = new Point(225, 178);
        groupFqdnsBox.Name = "groupFqdnsBox";
        groupFqdnsBox.Size = new Size(205, 24);
        groupFqdnsBox.TabIndex = 20;
        groupFqdnsBox.Text = "Group imported FQDNs";
        // 
        // wildcardsBox
        // 
        wildcardsBox.Location = new Point(438, 178);
        wildcardsBox.Name = "wildcardsBox";
        wildcardsBox.Size = new Size(310, 24);
        wildcardsBox.TabIndex = 21;
        wildcardsBox.Text = "Add FQDN wildcards for subdomains";
        // 
        // blacklist
        // 
        blacklist.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        blacklist.Controls.Add(actionLabel);
        blacklist.Controls.Add(actionBox);
        blacklist.Controls.Add(directionLabel);
        blacklist.Controls.Add(directionBox);
        blacklist.Controls.Add(sourceZoneLabel);
        blacklist.Controls.Add(sourceZoneBox);
        blacklist.Controls.Add(destinationZoneLabel);
        blacklist.Controls.Add(destinationZoneBox);
        blacklist.Controls.Add(enableBox);
        blacklist.Controls.Add(reverseRuleBox);
        blacklist.Location = new Point(20, 423);
        blacklist.Name = "blacklist";
        blacklist.Size = new Size(1080, 86);
        blacklist.TabIndex = 4;
        blacklist.TabStop = false;
        blacklist.Text = "Logged firewall rule";
        // 
        // actionLabel
        // 
        actionLabel.Location = new Point(14, 22);
        actionLabel.Name = "actionLabel";
        actionLabel.Size = new Size(155, 23);
        actionLabel.TabIndex = 0;
        actionLabel.Text = "Traffic action";
        // 
        // actionBox
        // 
        actionBox.DropDownStyle = ComboBoxStyle.DropDownList;
        actionBox.Items.AddRange(new object[] { "None", "Allow", "Drop" });
        actionBox.Location = new Point(14, 44);
        actionBox.Name = "actionBox";
        actionBox.Size = new Size(152, 23);
        actionBox.TabIndex = 1;
        // 
        // directionLabel
        // 
        directionLabel.Location = new Point(180, 22);
        directionLabel.Name = "directionLabel";
        directionLabel.Size = new Size(190, 23);
        directionLabel.TabIndex = 2;
        directionLabel.Text = "Match imported objects as";
        // 
        // directionBox
        // 
        directionBox.DropDownStyle = ComboBoxStyle.DropDownList;
        directionBox.Items.AddRange(new object[] { "Destination", "Source" });
        directionBox.Location = new Point(180, 44);
        directionBox.Name = "directionBox";
        directionBox.Size = new Size(190, 23);
        directionBox.TabIndex = 3;
        // 
        // sourceZoneLabel
        // 
        sourceZoneLabel.Location = new Point(384, 22);
        sourceZoneLabel.Name = "sourceZoneLabel";
        sourceZoneLabel.Size = new Size(190, 23);
        sourceZoneLabel.TabIndex = 4;
        sourceZoneLabel.Text = "Source zone";
        // 
        // sourceZoneBox
        // 
        sourceZoneBox.Location = new Point(384, 44);
        sourceZoneBox.Name = "sourceZoneBox";
        sourceZoneBox.Size = new Size(187, 23);
        sourceZoneBox.TabIndex = 5;
        sourceZoneBox.Text = "LAN";
        // 
        // destinationZoneLabel
        // 
        destinationZoneLabel.Location = new Point(585, 22);
        destinationZoneLabel.Name = "destinationZoneLabel";
        destinationZoneLabel.Size = new Size(190, 23);
        destinationZoneLabel.TabIndex = 6;
        destinationZoneLabel.Text = "Destination zone";
        // 
        // destinationZoneBox
        // 
        destinationZoneBox.Location = new Point(585, 44);
        destinationZoneBox.Name = "destinationZoneBox";
        destinationZoneBox.Size = new Size(187, 23);
        destinationZoneBox.TabIndex = 7;
        destinationZoneBox.Text = "WAN";
        // 
        // enableBox
        // 
        enableBox.Location = new Point(788, 44);
        enableBox.Name = "enableBox";
        enableBox.Size = new Size(264, 28);
        enableBox.TabIndex = 8;
        enableBox.Text = "Enable the generated rule";
        // 
        // reverseRuleBox
        // 
        reverseRuleBox.Location = new Point(788, 19);
        reverseRuleBox.Name = "reverseRuleBox";
        reverseRuleBox.Size = new Size(280, 24);
        reverseRuleBox.TabIndex = 9;
        reverseRuleBox.Text = "Also create reverse-direction rules";
        // 
        // tabs
        // 
        tabs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        tabs.Controls.Add(inputPage);
        tabs.Controls.Add(sourcesPage);
        tabs.Controls.Add(previewPage);
        tabs.Controls.Add(activityPage);
        tabs.Controls.Add(schedulePage);
        tabs.Location = new Point(20, 518);
        tabs.Name = "tabs";
        tabs.SelectedIndex = 0;
        tabs.Size = new Size(1080, 304);
        tabs.TabIndex = 5;
        // 
        // inputPage
        // 
        inputPage.Controls.Add(inputBox);
        inputPage.Location = new Point(4, 24);
        inputPage.Name = "inputPage";
        inputPage.Size = new Size(1072, 276);
        inputPage.TabIndex = 0;
        inputPage.Text = "Paste input";
        inputPage.UseVisualStyleBackColor = true;
        // 
        // inputBox
        // 
        inputBox.BackColor = Color.White;
        inputBox.Dock = DockStyle.Fill;
        inputBox.Font = new Font("Consolas", 11F);
        inputBox.Location = new Point(0, 0);
        inputBox.Multiline = true;
        inputBox.Name = "inputBox";
        inputBox.ScrollBars = ScrollBars.Both;
        inputBox.Size = new Size(1072, 276);
        inputBox.TabIndex = 0;
        inputBox.WordWrap = false;
        // 
        // sourcesPage
        // 
        sourcesPage.Controls.Add(sourcesHelpLabel);
        sourcesPage.Controls.Add(sourceListBox);
        sourcesPage.Controls.Add(removeSourceButton);
        sourcesPage.Controls.Add(templateBox);
        sourcesPage.Controls.Add(addTemplateButton);
        sourcesPage.Controls.Add(clearSourcesButton);
        sourcesPage.Controls.Add(sourceNameBox);
        sourcesPage.Controls.Add(sourceLimitLabel);
        sourcesPage.Controls.Add(sourceLimitBox);
        sourcesPage.Controls.Add(applySourceOptionsButton);
        sourcesPage.Location = new Point(4, 24);
        sourcesPage.Name = "sourcesPage";
        sourcesPage.Size = new Size(1072, 276);
        sourcesPage.TabIndex = 1;
        sourcesPage.Text = "Import sources (0)";
        sourcesPage.UseVisualStyleBackColor = true;
        // 
        // sourcesHelpLabel
        // 
        sourcesHelpLabel.Location = new Point(12, 46);
        sourcesHelpLabel.Name = "sourcesHelpLabel";
        sourcesHelpLabel.Size = new Size(820, 23);
        sourcesHelpLabel.TabIndex = 0;
        sourcesHelpLabel.Text = "Choose a source to edit its name or entry limit. An empty list uses the current input fields.";
        // 
        // sourceListBox
        // 
        sourceListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        sourceListBox.HorizontalScrollbar = true;
        sourceListBox.IntegralHeight = false;
        sourceListBox.ItemHeight = 15;
        sourceListBox.Location = new Point(12, 70);
        sourceListBox.Name = "sourceListBox";
        sourceListBox.SelectionMode = SelectionMode.MultiExtended;
        sourceListBox.Size = new Size(1041, 146);
        sourceListBox.TabIndex = 1;
        sourceListBox.SelectedIndexChanged += UpdateSourceEditor;
        // 
        // removeSourceButton
        // 
        removeSourceButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        removeSourceButton.Location = new Point(12, 230);
        removeSourceButton.Name = "removeSourceButton";
        removeSourceButton.Size = new Size(150, 33);
        removeSourceButton.TabIndex = 2;
        removeSourceButton.Text = "Remove selected";
        removeSourceButton.Click += RemoveSourceClick;
        // 
        // templateBox
        // 
        templateBox.AccessibleName = "Feed template";
        templateBox.DropDownStyle = ComboBoxStyle.DropDownList;
        templateBox.Location = new Point(12, 12);
        templateBox.Name = "templateBox";
        templateBox.Size = new Size(375, 23);
        templateBox.TabIndex = 3;
        // 
        // addTemplateButton
        // 
        addTemplateButton.Location = new Point(397, 10);
        addTemplateButton.Name = "addTemplateButton";
        addTemplateButton.Size = new Size(155, 28);
        addTemplateButton.TabIndex = 4;
        addTemplateButton.Text = "Add feed template";
        addTemplateButton.Click += AddTemplateClick;
        // 
        // clearSourcesButton
        // 
        clearSourcesButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        clearSourcesButton.Location = new Point(174, 229);
        clearSourcesButton.Name = "clearSourcesButton";
        clearSourcesButton.Size = new Size(121, 34);
        clearSourcesButton.TabIndex = 5;
        clearSourcesButton.Text = "Clear sources";
        clearSourcesButton.Click += ClearSourcesClick;
        // 
        // sourceNameBox
        // 
        sourceNameBox.AccessibleName = "Selected source list name";
        sourceNameBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        sourceNameBox.Location = new Point(317, 235);
        sourceNameBox.MaxLength = 30;
        sourceNameBox.Name = "sourceNameBox";
        sourceNameBox.PlaceholderText = "Source list name";
        sourceNameBox.Size = new Size(197, 23);
        sourceNameBox.TabIndex = 6;
        // 
        // sourceLimitLabel
        // 
        sourceLimitLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        sourceLimitLabel.Location = new Point(526, 238);
        sourceLimitLabel.Name = "sourceLimitLabel";
        sourceLimitLabel.Size = new Size(147, 23);
        sourceLimitLabel.TabIndex = 7;
        sourceLimitLabel.Text = "First N entries (0 = all)";
        // 
        // sourceLimitBox
        // 
        sourceLimitBox.AccessibleName = "Selected source entry limit";
        sourceLimitBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        sourceLimitBox.Location = new Point(675, 235);
        sourceLimitBox.Maximum = new decimal(new int[] { 16000, 0, 0, 0 });
        sourceLimitBox.Name = "sourceLimitBox";
        sourceLimitBox.Size = new Size(92, 23);
        sourceLimitBox.TabIndex = 8;
        // 
        // applySourceOptionsButton
        // 
        applySourceOptionsButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        applySourceOptionsButton.Location = new Point(778, 229);
        applySourceOptionsButton.Name = "applySourceOptionsButton";
        applySourceOptionsButton.Size = new Size(184, 34);
        applySourceOptionsButton.TabIndex = 9;
        applySourceOptionsButton.Text = "Update selected source";
        applySourceOptionsButton.Click += ApplySourceOptionsClick;
        // 
        // previewPage
        // 
        previewPage.Controls.Add(previewBox);
        previewPage.Location = new Point(4, 24);
        previewPage.Name = "previewPage";
        previewPage.Size = new Size(1072, 276);
        previewPage.TabIndex = 2;
        previewPage.Text = "Preview";
        previewPage.UseVisualStyleBackColor = true;
        // 
        // previewBox
        // 
        previewBox.BackColor = Color.White;
        previewBox.Dock = DockStyle.Fill;
        previewBox.Font = new Font("Consolas", 11F);
        previewBox.Location = new Point(0, 0);
        previewBox.Multiline = true;
        previewBox.Name = "previewBox";
        previewBox.ReadOnly = true;
        previewBox.ScrollBars = ScrollBars.Both;
        previewBox.Size = new Size(1072, 276);
        previewBox.TabIndex = 0;
        previewBox.WordWrap = false;
        // 
        // activityPage
        // 
        activityPage.Controls.Add(activityBox);
        activityPage.Location = new Point(4, 24);
        activityPage.Name = "activityPage";
        activityPage.Size = new Size(1072, 276);
        activityPage.TabIndex = 3;
        activityPage.Text = "Activity";
        activityPage.UseVisualStyleBackColor = true;
        // 
        // activityBox
        // 
        activityBox.BackColor = Color.White;
        activityBox.Dock = DockStyle.Fill;
        activityBox.Font = new Font("Consolas", 11F);
        activityBox.Location = new Point(0, 0);
        activityBox.Multiline = true;
        activityBox.Name = "activityBox";
        activityBox.ReadOnly = true;
        activityBox.ScrollBars = ScrollBars.Both;
        activityBox.Size = new Size(1072, 276);
        activityBox.TabIndex = 0;
        activityBox.WordWrap = false;
        // 
        // schedulePage
        // 
        schedulePage.Controls.Add(taskListLabel);
        schedulePage.Controls.Add(taskListBox);
        schedulePage.Controls.Add(taskNameLabel);
        schedulePage.Controls.Add(taskNameBox);
        schedulePage.Controls.Add(intervalLabel);
        schedulePage.Controls.Add(intervalBox);
        schedulePage.Controls.Add(startTimeLabel);
        schedulePage.Controls.Add(startTimeBox);
        schedulePage.Controls.Add(taskEnabledBox);
        schedulePage.Controls.Add(taskDetailsBox);
        schedulePage.Controls.Add(newTaskButton);
        schedulePage.Controls.Add(deleteTaskButton);
        schedulePage.Controls.Add(refreshTaskButton);
        schedulePage.Controls.Add(loadTaskButton);
        schedulePage.Controls.Add(runTaskButton);
        schedulePage.Controls.Add(saveTaskButton);
        schedulePage.Controls.Add(toggleTaskButton);
        schedulePage.Location = new Point(4, 24);
        schedulePage.Name = "schedulePage";
        schedulePage.Size = new Size(1072, 276);
        schedulePage.TabIndex = 4;
        schedulePage.Text = "Scheduled imports";
        schedulePage.UseVisualStyleBackColor = true;
        // 
        // taskListLabel
        // 
        taskListLabel.Location = new Point(12, 12);
        taskListLabel.Name = "taskListLabel";
        taskListLabel.Size = new Size(260, 23);
        taskListLabel.TabIndex = 0;
        taskListLabel.Text = "Saved tasks";
        // 
        // taskListBox
        // 
        taskListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        taskListBox.DisplayMember = "Name";
        taskListBox.IntegralHeight = false;
        taskListBox.ItemHeight = 15;
        taskListBox.Location = new Point(12, 37);
        taskListBox.Name = "taskListBox";
        taskListBox.Size = new Size(298, 185);
        taskListBox.TabIndex = 1;
        taskListBox.SelectedIndexChanged += TaskSelectionChanged;
        // 
        // taskNameLabel
        // 
        taskNameLabel.Location = new Point(328, 12);
        taskNameLabel.Name = "taskNameLabel";
        taskNameLabel.Size = new Size(230, 23);
        taskNameLabel.TabIndex = 2;
        taskNameLabel.Text = "Task name";
        // 
        // taskNameBox
        // 
        taskNameBox.Location = new Point(328, 37);
        taskNameBox.Name = "taskNameBox";
        taskNameBox.Size = new Size(230, 23);
        taskNameBox.TabIndex = 3;
        // 
        // intervalLabel
        // 
        intervalLabel.Location = new Point(571, 12);
        intervalLabel.Name = "intervalLabel";
        intervalLabel.Size = new Size(121, 23);
        intervalLabel.TabIndex = 4;
        intervalLabel.Text = "Interval in minutes";
        // 
        // intervalBox
        // 
        intervalBox.Location = new Point(571, 37);
        intervalBox.Maximum = new decimal(new int[] { 1439, 0, 0, 0 });
        intervalBox.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        intervalBox.Name = "intervalBox";
        intervalBox.Size = new Size(112, 23);
        intervalBox.TabIndex = 5;
        intervalBox.Value = new decimal(new int[] { 60, 0, 0, 0 });
        // 
        // startTimeLabel
        // 
        startTimeLabel.Location = new Point(697, 12);
        startTimeLabel.Name = "startTimeLabel";
        startTimeLabel.Size = new Size(202, 23);
        startTimeLabel.TabIndex = 6;
        startTimeLabel.Text = "First run (local time)";
        // 
        // startTimeBox
        // 
        startTimeBox.CustomFormat = "yyyy-MM-dd HH:mm";
        startTimeBox.Format = DateTimePickerFormat.Custom;
        startTimeBox.Location = new Point(697, 37);
        startTimeBox.Name = "startTimeBox";
        startTimeBox.Size = new Size(195, 23);
        startTimeBox.TabIndex = 7;
        // 
        // taskEnabledBox
        // 
        taskEnabledBox.Checked = true;
        taskEnabledBox.CheckState = CheckState.Checked;
        taskEnabledBox.Location = new Point(905, 37);
        taskEnabledBox.Name = "taskEnabledBox";
        taskEnabledBox.Size = new Size(148, 28);
        taskEnabledBox.TabIndex = 8;
        taskEnabledBox.Text = "Schedule enabled";
        // 
        // taskDetailsBox
        // 
        taskDetailsBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        taskDetailsBox.BackColor = Color.White;
        taskDetailsBox.Location = new Point(328, 74);
        taskDetailsBox.Multiline = true;
        taskDetailsBox.Name = "taskDetailsBox";
        taskDetailsBox.ReadOnly = true;
        taskDetailsBox.ScrollBars = ScrollBars.Vertical;
        taskDetailsBox.Size = new Size(725, 148);
        taskDetailsBox.TabIndex = 9;
        // 
        // newTaskButton
        // 
        newTaskButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        newTaskButton.BackColor = Color.White;
        newTaskButton.FlatStyle = FlatStyle.Flat;
        newTaskButton.ForeColor = Color.FromArgb(0, 51, 102);
        newTaskButton.Location = new Point(12, 230);
        newTaskButton.Name = "newTaskButton";
        newTaskButton.Size = new Size(90, 33);
        newTaskButton.TabIndex = 10;
        newTaskButton.Text = "New task";
        newTaskButton.UseVisualStyleBackColor = false;
        newTaskButton.Click += NewTaskClick;
        // 
        // deleteTaskButton
        // 
        deleteTaskButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        deleteTaskButton.BackColor = Color.White;
        deleteTaskButton.FlatStyle = FlatStyle.Flat;
        deleteTaskButton.ForeColor = Color.FromArgb(0, 51, 102);
        deleteTaskButton.Location = new Point(110, 230);
        deleteTaskButton.Name = "deleteTaskButton";
        deleteTaskButton.Size = new Size(91, 33);
        deleteTaskButton.TabIndex = 11;
        deleteTaskButton.Text = "Delete task";
        deleteTaskButton.UseVisualStyleBackColor = false;
        deleteTaskButton.Click += DeleteTaskClick;
        // 
        // refreshTaskButton
        // 
        refreshTaskButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        refreshTaskButton.BackColor = Color.White;
        refreshTaskButton.FlatStyle = FlatStyle.Flat;
        refreshTaskButton.ForeColor = Color.FromArgb(0, 51, 102);
        refreshTaskButton.Location = new Point(209, 230);
        refreshTaskButton.Name = "refreshTaskButton";
        refreshTaskButton.Size = new Size(101, 33);
        refreshTaskButton.TabIndex = 12;
        refreshTaskButton.Text = "Refresh";
        refreshTaskButton.UseVisualStyleBackColor = false;
        refreshTaskButton.Click += RefreshTasksClick;
        // 
        // loadTaskButton
        // 
        loadTaskButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        loadTaskButton.BackColor = Color.White;
        loadTaskButton.FlatStyle = FlatStyle.Flat;
        loadTaskButton.ForeColor = Color.FromArgb(0, 51, 102);
        loadTaskButton.Location = new Point(328, 230);
        loadTaskButton.Name = "loadTaskButton";
        loadTaskButton.Size = new Size(111, 33);
        loadTaskButton.TabIndex = 13;
        loadTaskButton.Text = "Load settings";
        loadTaskButton.UseVisualStyleBackColor = false;
        loadTaskButton.Click += LoadTaskClick;
        // 
        // runTaskButton
        // 
        runTaskButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        runTaskButton.BackColor = Color.White;
        runTaskButton.FlatStyle = FlatStyle.Flat;
        runTaskButton.ForeColor = Color.FromArgb(0, 51, 102);
        runTaskButton.Location = new Point(448, 230);
        runTaskButton.Name = "runTaskButton";
        runTaskButton.Size = new Size(104, 33);
        runTaskButton.TabIndex = 14;
        runTaskButton.Text = "Run now";
        runTaskButton.UseVisualStyleBackColor = false;
        runTaskButton.Click += RunTaskClick;
        // 
        // saveTaskButton
        // 
        saveTaskButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        saveTaskButton.BackColor = Color.FromArgb(0, 85, 170);
        saveTaskButton.FlatStyle = FlatStyle.Flat;
        saveTaskButton.ForeColor = Color.White;
        saveTaskButton.Location = new Point(563, 230);
        saveTaskButton.Name = "saveTaskButton";
        saveTaskButton.Size = new Size(249, 33);
        saveTaskButton.TabIndex = 15;
        saveTaskButton.Text = "Save current settings to task";
        saveTaskButton.UseVisualStyleBackColor = false;
        saveTaskButton.Click += SaveTaskClick;
        // 
        // toggleTaskButton
        // 
        toggleTaskButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        toggleTaskButton.BackColor = Color.White;
        toggleTaskButton.FlatStyle = FlatStyle.Flat;
        toggleTaskButton.ForeColor = Color.FromArgb(0, 51, 102);
        toggleTaskButton.Location = new Point(823, 230);
        toggleTaskButton.Name = "toggleTaskButton";
        toggleTaskButton.Size = new Size(134, 33);
        toggleTaskButton.TabIndex = 16;
        toggleTaskButton.Text = "Pause / resume";
        toggleTaskButton.UseVisualStyleBackColor = false;
        toggleTaskButton.Click += ToggleTaskClick;
        // 
        // progressBar
        // 
        progressBar.AccessibleName = "Firewall operation progress";
        progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        progressBar.Location = new Point(20, 836);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(900, 21);
        progressBar.TabIndex = 6;
        // 
        // cancelButton
        // 
        cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        cancelButton.BackColor = Color.White;
        cancelButton.Enabled = false;
        cancelButton.FlatStyle = FlatStyle.Flat;
        cancelButton.ForeColor = Color.FromArgb(0, 51, 102);
        cancelButton.Location = new Point(932, 831);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(168, 33);
        cancelButton.TabIndex = 7;
        cancelButton.Text = "Cancel operation";
        cancelButton.UseVisualStyleBackColor = false;
        cancelButton.Click += CancelClick;
        // 
        // statusLabel
        // 
        statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        statusLabel.Location = new Point(20, 862);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(1080, 28);
        statusLabel.TabIndex = 8;
        statusLabel.Text = "Ready";
        // 
        // trayMenu
        // 
        trayMenu.Items.AddRange(new ToolStripItem[] { trayOpenItem, trayExitItem });
        trayMenu.Name = "trayMenu";
        trayMenu.Size = new Size(104, 48);
        // 
        // trayOpenItem
        // 
        trayOpenItem.Name = "trayOpenItem";
        trayOpenItem.Size = new Size(103, 22);
        trayOpenItem.Text = "Open";
        trayOpenItem.Click += OpenFromTray;
        // 
        // trayExitItem
        // 
        trayExitItem.Name = "trayExitItem";
        trayExitItem.Size = new Size(103, 22);
        trayExitItem.Text = "Exit";
        trayExitItem.Click += ExitFromTray;
        // 
        // trayIcon
        // 
        trayIcon.ContextMenuStrip = trayMenu;
        trayIcon.Icon = (Icon)resources.GetObject("trayIcon.Icon");
        trayIcon.Text = "Sophos Firewall Object Manager";
        trayIcon.DoubleClick += OpenFromTray;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(241, 246, 252);
        ClientSize = new Size(1120, 893);
        Controls.Add(banner);
        Controls.Add(connection);
        Controls.Add(transfer);
        Controls.Add(blacklist);
        Controls.Add(tabs);
        Controls.Add(progressBar);
        Controls.Add(cancelButton);
        Controls.Add(statusLabel);
        Font = new Font("Segoe UI", 9F);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(1136, 865);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Sophos Firewall Object Manager";
        banner.ResumeLayout(false);
        connection.ResumeLayout(false);
        connection.PerformLayout();
        transfer.ResumeLayout(false);
        transfer.PerformLayout();
        blacklist.ResumeLayout(false);
        blacklist.PerformLayout();
        tabs.ResumeLayout(false);
        inputPage.ResumeLayout(false);
        inputPage.PerformLayout();
        sourcesPage.ResumeLayout(false);
        sourcesPage.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)sourceLimitBox).EndInit();
        previewPage.ResumeLayout(false);
        previewPage.PerformLayout();
        activityPage.ResumeLayout(false);
        activityPage.PerformLayout();
        schedulePage.ResumeLayout(false);
        schedulePage.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)intervalBox).EndInit();
        trayMenu.ResumeLayout(false);
        ResumeLayout(false);
    }
}

