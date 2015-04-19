namespace Sitecore.Rocks.Plugin.Commands.DistributedDb
{
    using Sitecore.Rocks.Plugin.DistributedDb.Tools;
    using Sitecore.VisualStudio.Commands;

    [Command(Submenu = "Provisioning")]
    public class ProvisionServerCommand : CommandBase
    {
        public ProvisionServerCommand()
        {
            this.Text = "Provision Server (First)";
            this.Group = "Provisioning";
            this.SortingValue = 2000;
        }

        public override bool CanExecute(object parameter)
        {
            return CommandWrapper.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            CommandWrapper.Execute(parameter, "ProvisionServer");
        }
    }
}