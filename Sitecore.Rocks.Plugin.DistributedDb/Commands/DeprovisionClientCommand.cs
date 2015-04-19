namespace Sitecore.Rocks.Plugin.Commands.DistributedDb
{
    using Sitecore.Rocks.Plugin.DistributedDb.Tools;
    using Sitecore.VisualStudio.Commands;

    [Command(Submenu = "Deprovisioning")]
    public class DeprovisionClientCommand : CommandBase
    {
        public DeprovisionClientCommand()
        {
            this.Text = "Deprovision Client (First)";
            this.Group = "Deprovisioning";
            this.SortingValue = 4000;
        }

        public override bool CanExecute(object parameter)
        {
            return CommandWrapper.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            CommandWrapper.Execute(parameter, "DeprovisionClient");
        }
    }
}