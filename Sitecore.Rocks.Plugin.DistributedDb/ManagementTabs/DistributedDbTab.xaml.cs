namespace Sitecore.Rocks.Plugin.DistributedDb.ManagementTabs
{
    using Sitecore.VisualStudio.UI.Management;
    using System.Windows;

    [Management("Distributed Databases", 1000)]
    public partial class DistributedDbTab : IManagementItem
    {
        public DistributedDbTab()
        {
            InitializeComponent();
        }

        public SiteManagementContext Context { get; protected set; }

        public bool CanExecute(IManagementContext context)
        {
            // context can be SiteManagementContext or DatabaseManagementContext
            return context is SiteManagementContext;
        }

        public UIElement GetControl(IManagementContext context)
        {
            this.Context = (SiteManagementContext)context;
            return this;
        }

        private void ProvisionServerButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ProvisionClientButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void SynchronizeButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}