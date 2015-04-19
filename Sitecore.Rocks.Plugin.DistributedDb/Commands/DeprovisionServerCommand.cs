namespace Sitecore.Rocks.Plugin.Commands.DistributedDb
{
    using Sitecore.Rocks.Plugin.DistributedDb.Tools;
    using Sitecore.VisualStudio.Commands;

    [Command(Submenu = "Deprovisioning")]
    public class DeprovisionServerCommand : CommandBase
    {
        public DeprovisionServerCommand()
        {
            this.Text = "Deprovision Server (Next)";
            this.Group = "Deprovisioning";
            this.SortingValue = 5000;
        }

        public override bool CanExecute(object parameter)
        {
            return CommandWrapper.CanExecute(parameter);
        }

        public override void Execute(object parameter)
        {
            CommandWrapper.Execute(parameter, "DeprovisionServer");
        }
    }
}