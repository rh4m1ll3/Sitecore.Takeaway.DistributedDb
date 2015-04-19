namespace Sitecore.Rocks.Plugin.Commands.DistributedDb
{
    using Sitecore.Rocks.Plugin.DistributedDb.Tools;
    using Sitecore.VisualStudio.Commands;

    [Command(Submenu = "Provisioning")]
    public class ProvisionClientCommand : CommandBase
    {
        public ProvisionClientCommand()
        {
            this.Text = "Provision Client (Next)";
            this.Group = "Provisioning";
            this.SortingValue = 3000;
        }

        public override bool CanExecute(object parameter)
        {
            return CommandWrapper.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            CommandWrapper.Execute(parameter, "ProvisionClient");
        }
    }
}